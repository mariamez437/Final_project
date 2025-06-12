from fastapi import FastAPI, UploadFile, File, Form, HTTPException
from fastapi.responses import JSONResponse
from fastapi.staticfiles import StaticFiles
from enum import Enum
from pydantic import BaseModel, validator
import shutil
import uuid
import os
from PIL import Image
import numpy as np
import torch
import faiss
from transformers import CLIPProcessor, CLIPModel
from sentence_transformers import SentenceTransformer, util
from deep_translator import GoogleTranslator
from langdetect import detect, LangDetectException
import json
import base64
from fastapi.middleware.cors import CORSMiddleware

app = FastAPI()


app.mount("/static", StaticFiles(directory="static"), name="static")
app.add_middleware(
    CORSMiddleware, 
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)

clip_processor = CLIPProcessor.from_pretrained("openai/clip-vit-base-patch32")
clip_model = CLIPModel.from_pretrained("openai/clip-vit-base-patch32")
text_model = SentenceTransformer("paraphrase-multilingual-MiniLM-L12-v2")




attributes = {
    "brand": 0.3,
    "color": 0.2,
    "governorate": 0.2,
    "center": 0.15,
    "street": 0.15,
}


def translate_text(text, target_lang="en"):
    if not text or len(text) < 3:
        return text
    try:
        detected_lang = detect(text)
        if detected_lang != target_lang:
            return GoogleTranslator(source='auto', target=target_lang).translate(text)
    except LangDetectException:
        return text
    return text

def calculate_similarity(lost, found):
    total_score = 0
    total_weight = 0  

    for attr, weight in attributes.items():
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



def get_image_embedding(image_path):
    image = Image.open(image_path).convert("RGB")
    inputs = clip_processor(images=image, return_tensors="pt", padding=True)
    with torch.no_grad():
        image_features = clip_model.get_image_features(**inputs)
    return image_features[0].cpu().numpy()


def build_faiss_index(image_folder):
    embeddings = []
    paths = []
    for img in os.listdir(image_folder):
        if img.startswith("."):
            continue
        path = os.path.join(image_folder, img)
        emb = get_image_embedding(path)
        embeddings.append(emb)
        paths.append(path)
    embedding_matrix = np.vstack(embeddings).astype("float32")
    faiss.normalize_L2(embedding_matrix)
    index = faiss.IndexFlatIP(embedding_matrix.shape[1])
    index.add(embedding_matrix)
    return index, paths


def find_most_similar_images_faiss(input_image_path, image_folder, top_k=3):
    emb = get_image_embedding(input_image_path).reshape(1, -1).astype("float32")
    faiss.normalize_L2(emb)
    index, paths = build_faiss_index(image_folder)
    distances, indices = index.search(emb, top_k)
    results = []
    for i in range(top_k):
        result = {
            "image_path": paths[indices[0][i]],
            "similarity": float(distances[0][i])
        }
        results.append(result)
    return results

async def save_to_file_system(prefix, image_dir, json_path, json_key,
                                governorate, city, street, contact,
                            brand, color, image, image_name):
    os.makedirs(image_dir, exist_ok=True)
    os.makedirs(os.path.dirname(json_path), exist_ok=True)

    image_filename = image_name
    image_path = os.path.join(image_dir, image_filename)

    image.file.seek(0)
    with open(image_path, "wb") as buffer:
        shutil.copyfileobj(image.file, buffer)

    if os.path.exists(json_path):
        with open(json_path, "r", encoding="utf-8") as f:
            data = json.load(f)
    else:
        data = {json_key: []}

    data[json_key].append({
        "governorate": governorate,
        "city": city,
        "street": street,
        "contact": contact,
        "brand": brand,
        "color": color,
        "image_url": f"http://localhost:8004/{image_path.replace(os.sep, '/')}"
    })

    with open(json_path, "w", encoding="utf-8") as f:
        json.dump(data, f, ensure_ascii=False, indent=2)

    return {"status": "added", "image": f"http://localhost:8004/{image_path.replace(os.sep, '/')}"}

@app.post("/add_found")
async def add_found(
    governorate: str = Form(...),
    city: str = Form(...),
    street: str = Form(...),
    contact: str = Form(...),
    image_name: str = Form(...),
    brand: str = Form(...),
    color: str = Form(...),
    image: UploadFile = File(None)
):
    return await save_to_file_system(
        prefix="founded",
        image_dir="static/foundedphone",
        json_path="metadata/foundedphone/foundedphone.json",
        json_key="founded",
        governorate=governorate,
        city=city,
        street=street,
        contact=contact,
        brand=brand,
        color=color,
        image=image,
        image_name=image_name
    )

class MatchType(str, Enum):
    text = "text"
    image = "image"
    both = "both"
class Lost(BaseModel):
    governorate: str
    city: str
    street: str
    contact: str
    brand: str
    color: str
    image_name: str
    image: str  

class MatchRequest(BaseModel):
    match_type: MatchType
    lost: Lost




@app.post("/match/")
async def match(request: MatchRequest):
    match_type = request.match_type
    lost = request.lost
    lost_dict = lost.dict()
    image = lost.image

    metadata_path = "metadata/foundedphone/foundedphone.json"
    if not os.path.exists(metadata_path):
        return JSONResponse(status_code=404, content={"error": "foundedphone.json not found"})

    with open(metadata_path, "r", encoding="utf-8") as f:
        data = json.load(f)

    found_list = data.get("founded", [])
    if not found_list:
        return JSONResponse(status_code=404, content={"error": "No found items available"})

    text_score = 0
    image_score = 0
    final_score = 0
    text_best_match = None
    matched_images = []

    temp_path = None  
    try:
        if match_type in ["image", "both"] and image:
            temp_filename = lost.image_name
            UPLOAD_FOLDER = "static/uploads"
            os.makedirs(UPLOAD_FOLDER, exist_ok=True)
            temp_path = os.path.join(UPLOAD_FOLDER, temp_filename)
            with open(temp_path, "wb") as f:
                    f.write(base64.b64decode(image))
                
            IMAGE_FOLDER = "static/foundedphone"
            image_results = find_most_similar_images_faiss(temp_path, IMAGE_FOLDER, top_k=3)
            image_score = image_results[0]["similarity"] if image_results else 0

            for res in image_results:
                image_name = os.path.basename(res["image_path"])
                associated_data = next(
                    (item for item in found_list if os.path.basename(item.get("image_url", "")) == image_name),
                    None
                )
                matched_images.append({
                    "image_url": f"http://localhost:8004/static/foundedphone/{image_name}",
                    "image_similarity": round(res["similarity"], 4),
                    "associated_data": associated_data
                })

        if match_type in ["text", "both"]:
            best_score = -1
            for found_data in found_list:
                score = calculate_similarity(found_data, lost_dict)
                if score > best_score:
                    best_score = score
                    text_best_match = found_data
            text_score = best_score

        if match_type == "text":
            final_score = text_score
        elif match_type == "image":
            final_score = image_score
        elif match_type == "both":
            final_score = (text_score * 0.5) + (image_score * 0.5)

        matched = 1 if final_score >= 0.7 else 0

        return JSONResponse({
            "match_type": match_type,
            "matched": matched,
            "final_score": round(final_score, 4),

            "text_similarity": round(text_score, 4),
            "text_best_match": text_best_match,

            "image_similarity": round(image_score, 4),
            "matched_images": matched_images
        })

    finally:
        if temp_path and os.path.exists(temp_path):
            os.remove(temp_path)