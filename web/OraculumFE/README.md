# Chatbot

This project was generated with [Angular CLI](https://github.com/angular/angular-cli) version 15.2.6.

## Install Node

On Windows use nvm to install node

## Development server

Run `npm run dev` for a dev server (uses dev.proxy.conf.json). Navigate to `http://localhost:4200/`. The application will automatically reload if you change any of the source files.

## Proxy configuration

In order to use the Angular proxy in your deployment you need to configure the proxy.conf.json with your endpoints and parameters. You can use dev.proxy.conf.json as a boilerplate.

## Code scaffolding

Run `ng generate component component-name` to generate a new component. You can also use `ng generate directive|pipe|service|class|guard|interface|enum|module`.

## Build

Run `ng build` to build the project. The build artifacts will be stored in the `dist/` directory.

## Further help

To get more help on the Angular CLI use `ng help` or go check out the [Angular CLI Overview and Command Reference](https://angular.io/cli) page.

## Dockerfile

docker build -t angular-docker .
docker run -p 80:80 angular-docker
