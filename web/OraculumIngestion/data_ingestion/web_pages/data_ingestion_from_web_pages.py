from data_ingestion.web_pages.CreateFact_by_Langchain_with_html import create_facts_by_lang_with_html


class DataIngestionFromWebPages:
    def __init__(self, json_input,chunk_size):
        self.json_input = json_input
        self.chunk_size = chunk_size

    def ingest_data(self):
        return create_facts_by_lang_with_html(self.json_input, self.chunk_size)