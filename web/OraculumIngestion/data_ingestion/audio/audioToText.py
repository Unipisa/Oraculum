from moviepy.editor import *
from moviepy.editor import VideoFileClip
from utils.textToFacts import content_to_facts
from pydub import AudioSegment
from openai import OpenAI
import os
import time


def remove_file_with_retry(file_path, max_attempts=10, delay_seconds=1):
    if os.path.exists(file_path):
        for attempt in range(max_attempts):
            try:
                os.remove(file_path)
                print(f"File {file_path} rimosso con successo.")
                break
            except PermissionError as e:
                print(f"Tentativo {attempt+1} fallito: {e}")
                time.sleep(delay_seconds)
        else:
            print(
                f"Non è stato possibile rimuovere il file {file_path} dopo {max_attempts} tentativi."
            )


def openai_transcriptions(file_path):
    try:
        client = OpenAI()

        with open(file_path, "rb") as audio_file:
            transcript = client.audio.transcriptions.create(
                model="whisper-1",
                file=audio_file,
                response_format="verbose_json",
                timestamp_granularities=["segment"],
            )

        remove_file_with_retry(file_path=file_path)
    except Exception as e:
        print("Error in audio transcription: ", e)
        transcript = None

    return transcript


def segment_audio_citation(transcript, n_segments=10):
    try:
        aug_segment = []

        num_segments = len(transcript.segments)
        num_groups = num_segments // n_segments  # Numero di gruppi completi
        remainder = num_segments % n_segments  # Elementi rimanenti

        # Itera sui gruppi completi
        for i in range(0, num_groups * n_segments, n_segments):
            segment_group = transcript.segments[i : i + n_segments]
            start_seg = segment_group[0]["start"]
            end_seg = 0
            text_seg = ""
            for elem in segment_group:
                end_seg = elem["end"]
                text_seg += elem["text"]

            new_seg = {"start": start_seg, "end": end_seg, "text": text_seg}
            aug_segment.append(new_seg)

        # Gestisci gli elementi rimanenti
        if remainder > 0:
            remaining_segments = transcript.segments[num_groups * n_segments :]
            if remaining_segments:
                start_seg = remaining_segments[0]["start"]
                end_seg = remaining_segments[0]["start"]
                text_seg = remaining_segments[0]["text"]

            for elem in remaining_segments[1:]:
                end_seg = elem["end"]
                text_seg += elem["text"]
            new_seg = {"start": start_seg, "end": end_seg, "text": text_seg}
            aug_segment.append(new_seg)
    except Exception as e:
        print("Error in audio segment: ", e)

    return aug_segment


def convert_video_to_audio_moviepy(file_path):
    try:
        # Output file path for audio
        audio_file_path = file_path.replace(".mp4", ".mp3")

        # Open video file clip
        with VideoFileClip(file_path) as video_clip:
            # Extract audio and write directly to file
            video_clip.audio.write_audiofile(audio_file_path, codec="libmp3lame")

        return audio_file_path
    except Exception as e:
        print("Error in converting video to audio: ", e)


def create_facts_by_AudioVideo(
    file_path, chunk_size, type_file, file_name, temp_direct, hqt
):

    try:

        if type_file == "mp4":
            file_path = convert_video_to_audio_moviepy(file_path)

        audio_segmented = AudioSegment.from_mp3(file_path)
        indice_inizio = 0
        ten_minutes = 10 * 60 * 1000
        final_facts = []

        if len(audio_segmented) < ten_minutes:

            transcript = openai_transcriptions(file_path)
            if transcript is None:
                return None

            aug_segment = segment_audio_citation(transcript)

            for seg in aug_segment:
                inizio = time.strftime("%H:%M:%S", time.gmtime(seg["start"]))
                fine = time.strftime("%H:%M:%S", time.gmtime(seg["end"]))
                citation = "File: " + file_name + " Start: " + inizio + " End: " + fine
                partial_fact = content_to_facts(
                    seg["text"],
                    file_path,
                    chunk_size,
                    "Audio-Video",
                    file_name,
                    citation,
                )

                if partial_fact is not None:
                    final_facts += partial_fact
            remove_file_with_retry(file_path=file_path)
            return final_facts

        # ELSE VIDEO > 10 MINUTES
        while indice_inizio < len(audio_segmented):
            indice_fine = indice_inizio + ten_minutes
            if indice_fine > len(audio_segmented):
                indice_fine = len(audio_segmented)
            segmento_corrente = audio_segmented[indice_inizio:indice_fine]
            temp_name = "audio" + str(indice_fine) + ".mp3"

            file_path = os.path.join(temp_direct, temp_name)
            segmento_corrente.export(file_path, format="mp3")

            transcript = openai_transcriptions(file_path)
            if transcript is None:
                return None

            aug_segment = segment_audio_citation(transcript)

            for seg in aug_segment:
                inizio = time.strftime(
                    "%H:%M:%S", time.gmtime(seg["start"] + float(indice_inizio / 1000))
                )
                fine = time.strftime(
                    "%H:%M:%S", time.gmtime(seg["end"] + float(indice_inizio / 1000))
                )
                citation = "File: " + file_name + " Start: " + inizio + " End: " + fine
                partial_fact = content_to_facts(
                    seg["text"],
                    file_path,
                    chunk_size,
                    "Audio-Video",
                    file_name,
                    citation,
                    hqt,
                )
                if partial_fact is not None:
                    final_facts += partial_fact

            indice_inizio += ten_minutes

        remove_file_with_retry(file_path=file_path)

    except Exception as e:
        print("Error in facts audio create: ", e)

    return final_facts
