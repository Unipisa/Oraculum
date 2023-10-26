## app.py

from flask import Flask, request, jsonify
from data_processor import DataProcessor
from models import db, Output
from config import Config
import json

class FlaskApp:
    """FlaskApp class for setting up and running the Flask app."""

    def __init__(self, config: Config):
        self.app = Flask(__name__)
        self.app.config[config.FLASK_APP_SETTINGS] = config.SECRET_KEY
        self.app.config['SQLALCHEMY_DATABASE_URI'] = config.SQLALCHEMY_DATABASE_URI
        self.app.config['SECURITY_PASSWORD_SALT'] = config.SECURITY_PASSWORD_SALT
        db.init_app(self.app)
        self.data_processor = DataProcessor()

    def run(self):
        """Run the Flask app."""

        @self.app.route('/augment', methods=['POST'])
        def augment():
            """Augment JSON data."""
            input_data = request.get_json()
            augmented_data = self.data_processor.augment_data(input_data)
            output = Output(json.dumps(augmented_data))
            db.session.add(output)
            db.session.commit()
            return jsonify({'message': 'Data augmented successfully'}), 200
        
        @self.app.route('/process', methods=['POST'])
        def evaluate():
            """Evaluate JSON data."""
            input_data = request.get_json()
            evaluated_data = self.data_processor.eval_data(input_data)
            output = Output(evaluated_data)
            db.session.add(output)
            db.session.commit()
            return jsonify({'message': 'Data evaluated successfully'}), 200

        @self.app.route('/retrieve', methods=['GET'])
        def retrieve():
            """Retrieve processed data."""
            outputs = Output.query.all()
            return jsonify([output.to_dict() for output in outputs]), 200

        self.app.run()

if __name__ == "__main__":
    config = Config()
    flask_app = FlaskApp(config)
    flask_app.run()
