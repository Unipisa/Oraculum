# Getting Started

This demo shows starts the Oraculum Web API and the Front-End to support explainability and knowledge inspection.

Use powershell core (7.3.0 or above) and install the Oraculum module:

    Install-Module Oraculum

Copy ``oraculum.conf`` into ``oraculum.secret.conf`` and insert the API Key (either OpenAI or Azure), and do the same 
with ``docker-compose.yml`` (in ``docker-compose.secret.yml``) and ``oraculumApiAppsettings.json`` (in ``oraculumApiAppsettings.secret.json``)

Start the system:

    docker compose -f ./docker-compose.secret.yml up

Open a new shell (or start the docker compose as a daemon) and execute the ``init.ps1`` script to reset the schema and load few facts for the demo.

Point your browser to http://localhost and have fun!
