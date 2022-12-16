import StartPage from "@/components/StartPage";
import TestSession from "@/components/TestSession";
import NewTestSession from "@/components/NewTestSession";
import { createApp } from 'vue';
import App from './../App.vue';
import { createRouter, createWebHistory } from "vue-router";

//const app = createApp(App);

//app.use(require("moment"));

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

export default createRouter({
  history: createWebHistory(process.env.BASE_URL),
  routes,
})
