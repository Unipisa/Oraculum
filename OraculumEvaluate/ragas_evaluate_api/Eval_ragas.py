import os
# os.environ["OPENAI_API_KEY"] = "sk-"
from ragas import evaluate
from ragas.metrics import faithfulness, answer_relevancy, context_recall, context_precision
from datasets import load_dataset
import numpy as np
from statistics import harmonic_mean
import json

def corrected_harmonic_mean(data):
    # Filtering out zero values
    non_zero_data = [x for x in data if x != 0]

    # If all values are zero, return 0
    if len(non_zero_data) == 0:
        return 0

    # Calculate harmonic mean of the non-zero values
    hm = harmonic_mean(non_zero_data)

    # Apply corrective factor
    corrected_hm = hm * (len(non_zero_data) / len(data))

    return corrected_hm
def ragasEvaluate(file_json_input, metrics_t=None):
    # fileJsonInput structure:
    #[
    #    {
    #        "question": "",
    #        "ground_truths": [
    #           ""],
    #        "contexts": [
    #           "","",""],
    #        "answer": ""
    #    }
    #]

    # Imposta le metriche predefinite se metrics_t è None

    if metrics_t is None:
        metrics_t = {
                'faithfulness': faithfulness,
                'answer_relevancy': answer_relevancy,
                'context_recall': context_recall,
                'context_precision': context_precision
            }
        if file_json_input and len(file_json_input) > 0 and "ground_truths" not in file_json_input[0]:
              del metrics_t['context_recall']

    if metrics_t and file_json_input and len(file_json_input) > 0 and "ground_truths" not in file_json_input[0] and 'context_recall' in metrics_t:
        print("AAAAAAAAAA")
        raise Exception("Non puoi usare la metrica context_recall se la ground_truths non è presente!")




    with open("file_json_input.json", "w") as outfile: #TODO Evitare la creazione di un file temporaneo!
        json.dump(file_json_input, outfile)

    mydataset = load_dataset("json", data_files="file_json_input.json")

    if os.path.exists("file_json_input.json"): #TODO ELIMINARE QUESTO GESTENDO INPUT SU DATA (TODO SOPRA)
        os.remove("file_json_input.json")

    augmented_data = []
    metric_values = {metric_name: [] for metric_name in metrics_t}

    total_elements = len(mydataset['train'])
    for index, row in enumerate(mydataset['train']):
        print(f"Evaluating {index + 1} of {total_elements}")
        filtered_dataset = mydataset.filter(lambda x: x == row)
        result = evaluate(filtered_dataset['train'], metrics=list(metrics_t.values()))

        for metric_name, metric_function in metrics_t.items():
            metric_values[metric_name].append(result[metric_name])

        row['evaluation'] = result
        augmented_data.append(row)

    mean_metric_values = {metric_name: np.mean(values) for metric_name, values in metric_values.items()}
    ragas_score = corrected_harmonic_mean(list(mean_metric_values.values()))

    final_row = {'ragas_score': ragas_score, **mean_metric_values}
    augmented_data.append(final_row)

    return augmented_data

#Esempio di utilizzo con metriche custom
#selected_metrics = {
#    'faithfulness': faithfulness,
#    'answer_relevancy': answer_relevancy,
#    'context_recall': context_recall
#}

#print(ragasEvaluate(domande ))

