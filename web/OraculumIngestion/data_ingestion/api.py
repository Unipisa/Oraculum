from flask import request, jsonify
from data_ingestion.audio.ingestionFromAudioVideo import DataIngestionFromAudioVideo
from data_ingestion.documents.data_ingestion_from_documents import (
    DataIngestionFromDocuments,
)
from data_ingestion.web_pages.data_ingestion_from_web_pages import (
    DataIngestionFromWebPages,
)
from utils.textToFacts import content_to_facts


def setup_routes(app):
    chunk_size = 800

    @app.route("/", methods=["GET"])
    def health_status():
        return "Server is running and healthy"

    @app.route("/API_facts_from_text", methods=["POST"])
    def api_facts_from_text():
        json_input = request.get_json()

        content = json_input[0]["content"]
        response = content_to_facts(
            content=content, chunk_size=chunk_size, factType="Text"
        )

        if response is None:
            return jsonify({"error": "No facts generated, content too short"}), 400

        if isinstance(response, Exception):
            return str(response), 400

        return jsonify(response)

    @app.route("/API_facts_from_web_pages", methods=["POST"])
    def api_facts_from_web_pages():
        json_input = request.get_json()

        data_ingester = DataIngestionFromWebPages(json_input, chunk_size)
        response = data_ingester.ingest_data()

        if isinstance(response, Exception):
            return str(response), 400

        if response is None:
            return jsonify({"error": "No facts generated"}), 400

        return jsonify(response)

    @app.route("/API_facts_from_documents", methods=["POST"])
    def api_facts_from_document():
        hqt = request.args.get("hqt", "true").lower() in ["true", "1"]
        if "file" not in request.files:
            return jsonify({"error": "No file received"}), 400

        file = request.files["file"]

        file_name = file.filename.rsplit(".", 1)[0].lower()

        if file.filename == "":
            return jsonify({"error": "Invalid file name"}), 400

        allowed_extensions = {"pdf", "docx"}
        if (
            "." in file.filename
            and file.filename.rsplit(".", 1)[1].lower() not in allowed_extensions
        ):
            return (
                jsonify(
                    {"error": "Unsupported file format, supported only (.pdf, .docx)"}
                ),
                400,
            )

        data_ingester = DataIngestionFromDocuments(file, chunk_size, file_name, hqt)
        result = data_ingester.ingest_data()

        if isinstance(result, Exception):
            return str(result), 400

        if result is None:
            return jsonify({"error": "No facts generated"}), 400

        return jsonify(result)

    @app.route("/API_facts_from_AudioVideo", methods=["POST"])
    def api_facts_from_audiovideo():
        hqt = request.args.get("hqt", "true").lower() in ["true", "1"]
        if "file" not in request.files:
            return jsonify({"error": "No file received"}), 400

        file = request.files["file"]

        if file.filename == "":
            return jsonify({"error": "Invalid file name"}), 400

        allowed_extensions = {"mp3", "mp4"}
        if (
            "." in file.filename
            and file.filename.rsplit(".", 1)[1].lower() not in allowed_extensions
        ):
            return (
                jsonify({"error": "Unsupported file format, supported only (mp3,mp4)"}),
                400,
            )

        file_extension = file.filename.rsplit(".", 1)[1].lower()

        file_name = file.filename.rsplit(".", 1)[0].lower()

        file_type = None
        if file_extension in allowed_extensions:
            file_type = file_extension

        data_ingester = DataIngestionFromAudioVideo(
            file, chunk_size, file_type, file_name, hqt
        )
        result = data_ingester.ingest_data()

        if isinstance(result, Exception):
            return str(result), 400

        if result is None:
            return (
                jsonify({"error": "No facts generated, error in audio transcription"}),
                400,
            )

        return jsonify(result)
