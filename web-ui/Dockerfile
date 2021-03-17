#FROM node:lts-alpine
FROM node:12

# install simple http server for serving static content
RUN npm install -g http-server

WORKDIR /app 

COPY package*.json ./

RUN npm install

COPY public ./public
COPY src ./src
COPY babel.config.js .
COPY .eslintrc.js .
COPY .env.production .

# build app for production with minification
RUN npm run build

EXPOSE 8080
CMD [ "http-server", "dist" ]