<template>
  <div id="app2">
    <div class="my-10">
      <div class="w-full flex flex-col">
        <div class="float-left w-[70%]">
          <h3 class="text-xl font-semibold">
            Sist utførte testsesjon:
          </h3>
        </div>
        <div>
          <a :href="lastTestUrl" class="mt-2 inline-flex items-center text-blue-600 hover:text-blue-800">
            <svg class="w-5 h-5 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 16a4 4 0 01-.88-7.903A5 5 0 1115.9 6L16 6a5 5 0 011 9.9M9 19l3 3m0 0l3-3m-3 3V10"/>
            </svg>
            Din testsesjon utført - {{ lastTestDateTime }} ({{ (lastTestUrl || "").split("TestSession/")[1]}})
          </a>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import moment from "moment";
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

