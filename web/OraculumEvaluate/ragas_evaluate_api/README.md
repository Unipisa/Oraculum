# RAGAS Evaluation API

## Requirements

python --version --> Python 3.10.11

## Local Installation
To install the necessary dependencies, execute the following command:
```console
python -m venv venv
venv\Scripts\activate 
pip install -r requirements.txt
```
edit in app.py self.app.run(host="0.0.0.0", port=80) TO self.app.run(host="localhost", port=5000)
# Local Use

python app.py


## Local Get Started

Swagger: http://localhost:5000/api/docs

### End-point "\evaluate"
used for standard evaluation
#### Input
```console
[
  {
    "question": "question",
    "answer": "answer",
    "ground_truth": "ground_truth",
    "contexts": [
      "contexts"
    ]
  }
]
```

"question" = The question we pose to the assistant
"answer" = The response provided by the assistant that we want to evaluate
"ground_truth" = The expected/correct response to the posed question
"contexts" = The knowledge used for generating the response

To perform quantitative evaluation, we use the Ragas framework.

### "\evaluate" returns a json object with evaluation metrics 
(Answer Relevancy,Faithfulness,Answer Correctness,Context Precision,Context Recall,Context Relevancy, mean metrics)


### "/evaluateDebug"
Endpoint for debugging tests, not included in the swagger.
NB: it is necessary to change the address to "API_debug" in the credential.py file.
Save the results in files: ./AUGMENTED.json and ./Output_evalX.json where X is the batch number.
#### Input
```console
[
    {
        "question": "question",
        "ground_truth": "ground_truth"
    }
]
```

# Ragas
link= https://docs.ragas.io/en/stable/

Ragas is a framework that helps you evaluate your Retrieval Augmented Generation (RAG) pipelines.

![Testo alternativo](https://docs.ragas.io/en/stable/_static/imgs/component-wise-metrics.png)

The metrics considered are:


## Generation

### Answer Relevancy
The evaluation metric, Answer Relevancy, focuses on assessing how pertinent the generated answer is to the given prompt. 
A lower score is assigned to answers that are incomplete or contain redundant information and higher scores indicate better relevancy. 
####  This metric is computed using the question, the context and the answer.

### Faithfulness
This measures the factual consistency of the generated answer against the given context. 
#### It is calculated from answer and retrieved context. 
The generated answer is regarded as faithful if all the claims that are made in the answer can be inferred from the given context.

### Answer Correctness
The assessment of Answer Correctness involves gauging the accuracy of the generated answer when compared to the ground truth. 
#### This evaluation relies on the ground truth and the answer
A higher score indicates a closer alignment between the generated answer and the ground truth, signifying better correctness.


## Retrieval

### Context Precision
Context Precision is a metric that evaluates whether all of the ground-truth relevant items present in the contexts are ranked higher or not. 
Ideally all the relevant chunks must appear at the top ranks. 
#### This metric is computed using the question, ground_truth and the contexts.

### Context Recall
Context recall measures the extent to which the retrieved context aligns with the annotated answer, treated as the ground truth. 
#### It is computed based on the ground truth and the retrieved context.
To estimate context recall from the ground truth answer, each sentence in the ground truth answer is analyzed to determine whether it can be attributed to the retrieved context or not

### Context Relevancy
This metric gauges the relevancy of the retrieved context. 
#### Calculated based on both the question and contexts. 
Ideally, the retrieved context should exclusively contain essential information to address the provided query.





