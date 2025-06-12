from sentence_transformers import SentenceTransformer, util
from deep_translator import GoogleTranslator
from langdetect import detect, LangDetectException

model = SentenceTransformer('paraphrase-multilingual-MiniLM-L12-v2')

attributes = {
    "brand": 0.3,
    "color": 0.2,
    "government": 0.2,
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
    total_score, total_weight = 0, sum(attributes.values())
    for attr, weight in attributes.items():
        t1 = translate_text(str(lost.get(attr, "")))
        t2 = translate_text(str(found.get(attr, "")))
        emb1 = model.encode(t1, convert_to_tensor=True)
        emb2 = model.encode(t2, convert_to_tensor=True)
        similarity = util.pytorch_cos_sim(emb1, emb2).item()
        total_score += similarity * weight
    return total_score / total_weight
