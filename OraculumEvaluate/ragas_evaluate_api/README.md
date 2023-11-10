# API di valutazione RAGAS

## Avvio di OraculumAPI in locale

Per eseguire la valutazione dell'assistente Sibylla viene utilizzato l'endpoint /api/v1/FrontOffice/getanswer/debug/{query} del progetto OraculumAPI, che permette di fornire una domanda e ottenere la risposta insieme ai documenti rilevanti recuperati dal vectorstore e utilizzati per generarla.

Con `dotnet run` nella cartella OraculumAPI si lancia il server in locale che ascolta su http://localhost:5009.
Su http://localhost:5009/swagger è anche possibile visualizzare tutte le altre API.

## Utilizzo del tool di valutazione

### Inizializzazione

È raccomandato usare cmd su Windows.
Al primo utilizzo eseguire `python init.py` per inizializzare il DB, che viene creato nella cartella instance.

### Avvio

Dopo l'init, e per tutti gli utilizzi successivi, eseguire `python app.py` per avviare il server in locale, che risponde su http://localhost:5000.
Su http://localhost:5000/api/docs/ è possibile visualizzare lo swagger.

### Preparazione delle domande da sottoporre all'assistente e generazione delle risposte

Per ottenere la valutazione è sufficiente un json con la lista delle domande, ad es:

```json
[
  {
    "question": "Domanda uno...?"
  },
  {
    "question": "Domanda due...?"
  }
  ...
]
```

Tuttavia, in mancanza delle risposte attese ("ground_truths"), non sarà possibile calcolare la metrica "context_recall", e di conseguenza il "ragas_score" riassuntivo sarà sempre zero.
Pertanto è consigliabile fornire un json con la seguente struttura:

```json
[
  {
    "question": "Domanda uno...?",
    "ground_truths": ["Risposta attesa uno"]
  },
  {
    "question": "Domanda due...?",
    "ground_truths": ["Risposta attesa due"]
  }
  ...
]
```

Eseguire una POST su `/augment`: la funzione effettua una chiamata a OraculumAPI per ogni domanda, e recupera le risposte e i contesti. La risposta di `/augment` è l'oggetto che viene salvato sul DB, che ha le seguenti proprietà:

- id (int)
- output_data (str), il json fornito a cui sono stati aggiunti "answer" e "contexts"
- data_type (str) che in questo caso vale "augmented" (sarà "evaluated" per i dati già valutati)
- timestamp (date)

### Valutazione delle risposte

Con una GET su `/evaluate?id=1` si effettua la valutazione con il metodo RAGAS e viene dato in output un oggetto di tipo "evaluated" che include le metriche faithfulness, answer_relevancy, context_precision, context_recall e ragas_score; queste ultime due sono disponibili solo se sono presenti le ground_truths. Le metriche vengono calcolate per ogni domanda e sull'intero dataset complessivo.


### Salvataggio dei risultati come foglio di calcolo

Per salvare la valutazione come .xlsx fare GET su `/report?id=2`