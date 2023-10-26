# Prova di Utilizzo augmented_data + Eval_ragas



from OraculumEvaluate.augmend_data import augment_json_data
from OraculumEvaluate.Eval_ragas import ragasEvaluate
import json

# Load Json
json_query = json.load(open("../polimiEval1.json"))
print(json_query)
json_augmented = augment_json_data(json_query)

