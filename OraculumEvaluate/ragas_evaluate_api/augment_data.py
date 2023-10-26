from datasets import load_dataset
import requests
import json, sys, os
import tempfile

def make_request(endpoint_url):
    response = requests.get(endpoint_url)
    if response.status_code == 200:
        return response.json()
    else:
        print(f"Errore nella richiesta per l'URL: {endpoint_url}")
        return None

def augment_json_data(input_data, endpoint: str) -> dict:
    if not isinstance(input_data, str):
        input_json = json.dumps(input_data)
    else:
        input_json = input_data
    # Create a temporary file in the same directory as the script.
    script_dir = os.path.dirname(os.path.abspath(__file__))  # Get script directory
    temp_fd, temp_file_name = tempfile.mkstemp(suffix='.json', dir=script_dir)  # Create a temp file in that directory

    # Write the JSON string to the temporary file.
    with os.fdopen(temp_fd, 'w') as temp_file:
        temp_file.write(input_json)

    # Load the dataset from the temporary file.
    mydataset = load_dataset("json", data_files=temp_file_name)

    # Manually delete the temporary file after usage.
    os.remove(temp_file_name)
    train_dataset = mydataset["train"]
    file_out = []
    total_items = len(train_dataset)
    for index, item in enumerate(train_dataset):
        question = item["question"]
        endpoint_url = f"{endpoint}{question}"
        # print(f"Processing question {index + 1} of {total_items}...", end='\r')
        sys.stdout.write(f"Processing item {index + 1} of {total_items}...\r")
        sys.stdout.flush()
        result = make_request(endpoint_url)

        while result is None:
            result = make_request(endpoint_url)

        content_list = [fact["content"] for fact in result["factsList"]]
        item["contexts"] = content_list
        item["answer"] = result["answer"]
        file_out.append(item)
    print()
    return file_out


##* Example usage:
# input_json_path = "./OraculumEvaluate/polimiEval1.json"
# augmented_data = augment_json_data(input_json_path)

##* Saving the augmented data to an output file
# output_file_name = input_json_path.replace(".json", "_augmented.json")
# with open(output_file_name, "w", encoding="utf-8") as json_file:
#     json.dump(augmented_data, json_file, ensure_ascii=False, indent=4)
