# models.py

from flask_sqlalchemy import SQLAlchemy
from datetime import datetime

db = SQLAlchemy()


class Output(db.Model):
    """Output model for storing processed data."""

    id = db.Column(db.Integer, primary_key=True)
    output_data = db.Column(db.String, nullable=False)
    data_type = db.Column(db.String)
    timestamp = db.Column(db.DateTime, default=datetime.utcnow)

    def __init__(self, output_data: str, data_type: str):
        self.output_data = output_data
        self.data_type = data_type

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
            'data_type': self.data_type,
            'timestamp': self.timestamp.strftime('%Y-%m-%d %H:%M:%S')
        }
