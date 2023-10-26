## models.py

from flask_sqlalchemy import SQLAlchemy
from datetime import datetime

db = SQLAlchemy()

class Output(db.Model):
    """Output model for storing processed data."""

    id = db.Column(db.Integer, primary_key=True)
    output_data = db.Column(db.String, nullable=False)
    timestamp = db.Column(db.DateTime, default=datetime.utcnow)

    def __init__(self, output_data: str):
        self.output_data = output_data

    def __repr__(self):
        return f"<Output {self.id}>"

    def to_dict(self):
        """
        Serialize the Output object to a dictionary.
        This can be useful when returning data in JSON format.
        """
        return {
            'id': self.id,
            'output_data': self.output_data,
            'timestamp': self.timestamp.strftime('%Y-%m-%d %H:%M:%S')
        }
