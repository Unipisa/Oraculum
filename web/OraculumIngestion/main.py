from data_ingestion.api import setup_routes
from flask import Flask
from flask_swagger_ui import get_swaggerui_blueprint

app = Flask(__name__)

SWAGGER_URL = "/swagger"
API_URL = "/static/swagger.json"

swagger_ui_blueprint = get_swaggerui_blueprint(
    SWAGGER_URL, API_URL, config={"DataIngestion": "DataIngestion Access API"}
)

if __name__ == "__main__":
    try:

        setup_routes(app)
        app.register_blueprint(swagger_ui_blueprint, url_prefix=SWAGGER_URL)
        app.run(host="0.0.0.0", port=80)

    except Exception as e:
        print(f"Error: {e}")
        raise e
