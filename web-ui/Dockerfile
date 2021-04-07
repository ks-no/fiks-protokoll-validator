FROM node:12 AS build-stage
ARG mode=production

# install simple http server for serving static content
#RUN npm install -g http-server

WORKDIR /app 

COPY package*.json ./

RUN npm install

COPY public ./public
COPY src ./src
COPY babel.config.js .
COPY .eslintrc.js .
COPY .env .
COPY .env.$mode .

# build app for production with minification
RUN npm run build -- --mode $mode

# production stage
FROM docker-all.artifactory.fiks.ks.no/fiks-nginx-openshift:latest as production-stage
COPY --from=build-stage /app/dist /usr/share/nginx/html
EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]

#EXPOSE 8080
#CMD [ "http-server", "dist" ]