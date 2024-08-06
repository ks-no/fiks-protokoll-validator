FROM docker-all.artifactory.fiks.ks.no/fiks-nginx-openshift:1.7 as production-stage

COPY dist /usr/share/nginx/html

COPY  nginx.conf /etc/nginx/conf.d/app.conf
WORKDIR /etc/nginx

CMD ["nginx", "-g", "daemon off;"]