from langchain.text_splitter import RecursiveCharacterTextSplitter
from utils.TitleContentGenerate_GPT_ChatCompletions import (
    title_content_generate,
    title_generate,
)
import json
from langchain_experimental.text_splitter import SemanticChunker
from langchain_openai.embeddings import OpenAIEmbeddings
from langchain_openai import OpenAIEmbeddings
from utils.semanticchunk_custom.custom_semantic_langchain import CustomSemanticChunker


def custom_semantic_lang(testo, threshold=95):

    text_splitter = CustomSemanticChunker(
        embeddings=OpenAIEmbeddings(model="text-embedding-3-large"),
    )

    chunks = text_splitter.split_text(testo, threshold)

    return chunks


def semanticTextSplitter(content):
    text_splitter = SemanticChunker(
        embeddings=OpenAIEmbeddings(model="text-embedding-3-large"),
    )
    chunks = text_splitter.split_text(content)

    return chunks


def recursiveTextSplitter(content, chunk_size=800):
    text_splitter = RecursiveCharacterTextSplitter(chunk_size=chunk_size)
    text_splitter._chunk_overlap = 70
    chunks = text_splitter.split_text(content)

    return chunks


def content_to_facts(
    content,
    path=None,
    chunk_size="800",
    factType="testo",
    file_name="testo",
    citation="",
    hqt=True,
):

    try:

        chunks = custom_semantic_lang(content)

        facts = []
        for chunk in chunks:
            if len(chunk) > 50:
                if len(chunk) > 1200:
                    split_chunks = recursiveTextSplitter(chunk, chunk_size=chunk_size)
                    for split_chunk in split_chunks:
                        fact = json.loads(title_content_generate(split_chunk))
                        fact["citation"] = citation
                        fact["factType"] = factType
                        facts.append(fact)

                else:
                    if hqt is True:
                        fact = json.loads(title_content_generate(chunk))
                    else:
                        fact = json.loads(title_generate(chunk))
                    fact["citation"] = citation
                    fact["factType"] = factType
                    facts.append(fact)
    except Exception as e:
        return list(e)

    return None if len(facts) == 0 else facts
