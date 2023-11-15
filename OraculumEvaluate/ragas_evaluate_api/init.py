from flask import Flask
from models import db, Output

app = Flask(__name__)
app.config['SQLALCHEMY_DATABASE_URI'] = 'sqlite:///ragas_evaluate.db'
db.init_app(app)

with app.app_context():
    db.create_all()