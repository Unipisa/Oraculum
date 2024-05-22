import tempfile
import os

from data_ingestion.audio.audioToText import create_facts_by_AudioVideo


class DataIngestionFromAudioVideo:
    def __init__(self, file, chunk_size, file_type, file_name, hqt):
        self.file = file
        self.chunk_size = chunk_size
        self.file_type = file_type
        self.file_name = file_name
        self.hqt = hqt

    def ingest_data(self):
        # Utilizza la directory temporanea specifica di sistema
        temp_direct = tempfile.gettempdir()
        file_path = os.path.join(tempfile.gettempdir(), self.file.filename)

        self.file.save(file_path)

        return create_facts_by_AudioVideo(
            file_path,
            self.chunk_size,
            self.file_type,
            self.file_name,
            temp_direct,
            self.hqt,
        )
