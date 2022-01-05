#FROM node:16 as build-stage
#ARG mode=production

#WORKDIR /app 

#COPY package*.json ./

#RUN npm install

#COPY public ./public
#COPY src ./src
#COPY babel.config.js .
#COPY .eslintrc.js .
#COPY .env .
#COPY .env.$mode .
#COPY vue.production.config.js vue.config.js

# build app for production with minification
#RUN npm run build -- --mode $mode

FROM docker-all.artifactory.fiks.ks.no/fiks-nginx-openshift:latest as production-stage

#COPY --from=build-stage /app/dist /usr/share/nginx/html
COPY /app/dist /usr/share/nginx/html

COPY  nginx.conf /etc/nginx/conf.d/app.conf
WORKDIR /etc/nginx

CMD ["nginx", "-g", "daemon off;"]