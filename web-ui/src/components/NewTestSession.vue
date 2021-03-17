<template>
  <div class="newTestSession">

    <div class="input-group mb-3">
      <div class="input-group-prepend">
        <span class="input-group-text" id="basic-addon3">FIKS-konto</span>
      </div>
      <input id="account-id" v-model="recipientId" type="text" class="form-control" placeholder="UUID" aria-label="UUID" aria-describedby="basic-addon1">
    </div>

    <div style="margin: 40px 0">
    <b-button style="float:right" variant="primary" v-on:click="runSelectedTests" v-if="!hasRun || running" :disabled="running">
      Kjør valgte tester
    </b-button>

    <b-form-group v-if="!hasRun">
      <span style="width: 100%; display: inline-block; vertical-align: middle;">
        <h3 style="float: left; width: 50%"> Tester </h3>
        <b-form-checkbox style="float:left" v-show="!hasRun"
          id="switch_selectAllTests"
          switch
          v-model="allTestsSelected"
          size="lg"
          aria-describedby="testCases"
          aria-controls="testCases"
          @change="toggleAll"
        >
          {{ allTestsSelected ? "Velg ingen" : "Velg alle" }}
        </b-form-checkbox>
      </span>

    <b-spinner label="Loading..." v-if="running || loading"></b-spinner>
    &nbsp;

      <b-form-checkbox-group
        switches
        id="test-list-all"
        v-model="selectedTests"
        name="test-list-all"
        size="lg"
        stacked
      >
        <TestCase v-for="testCase in testCases" :key="testCase.id"
          :testId="testCase.id"
          :testName="testCase.testName"
          :messageType="testCase.messageType"
          :payloadFileName="testCase.payloadFileName"
          :payloadAttachmentFileNames="testCase.payloadAttachmentFileNames"
          :description="testCase.description"
          :testStep="testCase.testStep"
          :operation="testCase.operation"
          :situation="testCase.situation"
          :expectedResult="testCase.expectedResult"
          :hasRun="hasRun"
          :isCollapsed="true"
        />
      </b-form-checkbox-group>
    </b-form-group>
    </div>
  </div>
</template>

<script>
import axios from "axios";
import TestCase from "./TestCase";

//const apiUrl = process.env.VUE_APP_API_URL;

export default {
  name: "newTestSession",

  components: {
    TestCase
  },

  data() {
    return {
      title: "Ny Testsesjon",
      testCases: [],
      resultData: [],
      running: false,
      hasRun: false,
      loading: false,
      hasLoaded: false,
      recipientId: null,
      selectedTests: [],
      allTestsSelected: false,
      tmpTests: []
    };
  },

  methods: {
    getTests: async function() {
      this.loading = true;
      const response = await axios.get(
          process.env.VUE_APP_API_URL + "/api/TestCases"
      );
      this.testCases = response.data;
      this.loading = false;
      this.hasLoaded = true;
    },

    runSelectedTests: async function() {
      this.running = true;

      if (this.selectedTests.length === 0) {
        this.running = false;
        return;
      }
      const params = {
        recipientId: this.recipientId,
        selectedTestCaseIds: this.selectedTests
      };

      const response = await axios.post(
          process.env.VUE_APP_API_URL + "/api/TestSessions",
        params
      );

      this.resultData = response.data;
      this.hasRun = true;
      this.running = false;
      this.$router.push({path: "/TestSession/" + response.data.id});
    },

    toggleAll(checked) {
      if (checked) {
        this.selectedTests = [];
        this.testCases.forEach(test => {
          this.tmpTests.push(test.id)
        });
        this.selectedTests = this.tmpTests;
        this.tmpTests = [];
      } else {
        this.selectedTests = [];
      }
    }
  },

  created () {
    this.getTests()
  },

  watch: {
    selectedTests(newVal) {
      if (newVal.length === 0) {
        this.allTestsSelected = false;
      } else if (newVal.length === this.testCases.length) {
        this.allTestsSelected = true;
      } else {
        this.allTestsSelected = false;
      }
    }
  }
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
.input-group {
  max-width: 430px;
}
</style>
