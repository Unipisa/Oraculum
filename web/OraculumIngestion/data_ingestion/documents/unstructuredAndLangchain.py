from unstructured.partition.auto import partition
from utils.textToFacts import content_to_facts


def create_facts_by_documents(file_input, chunk_size=800, file_name="", hqt=True):
    try:
        final_facts = []
        with open(file_input, "rb") as file:
            elements = partition(file=file)

        contents_by_page = {}

        for elem in elements:
            text = elem.text

            page_number = elem.metadata.page_number

            # Verifica se la pagina è già presente nel dizionario
            if page_number in contents_by_page:
                # Se la pagina è già presente, aggiungi il testo all'elenco dei contenuti
                contents_by_page[page_number].append(text)
            else:
                contents_by_page[page_number] = [text]

        result = []

        for page_number, contents in contents_by_page.items():
            # Concatena tutti i contenuti nella pagina
            concatenated_content = " ".join(contents)

            result.append((concatenated_content, page_number))

        result_filtered = [tupla for tupla in result if tupla[1] is not None]

        for tupla in result_filtered:
            testo = tupla[0]
            page_number = tupla[1]
            citation = "From file: " + str(file_name) + " in page: " + str(page_number)
            partial_fact = content_to_facts(
                testo, file_input, chunk_size, "Document", file_name, citation, hqt
            )
            if partial_fact is not None:
                final_facts += partial_fact

    except Exception as e:
        return e

    return final_facts
