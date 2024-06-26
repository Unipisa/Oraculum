# Oraculum and Sibylla

**New:** Checkout `OraclumLocalBox` for a self-contained sandbox for trying the system.

**New:** OraculumApi allows to easily embed Sibylla/Oraculum systems using swagger based API.

The aim of this project is to create a lightweight and easy to deploy AI assistant dedicated to answer
to questions using a knowledge base expressed in facts. Relying on a vector database (Weaviate) the
knowledge is "semantically" indexed using _embeddings_ so that information expressed in a language can
be queried using different languages.

Facts are small chunks of information with metadata (i.e. _category_, or a citation string) and are
selected for engineering a prompt by the chat assistant named _Sibylla_ for answering question about
topics in the knowledge base. It is possible to have multiple Sibyllae accessing the _Oraculum_ knowledge
base whose behavior is affected by the specific prompt used to configure each of them.

Knowledge is added to the system using a set of PowerShell cmdlets that can be used to spider and
preprocess information from different sources (i.e. web pages, documents, etc.) and then add them to
the knowledge base. To monitor the evolution of the knowledge and the behavior of the Sibyllae the project
also includes a .NET Interactive notebook that can be used to inspect the knowledge base and instantiate
multiple Sibyllae for testing.

![System architecture](Architecture.png "System architecture")

## Why another one?

When I started developing this project the main reasons were:

- Easy integration with existing software
- Easy to deploy and understand (small codebase)
- Avoid intricacies of a generalized architecture when many of the tools used are still in their infancy and models not yet consolidated
- Focus on tools for knowledge management, not on AI assistant

The project it's still in his infancy but it can be used in production if you want to quickly start with AI in your systems.

## What's new

### Version 1.3.5

- **Schema change**: The `Fact` class now is annotated to allow `Weaviate.NET` to use OpenAI embeddings v3 (large model).
  The upgrade procedure can be explicit or if you connect to Weaviate. The upgrade procedure can be slow since all the data is exported and
  reindexed. It is recommended to backup the database before upgrading.

### Version 1.3.4

- Added Backup and Restore functions to Oraculum to save and restore database

### Version 1.3.3

- Added `UserName` option in `Oraculum` configuration to indicate the default owner id. Fixed minor bugs.

### Version 1.3.1

- Exposed the Logger property in `Sibylla` and `Oraculum`

### Version 1.3.0

- Improved the `FindRelevantFacts` algorithm in Oraculum to avoid adding unrelated facts to the prompt. Now distance is normalized and used to decide which facts are relevant under the assumption that the retrieved facts are more than those relevant.
- Improved the memory management in Sibylla and changed the SibyllaConf (_breaking change in the file format_, look `Demo.json` file in `SibyllaSandbox` project)

### Version 1.2.0

- Added support for OpenAI functions and integration within Sibylla for easy implementation.
- Added an API project to easily embed Sibylla/Oraculum systems using swagger based API.

### Version 1.1.3

- Fixed a bug in the application of Knowledge Filters from Sibylla configuration

### Version 1.1.2

- Added `UpdateFact` to support fact update (in particular from CLI)

### Version 1.1.1

- Added support for Azure OpenAI version of GPT. The Oraculum configuration has been updated to include also Azure OpenAI parameters.

### Version 1.1.0

- Completely rewritten memory for Sibylla
- Introduced support to avoid persistance of unrelated messages into prompt
- Added `ILogger` support
- Changed Weaviate schema
- The upgrade procedure should be automatic though I suggest a `Get-Facts | ConvertTo-Json > backup.json` before upgrading.

### Version 1.0.2

- Exposed `Configuration` property in `Sibylla` class
- Added `Title` property to `SibyllaConf`

## What you need to start

In the jungle of models, companies, and tools of nowadays AI I decided for a set of tools, the overall system can be reasonably adapted to other tools but if you want to use it you need:

- An instance of Weaviate vector DB (either on-prem or in the cloud)
- An OpenAI subscription (both embeddings and chat are implemented using OpenAI technologies)

This project relies on [WeaviateNET](https://github.com/Unipisa/WeaviateNET) to access and use Weaviate vector DB.

You can tweak the _docker-compose.yml_ file included in the project to deploy a self-contained sandbox for trying.

## Tools and management

The core project is very small (less than 1k loc), and it is packaged as .NET library so that you can easily embed it inside any .NET project. A Web API is also on the roadmap so that also Web-based integration scenarios will be supported.

An important aspect considered in the design is CLI support: we firmly believe that knowledge management is essential and command line is a precious tool when you need to get knowledge from most disparate sources. For this reason we created a set of powershell cmdlets that you can run on any platform for inspecting the fact database and edit it using your favorite set of tools for spidering and preprocessing.

## Getting started

### Initialize Weaviate

The easiest way is to allocate a Weaviate instance on [Weaviate](https://weaviate.io) or to start a docker image if you have on-prem infrastructure. You can then install PowerShell core (7.3.0 or later) and then simply use the Oraculum powershell module from PowerShell gallery:

Install-Module Oraculum

During the first run you need to create the _Oraculum_ configuration file (if you use an unauthenticated version of Weaviate you can omit the ApiKey and also the OrgID in the OpenAI API is not rally used):

    $c = New-Object OraculumCLI.OraculumConfiguration
    $c.OpenAIApiKey = 'Your key'
    $c.OpenAIOrgId = 'Your org id'
    $c.WeaviateApiKey = 'Your weaviate api key'
    $c.WeaviateEndpoint = 'http://localhost:8080/v1'
    ConvertTo-Json $c > myoraculum.conf

Now you can initialize the schema and start loading your knowledge:

    Connect-Oraculum -ConfigFile myoraculum.conf
    Reset-Schema
    Add-Fact -Category FAQ -Title "My first FAQ" -Content "Content of my FAQ"
    ...
    Get-Facts | Out-GridView

The _Get-Facts_ cmdlet allows you to inspect the Fact class, you can use filters to select only facts with some meta-attributes.

### Configure Sibylla

You can configure an instance of _Sibylla_ in a similar way:

    $c = New-SibyllaConf -SystemPrompt "You are an operator who answers questions from users of the X system. You will only respond to questions regarding missions on behalf of the Organization. For all other questions, you will reply with 'I am authorized to respond only to X-related matters.' To answer, you will use only true facts and information related to the facts in XML format that will follow in place of your knowledge. Facts with the 'faq' element address frequently asked questions, and facts with the 'reg' element pertain to regulation clauses. If you use information from an XML fact that has a 'cit' attribute, include the citation in parentheses in your response. Each question you receive will be from a user with an issue." -AssistantPrompt "Welcome to X support" -Model gpt-3.5-turbo -MaxTokens 256 -Temperature 0.1
    ConvertTo-Json $c > mysibylla.conf

Now you can start a Sibylla session:

    New-SibyllaSession -ConfigFile mysibylla.conf

**Important**: The prompt of a Sibylla is up to you but the system will inject relevant facts from the Weaviate DB in XML format so you should always mention this in your prompt.

## OraculumAPI / OraculumFE Stack quick start (local)

If you want to quickly deploy the API/FE stack locally you need a Docker service up and running on your machine.

Then follow these steps:

1. Configure your OraculumAPI's appsettings.json with your Api Keys. Ignore the "Security" section if you don't need authentication.
2. Set your Api Key in the `docker-compose.yml`
3. Run the `docker compose up` command from the repository /demo/web folder
4. Initialize the Weaviate instance as described in the "Initialize Weaviate" sub-section or by visiting the api url (http://localhost:5009) and using the /api/v1/BackOffice/reset-and-initialize-schema endpoint
5. Open `http://localhost` in your browser to view the User Interface

## Implementation status

This is the first public release of the project, the strategy by which Sibylla adds facts to the prompt is still in its infancy and it will be needed more research and testing to find a policy respectful of the prompt size that still retain the important domain knowledge.
