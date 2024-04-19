# Data Ingestion Repository
## Description

This repository serves as a comprehensive solution for data ingestion, providing APIs to extract text from various sources, including documents, audio, and video files. Additionally, it offers the capability to segment the extracted text into chunks with associated titles.




## Installation
### To install IN LOCAL the necessary dependencies, execute the following command:
```console
python --version --> Python 3.10.11
python -m venv venv
venv\Scripts\activate 
python -m spacy download it_core_news_sm
pip install -r requirements.txt
pip install unstructured==0.12.6
pip install "unstructured[docx]"
pip install "unstructured[pdf]"
Edit host and port in main.py (host="localhost", port=5000)
delete in static/swagger.josn line:  "host": "https:...",
python main.py
```
### Docker install
```console
docker build -t data_ingestion .

docker run -d -p 80:80 -e OPENAI_API_KEY=apikeyhere data_ingestion
```
Swagger = http://localhost/swagger/

## Usage
Run main.py to initialize the application, and then interact with the following endpoints:
At this address, you can find the Swagger: http://localhost:5000/swagger/ for LOCAL RUN

### API for Extracting "Facts" from  Text
Endpoint: "/API_facts_from_text"

#### Accepts JSON input:

```console
[
    {
        "content": "Cineca è un Consorzio Interuniversitario senza scopo di lucro!"
    }
]
```
### API for Extracting "Facts" from Web Pages

Endpoint: "/API_facts_from_web_pages"

#### Accepts JSON input:

```console
{
    "origin": [
        "https://it.wikipedia.org/wiki/Pagina_principale"
    ]
}
```
### API for Extracting "Facts" from Documents
Endpoint: "/API_facts_from_documents"

Accepts file input with supported formats: 'pdf', 'docx'
#### Specify the request file using the keyword "file"


### API for Extracting "Facts" from Audio/Video
Endpoint: "/API_facts_from_AudioVideo"

Accepts file input with supported formats: audio (e.g., mp3) or video (e.g., mp4).
#### Specify the request file using the keyword "file"


### ALL API response
All APIs respond with this JSON structure:
```console
[
    {
        "citation": "citation",
        "content": "content",
        "title": "title"
        "factType": "factType"
    }
]
```

# How it Works

## Common Utility Functions

The function "title_content_generate" takes a text as input and, using a prompt and an "openai.chat.completions", generates a relevant title for the content.

The function "content_to_facts" takes a text as input, divides it into chunks using the custom function "custom_semantic_lang", and:

Any chunk-text < 50 characters is discarded.
Any chunk-text > 1200 characters is considered too long and further split by "recursiveTextSplitter".
Subsequently, a fact is created and added to the chain of facts.

### content_to_facts

## Audio/Video Pipeline

Accept only MP4 video formats and MP3 audio formats. Videos are converted to MP3, and MP3s are converted to text using OpenAI's API with the whisper-1 model.

For audio files longer than 10 minutes, segmentation is performed using the pydub library (AudioSegment), and each segment is processed by the whisper-1 model for transcription from audio to text using the following parameters:
### response_format="verbose_json",
### timestamp_granularities=["segment"]
Return a JSON with a list of segments and their start and end indices (expressed in seconds).
To merge the segments and create a valid and accurate "citation," the function "segment_audio_citation" is used.
Finally, for each segment, the utils function "content_to_facts" is called.

## Web_pages Pipeline

BeautifulSoup is used for low-level extraction of links present on the page, which are then added to the fact's content. HTMLHeaderTextSplitter is used to split the content of the web page, and RecursiveCharacterTextSplitter with split_documents is used for splitting the HTML content. Contents < 100 (chart) are discarded, and the "title_content_generate" function is called.

## Documents Pipeline

Accept files in 'pdf' and 'docx' formats. Through the function "create_facts_by_documents," concatenate the content "group by" per page to have a reference. For each page, call the function "content_to_facts," which generates the facts.

## API from_text
Accept a JSON and directly call the function "content_to_facts."
