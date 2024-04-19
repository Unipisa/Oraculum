from data_ingestion.documents.unstructuredAndLangchain import create_facts_by_documents
import tempfile
import os


class DataIngestionFromDocuments:
    def __init__(self, file, chunk_size, file_name, hqt):
        self.file = file
        self.chunk_size = chunk_size
        self.file_name = file_name
        self.hqt = hqt

    def ingest_data(self):
        # Utilizza la directory temporanea specifica di sistema
        file_path = os.path.join(tempfile.gettempdir(), self.file.filename)

        self.file.save(file_path)

        return create_facts_by_documents(
            file_path, self.chunk_size, self.file_name, self.hqt
        )
