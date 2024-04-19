from trafilatura.spider import focused_crawler


def augmented_list_url(json_input):
    print("Augmenting list of urls, spidering sub-pages")
    list_sub_pages = []
    for url in json_input['origin_urls']:
        to_visit, known_urls = focused_crawler(url, max_seen_urls=150, max_known_urls=150)
        to_visit = list(to_visit)
        known_urls = list(known_urls)
        to_visit += known_urls
        for page in to_visit:
            for valid_url_start in json_input['valid_urls_starts']:
                if str(page).startswith(valid_url_start):
                    list_sub_pages.append(page)
    if len(list_sub_pages) == 0:
        print("WARNING: No valid SUB-pages found")
    return list(dict.fromkeys(list_sub_pages + json_input['origin_urls']))
     