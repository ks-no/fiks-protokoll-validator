FROM ghcr.io/ks-no/openshift-nginx/fiks-nginx-openshift:1.9.0 as production-stage

COPY dist /usr/share/nginx/html

COPY  nginx.conf /etc/nginx/conf.d/app.conf
WORKDIR /etc/nginx

CMD ["nginx", "-g", "daemon off;"]