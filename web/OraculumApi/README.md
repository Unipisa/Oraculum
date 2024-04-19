# Oraculum API Documentation

## Version 1

Oraculum API offers a comprehensive suite of endpoints for interacting with both the Frontoffice and Backoffice services. This documentation outlines the available API endpoints, their purposes, and how to use them.

## Prerequisites

Ensure the following prerequisites are met to run the project:

- **.NET 7.0 SDK**: Required for project compilation and execution. Download and install from the [.NET official website](https://dotnet.microsoft.com/download/dotnet/7.0).

Additionally, ensure a Command Line Interface (CLI) is available, such as Windows Command Prompt or PowerShell, for executing commands.

## Configuration

Edit appsettings.json and provide your api keys and the endpoint of a running instance of Weaviate:

```json
...
 "Weaviate": {
      "ServiceEndpoint": "http://oraculum-weaviate:8080/v1"
    },
    "OpenAI": {
      "ApiKey": "your-api-key-here",
      "OrgId": "your-org-id-here"
    },
    ...
```

## Getting Started with CLI

Follow these steps to run your project using the .NET CLI:

1. **Open Your CLI**: Start Command Prompt, PowerShell, or your preferred terminal in `OraculumApi` folder.

2. **Restore Dependencies**: Run the following command to install required NuGet packages:

   ```bash
   dotnet restore
   ```

3. **Build the Project** (Optional): Compile your project explicitly (optional since `dotnet run` also compiles the project):

   ```bash
   dotnet build
   ```

4. **Run the Project**: Launch your application by executing:

   ```bash
   dotnet run
   ```

   Your project should now be running and accessible at the default URL: `http://localhost:5009`.
   Swagger is visible at `http://localhost:5009/swagger`.

## Detailed Configuration File Description

This section provides an overview of the `appsettings.json` file configurations for the OraculumAPI project. It explains each configuration's purpose and its implications for application behavior.

The `appsettings.json` file is structured into several key sections, each configuring different aspects of the Oraculum application, from logging and security to service endpoints and multi-tenancy support.

### Logging
The Logging section is configured to control the verbosity of log messages, with "Information" level for general application flow and "Warning" level for ASP.NET Core infrastructure messages.

### AllowedHosts 
- **AllowedHosts**: Specifies the hosts allowed to make requests to the application.

### Anonymous Access
- **AllowAnonymous**: When enabled, it bypasses all authentication and authorization checks across the application, making it crucial to use this setting cautiously, especially in production environments. When this option is enabled you can omit the "Security" section.

### DBSibyllaeConfigOnly
When set to true, this configuration instructs the application to disregard any Sibyllae configurations found in the `SibyllaeConf` folder, relying solely on configurations stored in the database. This setting is crucial for environments where database-stored configurations are preferred over file-based configurations.

### Tenants Configuration
OraculumAPI supports multi-tenancy with configurations per tenant. The "Default" tenant serves as a fallback. Each tenant configuration allows specifying different settings for services such as Weaviate (vector database), OpenAI, and security settings.

#### Services
- **Weaviate**: A vector database service essential for managing and searching vector data.
- **OpenAI**: Configuration for integrating with OpenAI services, requiring an API key and an organization ID.
- **Azure**: Configuration for integrating with Azure AI services, requiring an API key.

#### Security
Manages authentication and authorization through OpenID Connect (OIDC) settings, including the authority URL, client ID, and audience.
**The application needs an OIDC Public client.**

The `AuthorizationRolesMap` translates external client roles into internal application roles, defined as follows:
  - **frontoffice**: The least privileged role, allowing access to basic functionalities such as simple chat endpoints.
  - **backoffice**: A more privileged role than Frontoffice, granting access to knowledge management features like chat explanations and knowledge ingestion.
  - **sysadmin**: The most privileged role, enabling configuration of Sibyllae, and viewing and editing of model parameters like system prompts and temperature.


### Other Services
- **EvaluateService and DataIngestion**: These configurations define the endpoints for external Python services that provide functionalities for evaluating RAG models and ingesting documents into the vector database, respectively. The `MaxTimeoutMinutes` setting specifies the timeout for these operations.

## Security Warning

It is imperative to use the `AllowAnonymous` setting with caution, especially in production environments, due to the significant security implications of bypassing authentication and authorization mechanisms.

---

**OraculumFE** is based on 'FrontOffice', 'Backoffice/Facts', 'User', 'DataIngestion' and 'Evaluation' endpoints.

'ChatDetail', 'Feedback', 'Metric' and 'SibyllaPersistentConfig' are internally used by application and their sections in swagger are auto-generated and not documented.

---

## BackOffice

### Sibylla Configurations

Allows sysadmins to manage complete configuration of a Sibylla and its parameters

- **Create a new Sibylla configuration**

  `POST /api/v1/BackOffice/sibylla-configs`

- **Retrieve all Sibylla configurations**

  `GET /api/v1/BackOffice/sibylla-configs`

- **Edit Sibylla configs**

  `PUT /api/v1/BackOffice/sibylla-configs`

- **Delete a Sibylla configuration by its ID**

  `DELETE /api/v1/BackOffice/sibylla-configs/{id}`

- **Retrieve a Sibylla configuration by its ID**

  `GET /api/v1/BackOffice/sibylla-configs/{id}`

### Facts

CRUD operations on facts and semantic search

- **Add new facts**

  `POST /api/v1/BackOffice/facts`

- **Retrieve all facts**

  `GET /api/v1/BackOffice/facts`

- **Edit facts**

  `PUT /api/v1/BackOffice/facts`

- **Delete a fact by its ID**

  `DELETE /api/v1/BackOffice/facts/{id}`

- **Retrieve a fact by its ID**

  `GET /api/v1/BackOffice/facts/{id}`

- **Multiple fact delete with filters**

  `DELETE /api/v1/BackOffice/facts`

- **Find relevant facts based on provided criteria**

  `POST /api/v1/BackOffice/facts/query`

### Generic Objects

> **CRUD operations** - Internally used by application

- **Add a generic object**

  `POST /api/v1/BackOffice/generic-objects`

- **List all generic objects**

  `GET /api/v1/BackOffice/generic-objects`

- **Edit generic object**

  `PUT /api/v1/BackOffice/generic-objects`

- **Get a generic object**

  `GET /api/v1/BackOffice/generic-objects/{id}`

- **Delete a generic object by id**

  `DELETE /api/v1/BackOffice/generic-objects/{id}`

### Feedback

- **List feedbacks**
  Get all existing feedbacks and references to messages and chat

  `GET /api/v1/BackOffice/feedback`

---

## Backup

- **Create, retrieve, and manage backups**
  Allows to save a copy of the vectorstore

  `POST /api/v1/Backup`

  `GET /api/v1/Backup`

  `GET /api/v1/Backup/{backupId}/status`

  `GET /api/v1/Backup/{backupId}`

  `DELETE /api/v1/Backup/{backupId}`

  `POST /api/v1/Backup/Restore`

  `GET /api/v1/Backup/Restore`

---

## ChatDetail

> **CRUD operations** - Internally used by application

- **Manage and retrieve chat details**

  `GET /api/v1/ChatDetail/{id}`

  `DELETE /api/v1/ChatDetail/{id}`

  `GET /api/v1/ChatDetail`

  `POST /api/v1/ChatDetail`

  `PUT /api/v1/ChatDetail`

  `GET /api/v1/ChatDetail/property/{propertyName}/{propertyValue}`

---

## DataIngestion

- **Create new facts from various sources and check an ingestion task status**

  `POST /api/v1/DataIngestion/factsFromText`

  `POST /api/v1/DataIngestion/factsFromWebPages`

  `POST /api/v1/DataIngestion/factsFromDocuments`

  `POST /api/v1/DataIngestion/factsFromAudioVideo`

  `GET /api/v1/DataIngestion/checkStatus/{taskId}`

---

## Evaluate

- **Evaluate a test set and check an evaluation task status**

  `POST /api/v1/Evaluate/sibylla/{sibyllaId}/Evaluate`

  `GET /api/v1/Evaluate/TaskStatus/{taskId}`

---

## Feedback

> **CRUD operations** - Internally used by application

Feedback endpoints allow the management and retrieval of feedback items.

- **Retrieve feedback by ID**

  `GET /api/v1/Feedback/{id}`

- **Delete feedback by ID**

  `DELETE /api/v1/Feedback/{id}`

- **Retrieve all feedback**

  `GET /api/v1/Feedback`

- **Create new feedback**

  `POST /api/v1/Feedback`

- **Update existing feedback**

  `PUT /api/v1/Feedback`

- **Retrieve feedback by property**

  `GET /api/v1/Feedback/property/{propertyName}/{propertyValue}`

---

## FrontOffice

FrontOffice endpoints focus on interactions with Sibylla and chat management.

- **Delete a chat by its ID**

  `DELETE /api/v1/FrontOffice/sibylla/{sibyllaId}/chat/{chatId}`

- **Get single chat details by ID**

  `GET /api/v1/FrontOffice/sibylla/{sibyllaId}/chat/{chatId}`

- **Get all Sibyllae info**

  `GET /api/v1/FrontOffice/sibylla`

- **Get all chats of a Sibylla**

  `GET /api/v1/FrontOffice/sibylla/{sibyllaId}/chat`

- **Creates a new chat on a Sibylla given its ID**

  `POST /api/v1/FrontOffice/sibylla/{sibyllaId}/chat`

- **Post a message in an existing and active chat**

  `POST /api/v1/FrontOffice/sibylla/{sibyllaId}/chat/{chatId}/message`

- **Give feedback to an existing message**

  `POST /api/v1/FrontOffice/sibylla/{sibyllaId}/chat/{chatId}/feedback`

- **(Not implemented) Retrieve reference by ID**

  `GET /api/v1/FrontOffice/reference/{id}`

- **Debug API for metrics (sync method)**

  `POST /api/v1/FrontOffice/debug/answer`

- **Post a message to an existing chat and get answer and relevant facts**
  Used to provide high-explainability chat

  `POST /api/v1/FrontOffice/sibylla/{sibyllaId}/chat-explain/{chatId}/message`

## Metric

> Internally used by application

Metric endpoints are used for creating, retrieving, and managing metrics.

- **Retrieve metric by ID**

  `GET /api/v1/Metric/{id}`

- **Delete metric by ID**

  `DELETE /api/v1/Metric/{id}`

- **Retrieve all metrics**

  `GET /api/v1/Metric`

- **Create a new metric**

  `POST /api/v1/Metric`

- **Update an existing metric**

  `PUT /api/v1/Metric`

- **Retrieve metric by property**

  `GET /api/v1/Metric/property/{propertyName}/{propertyValue}`

## SibyllaPersistentConfig

Allows admins to create and manage Sibyllae persisting configurations

- **Retrieve configuration by ID**

  `GET /api/v1/SibyllaPersistentConfig/{id}`

- **Delete configuration by ID**

  `DELETE /api/v1/SibyllaPersistentConfig/{id}`

- **Retrieve all configurations**

  `GET /api/v1/SibyllaPersistentConfig`

- **Create a new configuration**

  `POST /api/v1/SibyllaPersistentConfig`

- **Update an existing configuration**

  `PUT /api/v1/SibyllaPersistentConfig`

- **Retrieve configuration by property**

  `GET /api/v1/SibyllaPersistentConfig/property/{propertyName}/{propertyValue}`

## User

User endpoints provide user authentication and information services.

- **Get user info**

  `GET /api/v1/User/me`

- **Login by IDP (Identity Provider)**

  `GET /api/v1/User/login-oidc`

- **OIDC (OpenID Connect) info**

  `GET /api/v1/User/oidc-info`
