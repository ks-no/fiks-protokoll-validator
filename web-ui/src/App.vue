<template>
  <div id="app">
    <b-navbar toggleable="lg" type="light" variant="light" :fixed="'top'">
      <img style="margin-left:150px" src="./assets/logo.png" fluid alt="stam" />
      <b-navbar-brand style="padding:10px 25px 10px 4px" to="/">
        Fiks-protokollvalidator
      </b-navbar-brand>
      <b-navbar-nav>
        <b-nav-item to="/NewTestSession">Ny testsesjon</b-nav-item>
      </b-navbar-nav>
    </b-navbar>

    <router-view />
  </div>
</template>

<script>
export default {
  name: "App"
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
