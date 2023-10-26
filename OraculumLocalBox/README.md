# Testing Oraculum and Sibylla locally

## Prerequisites

In order To test Oraculum and Sibylla locally, you *must* install Docker for both Windows and macOS:

- [Docker](https://docs.docker.com/install/) and use thw WLS2 backend if you are on Windows

It is recommended to install also the following tools:

- [Visual Studio Code](https://code.visualstudio.com/)
- [Latest .NET SDK](https://dotnet.microsoft.com/en-us/download)
- [Polyglot](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode)

Additional requirements are needed for macOS, go to the appropriate section further down.


## MacOS Intel and M1 

- As first thing we need to install [Xcode] (https://apps.apple.com/us/app/xcode/id497799835?mt=12)
- [.NET SDK] For Intel Silicon (https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-7.0.403-macos-x64-installer) for M1 Silicon (https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-7.0.403-macos-arm64-installer)
- Next we proceed with the installation of [Visual Studio Code] for MacOS for the right chipset (https://code.visualstudio.com/)
Remember it is critical to grant Visual Studio Code the right *security permissions*: Settings -> Privacy and Security -> Files and Folders and select Visual Studio Code for the right workspace where the repository is located.
Also be careful to give the right permissions to Terminal (or iTerm2 if you prefer): Settings -> Privacy and Security -> Developer Tools in order to ensure the proper execution of .NET interactive in the .ipynb notebook. 
- You can install [Polyglot] from the MarketPlace (https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.dotnet-interactive-vscode)
- Now it's time to install *Powershell* on Macos! There are several methods to be able to do this, the one I recommend is as a .NET Global tool but, even using Homebrew is a viable alternative. In the case of Homebrew be careful if you use bash or ZSH and the related environment variables identified by ```PATH```, otherwise I would not be able to invoke ```pwsh``` command.
If you already have the .NET Core SDK installed, it's easy to install PowerShell as a .NET Global tool.
```
dotnet tool install --global PowerShell
```

Then copy:

- _oraculum.conf_ into _oraculum.secret.conf_
- _weaviate.yml_ into _weaviate.secret.yml_

Edit the _secret_ files just copied and fill the OpenAI API key and Organization ID.

## .NET Api

Open the _dotnetplayground.ipynb_ notebook in Visual Studio code and run it. You need _.NET Interactive_ and _Polyglot_ to interactively test the .NET API.

## PowerShell Api

Open the _pwshplayground.ipynb_ notebook in Visual Studio code and run it. You can also run the command inside the _pwsh_ terminal without using Visual Studio Code. The notebook explains how to use commands step by step. 