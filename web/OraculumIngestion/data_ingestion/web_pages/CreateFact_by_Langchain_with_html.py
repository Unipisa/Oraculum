from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain.text_splitter import HTMLHeaderTextSplitter
import requests
from bs4 import BeautifulSoup
import re
import json
from utils.TitleContentGenerate_GPT_ChatCompletions import (
    title_content_generate,
    title_generate,
)


def add_link(content, link_dict, split_page_content):
    if link_dict is None:
        return content
    try:
        for text_link, link in link_dict.items():
            escaped_text_link = re.escape(text_link)
            match_object = re.search(escaped_text_link, split_page_content)
            if match_object:
                if content and text_link and link is not None:
                    content = content + " [" + text_link + "](" + link + ")"
    except Exception as e:
        print(f"Error (add link): {e}")
        return content

    return content


def clean_link_values(links):
    try:
        cleaned_links = {}
        for link in links:
            text = link.text.strip()
            href = link.get("href")
            if text and href:
                cleaned_links[text] = href
    except Exception as e:
        print(f"Error (clean link values): {e}")
        return None

    return cleaned_links


def create_facts_by_lang_with_html(json_input, chunk_size=800):

    facts = []
    all_valid_page = json_input["origin"]
    hqt = True
    if "hqt" in json_input:
        hqt = json_input["hqt"]
    else:
        hqt = True

    for url in all_valid_page:
        print(url + " Processing...")
        try:
            response = requests.get(url)
            soup = BeautifulSoup(response.content, "html.parser")
            links = soup.find_all("a")
        except Exception as e:
            print(f"Error: {e}")
            continue

        try:
            link_dict = clean_link_values(links)

            headers_to_split_on = [
                ("h1", "Header 1"),
                ("h2", "Header 2"),
                ("h3", "Header 3"),
                ("h4", "Header 4"),
                ("h5", "Header 5"),
                ("h6", "Header 6"),
                ("h7", "Header 7"),
            ]

            html_splitter = HTMLHeaderTextSplitter(
                headers_to_split_on=headers_to_split_on
            )
            html_header_splits = html_splitter.split_text_from_url(url)

            text_splitter = RecursiveCharacterTextSplitter(chunk_size=chunk_size)

            # Split
            splits = text_splitter.split_documents(html_header_splits)
            chunks = []
            for split in splits:
                if (
                    split.metadata and len(split.page_content) > 100
                ):  # ELIMINO split senza metadata o contenuto corto
                    content = split.page_content
                    content = add_link(content, link_dict, split.page_content)
                    if content is not None:
                        chunks.append(content)

            for chunk in chunks:
                if hqt is True:
                    fact = json.loads(title_content_generate(chunk))
                else:
                    fact = json.loads(title_generate(chunk))
                fact["citation"] = url
                fact["factType"] = "Web_Pages"
                facts.append(fact)

        except Exception as e:
            print(f"Error facts create webpages: {e}")
            continue

    return facts
