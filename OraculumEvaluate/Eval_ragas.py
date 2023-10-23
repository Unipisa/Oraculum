import os
from ragas import evaluate
from ragas.metrics import faithfulness, answer_relevancy, context_recall , answer_similarity , context_precision
from datasets import load_dataset

mydataset=load_dataset("json", data_files="polimiEval1_preprocessed.json")

metrics_t = [faithfulness, answer_relevancy, context_recall, answer_similarity, context_precision]

results = evaluate(mydataset['train'], metrics=metrics_t)

print(results)


# Valutazione di 13 domande aggiornato al 23/10/2023

# {'ragas_score': 0.3916, 'faithfulness': 0.7742, 'answer_relevancy': 0.8751, 
# 'context_recall': 0.2718, 'answer_similarity': 0.8462, 'context_precision': 0.1828}
