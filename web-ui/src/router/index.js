import StartPage from "@/components/StartPage";
import TestSession from "@/components/TestSession";
import NewTestSession from "@/components/NewTestSession";
Vue.use(require("moment"));


import Vue from "vue";
import Router from "vue-router";

Vue.use(Router);

export default new Router({
  base: process.env.BASE_URL,
  routes: [
    {
      path: "/",
      name: "StartePage",
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
    }
  ]
});
