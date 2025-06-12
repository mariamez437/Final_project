from transformers import CLIPProcessor, CLIPModel
import torch
from PIL import Image
import os
import numpy as np
import faiss

processor = CLIPProcessor.from_pretrained("openai/clip-vit-base-patch32")
model = CLIPModel.from_pretrained("openai/clip-vit-base-patch32")

def get_image_embedding(image_path):
    image = Image.open(image_path).convert("RGB")
    inputs = processor(images=image, return_tensors="pt", padding=True)
    with torch.no_grad():
        image_features = model.get_image_features(**inputs)
    embedding = image_features[0].cpu().numpy()
    return embedding, image

def build_faiss_index(image_folder):
    embeddings, image_paths = [], []

    for image_name in os.listdir(image_folder):
        if image_name.startswith(".") or os.path.isdir(os.path.join(image_folder, image_name)):
            continue
        path = os.path.join(image_folder, image_name)
        emb, _ = get_image_embedding(path)
        embeddings.append(emb)
        image_paths.append(path)

    embedding_matrix = np.vstack(embeddings).astype("float32")
    faiss.normalize_L2(embedding_matrix)

    index = faiss.IndexFlatIP(embedding_matrix.shape[1])
    index.add(embedding_matrix)
    return index, image_paths

def find_most_similar_images_faiss(input_image_path, image_folder, top_k=2):
    input_emb, input_image = get_image_embedding(input_image_path)
    input_emb = input_emb.reshape(1, -1).astype("float32")
    faiss.normalize_L2(input_emb)

    index, image_paths = build_faiss_index(image_folder)
    distances, indices = index.search(input_emb, top_k)

    results = []
    for i in range(top_k):
        path = image_paths[indices[0][i]]
        similarity = distances[0][i]
        image = Image.open(path)
        results.append((path, similarity, image))
    return input_image, results

