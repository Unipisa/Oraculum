FROM python:3.9 AS base

WORKDIR /app
EXPOSE 80

RUN python -m venv venv
ENV PATH="/app/venv/bin:$PATH"

COPY ["web/OraculumEvaluate/ragas_evaluate_api/.", "/app"]

RUN pip install --no-cache-dir -r requirements.txt

FROM base AS bundle
WORKDIR /app

CMD ["python", "app.py"]
