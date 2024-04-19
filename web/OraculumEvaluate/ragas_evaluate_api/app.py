## app.py
from flask import Flask, request, jsonify, send_file
from Eval_ragas import ragasEvaluate
from data_processor import DataProcessor
from flask import Flask, request, jsonify, render_template
import json

# http://localhost:5000/api/docs#/


class FlaskApp:
    """FlaskApp class for setting up and running the Flask app."""

    def __init__(self):
        self.app = Flask(__name__)
        self.data_processor = DataProcessor()

    def run(self):
        """Run the Flask app."""

        @self.app.route("/")
        def get_root():
            return render_template("index.html")

        @self.app.route("/api/docs")
        def get_docs():
            return render_template("swaggerui.html")
        
        
        @self.app.route("/evaluate", methods=["POST"])
        def evaluateNEW():
            try:
                """Augment JSON data."""
                input_data = request.get_json()
                Output_eval = ragasEvaluate(input_data, metrics_t=None)
                return jsonify(Output_eval), 200
            except Exception as e:
                return str(e), 500
        

        self.app.run(host="0.0.0.0", port=80)


if __name__ == "__main__":
    flask_app = FlaskApp()
    flask_app.run()
