import os
# os.environ["OPENAI_API_KEY"] = "sk-"
from ragas import evaluate
from ragas.metrics import faithfulness, answer_relevancy, context_recall , context_precision
from datasets import load_dataset
import json
import pandas as pd

filename = 'polimiEval1_preprocessed'
path = f'{filename}.json'
mydataset=load_dataset("json", data_files=path)

metrics_t = [faithfulness, answer_relevancy, context_recall, context_precision]


augmented_data = []

total_elements = len(mydataset['train'])
for index, row in enumerate(mydataset['train']):
    print(f"Evaluating {index + 1} of {total_elements}")
    
    filtered_dataset = mydataset.filter(lambda x: x == row)
    
    result = evaluate(filtered_dataset['train'], metrics=metrics_t)
    print(result)

    row['evaluation'] = result
    augmented_data.append(row)

print('Evaluating dataset')
#* Calcolare results senza rifare evaluate di tutto il dataset
# results = evaluate(mydataset['train'], metrics=metrics_t)
# print(results)
# augmented_data.append({"ground_truths": "Dataset evaluation", "evaluation": results})

with open("./OraculumEvaluate/augmented_polimiEval1_preprocessed.json", "w", encoding='UTF-8') as outfile:
    json.dump(augmented_data, outfile)
    df = pd.json_normalize(augmented_data)
    df.to_excel(f'./OraculumEvaluate/{filename}.xlsx', index=False, engine='openpyxl')


# if len(values) > 1:
#             reciprocal_sum = np.sum(1.0 / np.array(values))  # type: ignore
#             self["ragas_score"] = len(values) / reciprocal_sum



# Valutazione di 13 domande aggiornato al 23/10/2023

# {'ragas_score': 0.3916, 'faithfulness': 0.7742, 'answer_relevancy': 0.8751, 
# 'context_recall': 0.2718, 'answer_similarity': 0.8462, 'context_precision': 0.1828}
