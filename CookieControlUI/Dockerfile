FROM node:10
WORKDIR /usr/src/app

COPY App/package*.json ./

RUN npm install

COPY App/ .

ENV UIApiHostname="UIApi"

ENTRYPOINT ["npm", "start"]