from sentence_transformers import SentenceTransformer, util
from deep_translator import GoogleTranslator
from langdetect import detect, LangDetectException
from deepface import DeepFace
from PIL import Image
import tempfile
import torch
from face_detection import detect_and_crop_face  

class Matcher:
    def __init__(self):
        self.text_model = SentenceTransformer('paraphrase-MiniLM-L6-v2')

        self.weights = {
            "name": 0.4,
            "national_id": 0.3,
            "birth_date": 0.1,
            "governorate": 0.1,
            "city": 0.05,
            "street": 0.05
        }

    def translate_text(self, text, target_lang="en"):
        if not text or len(text) < 3:
            return text
        try:
            detected_lang = detect(text)
            if detected_lang != target_lang:
                return GoogleTranslator(source='auto', target=target_lang).translate(text)
        except LangDetectException:
            return text
        return text

    def calculate_text_similarity(self, lost, found):
        total_score = 0
        for attr, weight in self.weights.items():
            text1 = self.translate_text(str(lost.get(attr, "")))
            text2 = self.translate_text(str(found.get(attr, "")))

            embedding1 = self.text_model.encode(text1, convert_to_tensor=True)
            embedding2 = self.text_model.encode(text2, convert_to_tensor=True)
            similarity = util.pytorch_cos_sim(embedding1, embedding2).item()
            total_score += similarity * weight
        return total_score / sum(self.weights.values())

    def compare_faces(self, lost_path: str, found_path: str):
        try:
            face1_path = detect_and_crop_face(lost_path, "static/faces/lost_face.jpg")
            face2_path = detect_and_crop_face(found_path, "static/faces/found_face.jpg")

            if not face1_path or not face2_path:
                return False, 1.0, None  # Face not detected

            result = DeepFace.verify(
                img1_path=face1_path,
                img2_path=face2_path,
                model_name='ArcFace',
                distance_metric='euclidean_l2',
                threshold=0.3
            )

            return result["verified"], result["distance"], face1_path, face2_path

        except Exception as e:
            print(f"[Face Matching Error]: {e}")
            return False, 1.0, None, None

    def match(self, lost, found, match_type="both"):
        text_score = None
        face_score = None
        verified = None

        if match_type in ["text", "both"]:
            text_score = self.calculate_text_similarity(lost, found)

        if match_type in ["image", "both"]:
            verified, face_score, face1_path, face2_path = self.compare_faces(lost["image_path"], found["image_path"])

        if match_type == "text":
            final_score = text_score
        elif match_type == "image":
            final_score = 1 - face_score 
        elif match_type == "both":
            
            final_score = 0.6 * text_score + 0.4 * (1 - face_score)
        else:
            raise ValueError("Invalid match_type")

        return {
            "score": final_score,
            "text_score": text_score,
            "face_score": face_score,
            "face_verified": verified
        }
