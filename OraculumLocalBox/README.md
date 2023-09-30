# Testing Oraculum and Sibylla locally

## Prerequisites

In order to test Oraculum and Sibylla locally you *must* install:

- [Docker](https://docs.docker.com/install/) and use thw WLS2 backend if you are on Windows
- [Visual Studio Code](https://code.visualstudio.com/)
- [Latest .NET SDK](https://dotnet.microsoft.com/en-us/download)
- [Polyglot](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode)

Then copy:

- _oraculum.conf_ into _oraculum.secret.conf_
- _weaviate.yml_ into _weaviate.secret.yml_

Edit the _secret_ files just copied and fill the OpenAI API key and Organization ID.

## .NET Api

Open the _dotnetplayground.ipynb_ notebook in Visual Studio code and run it.

## PowerShell Api

Open the _pwshplayground.ipynb_ notebook in Visual Studio code and run it.