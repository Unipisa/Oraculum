from datasets import load_dataset
import requests
import json

file_name_input = "polimiEval1"
path = f'{file_name_input}.json'
#path = f'./OraculumEvaluate/{file_name_input}.json'


mydataset = load_dataset("json", data_files=path)
# File Structure:
#[
#    {
#        "question": "",
#        "ground_truths": [""]
#    },
#]
train_dataset = mydataset["train"]
file_out = []

def make_request(endpoint_url):
    response = requests.get(endpoint_url)
    if response.status_code == 200:
        return response.json()
    else:
        print(f"Errore nella richiesta per l'URL: {endpoint_url}")
        return None

def Augmented_data(getanswerURLAPI):
    for item in train_dataset:
        domanda = item["question"]
        endpoint_url = f"{getanswerURLAPI}{domanda}"

        result = make_request(endpoint_url)

        while result is None:
            result = make_request(endpoint_url)

        content_list = [fact["content"] for fact in result["factsList"]]
        item["contexts"] = content_list
        item["answer"] = result["answer"]
        file_out.append(item)
    return file_out


getanswerURLAPI = "http://localhost:5009/api/v1/FrontOffice/getanswer/debug/"
file_out = Augmented_data(getanswerURLAPI)
with open(file_name_input + "_augmented.json", "w", encoding="utf-8") as json_file:
    json.dump(file_out, json_file, ensure_ascii=False, indent=4)

