from flask import Flask, request, jsonify
import torch
from transformers import AutoTokenizer, AutoModel

# Load SciBERT model and tokenizer
tokenizer = AutoTokenizer.from_pretrained("allenai/scibert_scivocab_uncased")
model = AutoModel.from_pretrained("allenai/scibert_scivocab_uncased")

app = Flask(__name__)

def get_keywords_and_vector(text):
    inputs = tokenizer(text, return_tensors='pt', truncation=True, padding=True)
    outputs = model(**inputs)
    hidden_states = outputs.last_hidden_state

    # Get the token vectors
    token_vecs = hidden_states[0]
    
    # Calculate the average vector
    avg_vec = torch.mean(token_vecs, dim=0)
    
    # Extract keywords
    keywords = [tokenizer.decode(token_id) for token_id in inputs.input_ids[0]]
    
    return keywords, avg_vec.detach().numpy().tolist()

@app.route('/get_keywords_and_vector', methods=['POST'])
def get_keywords_and_vector_api():
    data = request.json
    text = data.get('text', '')
    if not text:
        return jsonify({"error": "Text is required"}), 400
    
    keywords, vector = get_keywords_and_vector(text)
    return jsonify({"keywords": keywords, "vector": vector})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5002)
