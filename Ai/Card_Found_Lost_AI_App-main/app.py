from fastapi import FastAPI, File, UploadFile, Form
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from fastapi.staticfiles import StaticFiles
from sentence_transformers import SentenceTransformer, util
from deep_translator import GoogleTranslator
from langdetect import detect, LangDetectException
from ultralytics import YOLO
from huggingface_hub import hf_hub_download
from deepface import DeepFace
from PIL import Image
from enum import Enum
import shutil
import os
import json
import uuid


app = FastAPI()
app.mount("/static", StaticFiles(directory="static"), name="static")
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

text_model = SentenceTransformer('paraphrase-MiniLM-L12-v2')
yolo_model_path = hf_hub_download(repo_id="arnabdhar/YOLOv8-Face-Detection", filename="model.pt")
face_model = YOLO(yolo_model_path)

attribute_weights = {
    "name": 0.5,
    "national_id": 0.3,
    "governorate": 0.1,
    "city": 0.05,
    "street": 0.05
}

THRESHOLD = 0.75



def translate_text(text, target_lang="en"):
    try:
        if not text or len(text) < 3:
            return text
        detected_lang = detect(text)
        if detected_lang != target_lang:
            return GoogleTranslator(source='auto', target=target_lang).translate(text)
    except LangDetectException:
        return text
    return text

def calculate_similarity(lost, found):
    total_score = 0
    total_weight = 0  

    for attr, weight in attribute_weights.items():
        if attr in lost and attr in found:
            text1 = translate_text(str(lost[attr]))
            text2 = translate_text(str(found[attr]))
            embedding1 = text_model.encode(text1, convert_to_tensor=True)
            embedding2 = text_model.encode(text2, convert_to_tensor=True)
            similarity = util.pytorch_cos_sim(embedding1, embedding2).item()
            total_score += similarity * weight
            total_weight += weight

    if total_weight == 0:
        return 0 

    return total_score / total_weight

def detect_and_crop_face(image_path, prefix="face"):
    img = Image.open(image_path).convert("RGB")
    result = face_model(img, classes=[0])
    boxes = result[0].boxes.xyxy.cpu().numpy()
    scores = result[0].boxes.conf.cpu().numpy()
    for i, score in enumerate(scores):
        if score > 0.5:
            x1, y1, x2, y2 = map(int, boxes[i])
            face = img.crop((x1, y1, x2, y2))
            output_path = f"static/faces/{prefix}_face.jpg"
            os.makedirs("static/faces", exist_ok=True)
            face.save(output_path)
            return output_path
    return None

async def save_to_file_system(prefix, image_dir, json_path, json_key,
                                name, national_id, governorate, city, street, contact, image, image_name):
    try:
        os.makedirs(image_dir, exist_ok=True)
        os.makedirs(os.path.dirname(json_path), exist_ok=True)

        face_filename = image_name
        face_path = os.path.join(image_dir, face_filename)
        
        # التعامل مع الصورة إذا كانت موجودة
        if image and hasattr(image, 'file'):
            # حفظ الصورة الأصلية
            image.file.seek(0)
            with open(face_path, "wb") as buffer:
                shutil.copyfileobj(image.file, buffer)

            # معالجة اكتشاف الوجه
            temp_path = f"temp_{uuid.uuid4().hex}.jpg"
            image.file.seek(0)
            with open(temp_path, "wb") as f:
                shutil.copyfileobj(image.file, f)

            try:
                cropped = detect_and_crop_face(temp_path, prefix=face_filename.replace(".jpg", ""))
                if cropped:
                    shutil.copy(cropped, face_path)
                else:
                    # إذا لم يتم اكتشاف وجه، استخدم الصورة الأصلية
                    shutil.move(temp_path, face_path)
            except Exception as e:
                print(f"Face detection error: {e}")
                # في حالة فشل اكتشاف الوجه، استخدم الصورة الأصلية
                shutil.move(temp_path, face_path)
            finally:
                # تنظيف الملف المؤقت
                if os.path.exists(temp_path):
                    os.remove(temp_path)
        else:
            # إذا لم تكن هناك صورة، أنشئ ملف placeholder
            print("No image provided, creating placeholder")
            placeholder_path = face_path
            with open(placeholder_path, "w") as f:
                f.write("No image")

        # تحميل بيانات JSON
        if os.path.exists(json_path):
            with open(json_path, "r", encoding="utf-8") as f:
                data = json.load(f)
        else:
            data = {json_key: []}

        # إضافة البيانات الجديدة
        new_entry = {
            "name": name or "",
            "national_id": national_id or "",
            "governorate": governorate or "",
            "city": city or "",
            "street": street or "",
            "contact": contact or "",
            "image_url": f"http://localhost:8003/{face_path.replace(os.sep, '/')}" if os.path.exists(face_path) else None
        }
        
        data[json_key].append(new_entry)

        # حفظ البيانات
        with open(json_path, "w", encoding="utf-8") as f:
            json.dump(data, f, ensure_ascii=False, indent=2)

        return {
            "status": "added", 
            "image": f"http://localhost:8003/{face_path.replace(os.sep, '/')}" if os.path.exists(face_path) else None,
            "data": new_entry
        }
        
    except Exception as e:
        print(f"Error in save_to_file_system: {str(e)}")
        raise Exception(f"Failed to save data: {str(e)}")
    
@app.post("/add_lost")
async def add_lost(
    name: str = Form(""),        
    national_id: str = Form(""),    
    governorate: str = Form(""),    
    city: str = Form(""),          
    street: str = Form(""),        
    contact: str = Form(""),       
    image_name: str = Form(""),    
    image: UploadFile = File(None)
):
    return await save_to_file_system(
        prefix="losted",
        image_dir="static/lostedcard",
        json_path="metadata/lostedcard/lostedcard.json",
        json_key="losted",
        name=name,
        national_id=national_id,
        governorate=governorate,
        city=city,
        street=street,
        contact=contact,
        image=image,
        image_name=image_name
    )
@app.post("/add_found")
async def add_found(
    name: str = Form(""),          
    national_id: str = Form(""),    
    governorate: str = Form(""),   
    city: str = Form(""),          
    street: str = Form(""),       
    contact: str = Form(""),       
    image_name: str = Form(""),   
    image: UploadFile = File(None)  
):
    return await save_to_file_system(
        prefix="founded",
        image_dir="static/foundedcard",
        json_path="metadata/foundedcard/foundedcard.json",
        json_key="founded",
        name=name,
        national_id=national_id,
        governorate=governorate,
        city=city,
        street=street,
        contact=contact,
        image=image,
        image_name=image_name
    )

THRESHOLD = 0.6
class MatchType(str, Enum):
    text = "text"
    image = "image"
    both = "both"

class Lost(BaseModel):
    name: str
    national_id: str
    governorate: str
    city: str
    street: str
    contact: str
    image_name: str

class MatchRequest(BaseModel):
    match_type: MatchType
    lost: Lost
@app.post("/match/")
async def match(request: MatchRequest):   
    match_type = request.match_type
    lost = request.lost
    lost_dict = lost.dict()

    metadata_path = "metadata/foundedcard/foundedcard.json"
    if not os.path.exists(metadata_path):
        return {"error": "foundedcard.json not found"}

    with open(metadata_path, "r", encoding="utf-8") as f:
        data = json.load(f)

    found_list = data.get("founded", [])

    best_score = -1
    text_best_match = None
    image_best_match = None
    text_score = None
    face_verified = None
    face_distance = None
    lost_face_url = None
    found_face_url = None
 
    if match_type in ["text", "both"]:
        for found_data in found_list:
            score = calculate_similarity(lost_dict, found_data)
            if score > best_score:
                best_score = score
                text_best_match = found_data
        text_score = best_score

    if match_type in ["image", "both"]:
        lost_img_path = os.path.join("static/lostedcard", lost.image_name)
        found_images_dir = "static/foundedcard/"
        found_images = os.listdir(found_images_dir)

        for found_img_name in found_images:
            found_img_path = os.path.join(found_images_dir, found_img_name)
            try:
                result = DeepFace.verify(
                    img1_path=lost_img_path,
                    img2_path=found_img_path,
                    model_name="Facenet512",
                    distance_metric="euclidean_l2",
                    threshold=0.7
                )
                if result["verified"]:
                    face_verified = True
                    face_distance = result["distance"]
                    lost_face_url = f"/static/lostedcard/{lost.image_name}"
                    found_face_url = f"/static/foundedcard/{found_img_name}"
                    image_best_match = next(
                        (item for item in found_list if os.path.basename(item.get("image_url", "")) == found_img_name),
                        None
                    )
                    break
            except Exception as e:
                print(f"Face matching error: {e}")
                face_verified = False
                face_distance = None

    # Determine final match
    if match_type == "text":
        best_match = text_best_match
        final_result = text_score is not None and text_score > THRESHOLD
    elif match_type == "image":
        best_match = image_best_match
        final_result = face_verified
    else:  # both
        best_match = image_best_match if face_verified else text_best_match
        final_result = (text_score is not None and text_score > THRESHOLD) and face_verified

    if best_match is None:
        final_result = False

     
    return {

        "text_similarity": round(text_score, 4) if text_score is not None else None,
        "face_verified": face_verified,
        "face_distance": face_distance,
        "match_result": final_result,
        "face_images": {
            "lost_face": f"http://localhost:8003{lost_face_url}" if lost_face_url else None,
            "found_face": f"http://localhost:8003{found_face_url}" if found_face_url else None
        },
        "contact_info": {
            "found": best_match.get("contact") if best_match else None
        }
    }