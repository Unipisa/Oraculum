"""config.py"""

from typing import Tuple

class Config:
    """Configuration settings for the application."""

    def __init__(self, db_name: str = "ragas_evaluate.db", secret_key: str = "secret"):
        self.DB_NAME = db_name
        self.SECRET_KEY = secret_key

    @property
    def SQLALCHEMY_DATABASE_URI(self) -> str:
        """Database URI for SQLAlchemy."""
        return f"sqlite:///{self.DB_NAME}"

    @property
    def SECURITY_PASSWORD_SALT(self) -> str:
        """Password salt for Flask-Security."""
        return self.SECRET_KEY

    @property
    def FLASK_APP_SETTINGS(self) -> Tuple[str, str]:
        """Flask app settings."""
        return "SECRET_KEY", self.SECRET_KEY
