using HidSharp.Reports.Encodings;
using OpenAI;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Serialization.Formatters;
using static OpenAI.ObjectModels.Models;

var mgr = new PowerSensorManager();

var openAIopt = new OpenAiOptions()
{
    BaseDomain = "http://127.0.0.1:11434",
    ProviderType = ProviderType.OpenAi,    
    ApiKey = "your-api-key"
};

var openAI = new OpenAIService(openAIopt);

var modelname =  (args.Length > 0) ? args[0] : "llama3.2";

var chat = new ChatCompletionCreateRequest()
{
    Model = modelname,
    Messages = new List<ChatMessage> {
        //new ChatMessage() { Role = "user", Content = "Sei un operatore che risponde alle domande degli utenti del sistema UWeb missioni dell'Università di Pisa. Rispondi solamente a domande riguardanti le missioni o il sistema U-Web missioni per conto dell'Università di Pisa, a tutte le altre domande risponderai 'sono autorizzato a rispondere solo a questioni riguardanti le missioni'. Per rispondere userai solo fatti veri e le informazioni relative ai fatti in XML che seguiranno al posto della tua conoscenza. I fatti con elemento 'faq' rispondono a domande frequenti, i fatti con elemento 'reg' si riferiscono a commi di regolamento. Se usi le informazioni di un fatto XML che ha un attributo 'cit' includi la citazione tra parentesi nella risposta. NON includere XML nella risposta solo le informazioni al suo interno. Ciascuna domanda che riceverai sarà quella di un utente che ha un problema.\n<facts><faq title=\"Quando posso utilizzare il codice spesa PASTF?\">Questa tipologia di spesa può essere utilizzata solo quando ricorrono le particolari condizioni previste dall’art. 10 del regolamento delle missioni (attività di protezione civile; rilevazione e controllo di impianti e installazioni scientifiche; tutela e rilevazioni del patrimonio storico, artistico e ambientale; scavi archeologici; attività che comportino imbarchi su unità navali).</faq><faq title=\"Quando si usa il codice spesa PASTG e quando il codice PASTS?\">Si sceglie il codice tipo spesa PASTG quando la missione ha una durata superiore a 12 ore (per le missioni in Italia è previsto il limite massimo giornaliero di € 80,00). Si sceglie il codice PASTS quando la missione ha una durata tra 4 e 12 ore (per le missioni in Italia è previsto il limite massimo giornaliero di € 40,00).</faq></facts>\nDevo compilare il riborso di una missione ad Amsterdam per unipi, cosa sono i codici PAST e quando si usano?" }
        new ChatMessage() { Role = "user", Content = "Descrivi l'organizzazione dello stato Italiano" }
    },
    MaxTokens = 4096
};

Console.WriteLine($"Starting benchmark - {modelname}");

var totk = 0.0;
var inpk = 0.0;
foreach (var m in chat.Messages)
{
    inpk += OpenAI.Tokenizer.GPT3.TokenizerGpt3.TokenCount(m.Content!) / 1024.0;
}
Console.WriteLine($"Total 1k context token count: {inpk}");
var t = mgr.RecordEnergy();

totk = inpk * 3;
for (int i = 0; i < 3; i++)
{
    var result = openAI.ChatCompletion.CreateCompletion(chat);
    var tokens = OpenAI.Tokenizer.GPT3.TokenizerGpt3.TokenCount(result.Result.Choices[0].Message.Content!) / 1024.0;
//    Console.WriteLine(result.Result.Choices[0].Message.Content!);
    Console.WriteLine($"Round #{i + 1}: 1k Token count: {tokens}");
    totk += tokens;
}
mgr.StopRecording();
t.Wait();
//Console.WriteLine(result.Result.Choices[0].Message.Content);
var (duration, en) = t.Result;
Console.WriteLine($"Total 1k Token count: {totk}");
Console.WriteLine($"Duration: {duration} s");
foreach (var k in en.Keys)
{
    Console.WriteLine($"{k}: {en[k] / totk} J/1k token");
}
