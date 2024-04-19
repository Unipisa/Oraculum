from openai import OpenAI
import json

from utils.PROMPT import TITLE_GENERATE
from utils.config import MODEL

client = OpenAI()
import spacy
from tenacity import (
    retry,
    stop_after_attempt,
    wait_random_exponential,
)


@retry(wait=wait_random_exponential(min=1, max=60), stop=stop_after_attempt(3))
def title_content_generate(content):
    response = client.chat.completions.create(
        model=MODEL,
        messages=[
            {"role": "system", "content": TITLE_GENERATE},
            {"role": "user", "content": content},
        ],
    )

    title = response.choices[0].message.content

    # Return the resulting JSON
    result = {
        "title": title,
        "content": content,
    }

    return json.dumps(result, indent=4)


def title_generate(content):
    nlp = spacy.load("it_core_news_sm")
    doc = nlp(content)
    word_freq = {}

    for token in doc:
        if not token.is_stop and not token.is_punct:
            word = token.text.lower()
            word_freq[word] = word_freq.get(word, 0) + 1

    sorted_word_freq = sorted(word_freq.items(), key=lambda x: x[1], reverse=True)

    title_words = [word for word, _ in sorted_word_freq[:5]]

    title = " ".join(title_words)

    result = {
        "title": title,
        "content": content,
    }
    return json.dumps(result, indent=4)
