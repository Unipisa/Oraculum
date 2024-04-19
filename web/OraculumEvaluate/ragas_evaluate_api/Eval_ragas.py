import os
from config import model
from ragas import evaluate
from ragas.metrics import (
    faithfulness,
    answer_relevancy,
    context_recall,
    context_precision,
    answer_correctness,
    answer_similarity,
)
from datasets import load_dataset
import numpy as np
from statistics import harmonic_mean
import json
from langchain.chat_models import ChatOpenAI
import numpy as np

gpt = ChatOpenAI(model_name=model)


def convert_numbered_properties_to_arrays(data, result):
    try:
        data = json.loads(data)
        converted_data = {}
        for key, value in data.items():        
            if isinstance(value, dict):
                converted_data[key] = [
                    value[str(i)] if value[str(i)] is not None else -1
                    for i in range(len(value))
                ]
            else:
                converted_data[key] = value if value is not None else -1

        for key, value in result.items():
                if value is None or np.isnan(value) or value == "nan":
                    result[key] = -1

        converted_data["mean_metrics"] = result

        return converted_data
    except Exception as e:
        return str(e), 500


def ragasEvaluate(file_json_input, metrics_t=None):

    try:
        if metrics_t is None:
            metrics_t = {
                "faithfulness": faithfulness,
                "answer_relevancy": answer_relevancy,
                "context_recall": context_recall,
                "context_precision": context_precision,
                "answer_correctness": answer_correctness,
                "answer_similarity": answer_similarity,
            }
            if (
                file_json_input
                and len(file_json_input) > 0
                and "ground_truth" not in file_json_input
            ):
                del metrics_t["context_recall"]

        if (
            metrics_t
            and file_json_input
            and len(file_json_input) > 0
            and "ground_truth" not in file_json_input
            and "context_recall" in metrics_t
        ):
            raise Exception(
                "Non puoi usare la metrica context_recall se la ground_truth non è presente!"
            )

        with open(
            "file_json_input.json", "w"
        ) as outfile:  # TODO Evitare la creazione di un file temporaneo!
            json.dump(file_json_input, outfile)

        mydataset = load_dataset("json", data_files="file_json_input.json")

        if os.path.exists(
            "file_json_input.json"
        ):  # TODO ELIMINARE QUESTO GESTENDO INPUT SU DATA (TODO SOPRA)
            os.remove("file_json_input.json")

        augmented_data = []
        result = evaluate(
            mydataset["train"],
            metrics=list(metrics_t.values()),
            llm=gpt,
            raise_exceptions=False,
        )


        augmented_data = convert_numbered_properties_to_arrays(
            result.to_pandas().to_json(), result
        )

        return augmented_data
    
    except Exception as e:
        return str(e), 500

