import re

def sanitize_string(stringa):
    if stringa is None:
        return None
    stringa_pulita = stringa.replace("\\","")
    stringa_pulita = stringa.replace("\\n"," ")
    stringa_pulita = stringa.replace("\n"," ")
    stringa_pulita = stringa.replace("\xa0"," ")
 
    stringa_pulita = re.sub(r'[^\w\s\d.,?!]', '', stringa)
    return stringa_pulita


def sanitize_json_string_for_report(data):
    js_string = data["content"]
    js_string = js_string[1:-1]
    js_string = js_string.replace("'", '"')
    js_string = js_string.replace('\\xa0', ' ')
    lista = re.split(r'(?<=}\}),\s*', js_string)
    lista = [elemento for elemento in lista if elemento]
    return lista