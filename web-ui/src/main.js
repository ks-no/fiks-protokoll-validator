// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import { createApp } from "vue";
import App from "@/App";
//import router from "@/router";
import { BootstrapVue, IconsPlugin } from "bootstrap-vue";
import { library } from '@fortawesome/fontawesome-svg-core'
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome'
import { faCheck } from '@fortawesome/free-solid-svg-icons'
import VueRouter from 'vue-router'

import "bootstrap/dist/css/bootstrap.css";
import "bootstrap-vue/dist/bootstrap-vue.css";

//import "bootstrap/dist/css/bootstrap.min.css"
//import "bootstrap"

import StartPage from "@/components/StartPage";
import TestSession from "@/components/TestSession";
import NewTestSession from "@/components/NewTestSession";
import {createRouter, createWebHistory} from "vue-router/dist/vue-router";

library.add(faCheck)

/* eslint-disable no-new */
/*new Vue({
  el: "#app",
  router,
  components: { App },
  template: "<App/>",
  render(h) {
    return h(App);
  }
});*/

/*
const vueApp = createApp({
  render: h => h(App),
  //router,
  components: { App },
  template: "<App/>"
});*/

const routes = [
    {
        path: "/",
        name: "StartPage",
        component: StartPage
    },
    {
        path: "/TestSession/:testSessionId",
        name: "testSession",
        component: TestSession
    },
    {
        path: "/NewTestSession",
        name: "newTestSession",
        component: NewTestSession
    }]

const router = createRouter({
    history: createWebHistory(process.env.BASE_URL),
    routes,
})

const vueApp = createApp(App);
vueApp.use(router);
vueApp.use(BootstrapVue);
vueApp.mount("#app");
vueApp.component('font-awesome-icon', FontAwesomeIcon);

vueApp.use(IconsPlugin);

// vueApp.config.productionTip = false; //Funker ikke i vue 3
//vueApp.use(BootstrapVue);
