<template>
  <div id="app" class="min-h-screen bg-gray-50">
    <nav class="fixed top-0 left-0 right-0 bg-white shadow-sm z-50">
      <div class="flex items-center h-16 px-4">
        <img src="./assets/logo.png" alt="logo" class="h-12 ml-32" />
        <router-link to="/" class="px-6 py-2 font-semibold text-gray-900 hover:text-gray-700">
          Fiks-protokollvalidator
        </router-link>
        <router-link to="/NewTestSession" class="px-4 py-2 text-gray-700 hover:text-gray-900">
          Ny testsesjon
        </router-link>
      </div>
    </nav>

    <div class="container mx-auto mt-16 pt-12 px-8 bg-white max-w-[75%]">
      <router-view />
    </div>
  </div>
</template>

<script>
export default {
  name: "App"
};
</script>

<style>
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
