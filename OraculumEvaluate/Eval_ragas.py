import os
from ragas import evaluate
from ragas.metrics import faithfulness, answer_relevancy, context_recall , answer_similarity , context_precision
from datasets import load_dataset

mydataset=load_dataset("json", data_files="dataset_test.json")

metrics_t = [faithfulness, answer_relevancy, context_recall, answer_similarity, context_precision]

results = evaluate(mydataset['train'], metrics=metrics_t)

print(results)
