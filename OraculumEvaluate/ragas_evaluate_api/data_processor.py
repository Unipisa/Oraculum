## data_processor.py

import json
from typing import Dict
from augment_data import augment_json_data
from Eval_ragas import ragasEvaluate

class DataProcessor:
    """DataProcessor class for processing JSON data."""

    @staticmethod
    def augment_data(input_data: Dict, endpoint: str = "http://localhost:5009/api/v1/FrontOffice/getanswer/debug/") -> Dict:
        return augment_json_data(input_data, endpoint)
    
    @staticmethod
    def eval_data(input_data: Dict) -> Dict:
        return ragasEvaluate(input_data)