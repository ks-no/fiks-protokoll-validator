FROM node:12 as build-stage
ARG mode=production

# install simple http server for serving static content
RUN npm install -g http-server

WORKDIR /app 

COPY package*.json ./

RUN npm install

COPY public ./public
COPY src ./src
COPY babel.config.js .
COPY .eslintrc.js .
COPY .env .
COPY .env.$mode .
COPY vue.production.config.js vue.config.js

# build app for production with minification
RUN npm run build -- --mode $mode

FROM nginx:1.15.3

COPY --from=build-stage /app/dist /usr/share/nginx/html/dist

RUN rm /etc/nginx/conf.d/default.conf
COPY  nginx.conf /etc/nginx/conf.d/app.conf
WORKDIR /etc/nginx

CMD ["nginx", "-g", "daemon off;"]

#EXPOSE 8080
#CMD [ "http-server", "dist" ]