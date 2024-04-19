from typing import Dict
from Eval_ragas import ragasEvaluate


class DataProcessor:
    @staticmethod
    def eval_data(input_data: Dict) -> Dict:
        return ragasEvaluate(input_data)

