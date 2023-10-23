from datasets import load_dataset
import requests
import json

file_name_input = "polimiEval1"
mydataset=load_dataset("json", data_files=file_name_input+".json")
# File Structure:
#[
#    {
#        "question": "",
#        "ground_truths": [""]
#    },
#]

train_dataset = mydataset["train"]
file_out = []


# Cicla attraverso il dataset e richiamo End-point
for item in train_dataset:
    domanda = item["question"]
    endpoint_url = f"http://localhost:5009/api/v1/FrontOffice/getanswer/debug/{domanda}"
    
    response = requests.get(endpoint_url)

    if response.status_code == 200: # Tutto ok!
        result = response.json()
        content_list = [fact["content"] for fact in result["factsList"]]
        item["contexts"] = content_list
        file_out.append(item)
        item["answer"] = result["answer"]
    else:
        print(f"Errore nella richiesta per la domanda: {domanda}")
    

with open(file_name_input+"_preprocessed.json", "w", encoding="utf-8") as json_file:
    json.dump(file_out, json_file, ensure_ascii=False, indent=4)


