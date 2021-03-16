<template>
  <div id="app2">
    <div style="margin: 40px 0">
        <span
          style="width: 100%; display: flex; flex-direction: column; vertical-align: middle;"
        >
          <div style="float: left; width: 70%">
            <h3>
              Sist utførte testsesjon:
            </h3>
          </div>
          <div>
            <a v-bind:href="lastTestUrl" style="margin-top: 10px;">
              <span>
                <b-icon-cloud-arrow-down/>
                 Din testsesjon utført - {{ lastTestDateTime }} ({{lastTestUrl.split("TestSession/")[1]}})
        </span>
        </a>
          </div>
        </span>
    </div>
  </div>
</template>

<script>
import * as moment from "moment/moment";
export default {
  name: "App",

  data() {
    return {
      lastTestUrl: localStorage.validatorLastTest,
      lastTestDateTime: moment(localStorage.createdAt).format('DD/MM/YYYY')
    }
  }
};
</script>

<style>
html {
  overflow-y: scroll;
}

body {
  background-color: #fbfbfb;
  font-size: 14px;
}

#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: left;
  color: #2c3e50;
  margin-top: 60px;
  margin: 60px auto;
  width: 75%;
  background-color: white;
  padding: 35px;
}

h1 {
  margin-bottom: 15px;
}

.card-body {
  text-align: left;
}

.spinner-border {
  vertical-align: 8px;
  width: 25px;
  height: 25px;
  margin-left: 10px;
}

th,
td {
  border: 1px solid gray;
  padding: 5px 10px;
}

.result {
  margin-bottom: 35px;
}

.btn {
  margin-bottom: 35px;
}
</style>
