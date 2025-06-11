import torch
from ultralytics import YOLO
from PIL import Image
from huggingface_hub import hf_hub_download
import os

model_path = hf_hub_download(repo_id="arnabdhar/YOLOv8-Face-Detection", filename="model.pt")
face_model = YOLO(model_path)

def detect_and_crop_face(image_path, output_path):
    img = Image.open(image_path).convert("RGB")
    result = face_model(img, classes=[0])
    boxes = result[0].boxes.xyxy.cpu().numpy()
    scores = result[0].boxes.conf.cpu().numpy()

    threshold = 0.5
    for i, score in enumerate(scores):
        if score > threshold:
            x1, y1, x2, y2 = map(int, boxes[i])
            face = img.crop((x1, y1, x2, y2))

            os.makedirs(os.path.dirname(output_path), exist_ok=True)
            face.save(output_path)
            return output_path

    return None
