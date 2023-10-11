# Testing Oraculum and Sibylla locally

## Prerequisites

In order to test Oraculum and Sibylla locally you *must* install:

- [Docker](https://docs.docker.com/install/) and use thw WLS2 backend if you are on Windows

It is recommended to install also the following tools:

- [Visual Studio Code](https://code.visualstudio.com/)
- [Latest .NET SDK](https://dotnet.microsoft.com/en-us/download)
- [Polyglot](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode)

Then copy:

- _oraculum.conf_ into _oraculum.secret.conf_
- _weaviate.yml_ into _weaviate.secret.yml_

Edit the _secret_ files just copied and fill the OpenAI API key and Organization ID.

## .NET Api

Open the _dotnetplayground.ipynb_ notebook in Visual Studio code and run it. You need _.NET Interactive_ and _Polyglot_ to interactively test the .NET API.

## PowerShell Api

Open the _pwshplayground.ipynb_ notebook in Visual Studio code and run it. You can also run the command inside the _pwsh_ terminal without using Visual Studio Code. The notebook explains how to use commands step by step. 