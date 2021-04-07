<template>
  <div class="newTestSession">
    <div class="input-group mb-3">
      <div class="input-group-prepend">
        <span class="input-group-text" id="basic-addon3">FIKS-konto</span>
      </div>
      <input
        id="account-id"
        v-model="recipientId"
        type="text"
        class="form-control"
        placeholder="UUID"
        aria-label="UUID"
        aria-describedby="basic-addon1"
      />
    </div>
    <div style="margin: 40px 0">
      <b-form-group v-if="!hasRun">
        <span
          style="width: 100%; display: inline-block; vertical-align: middle;"
        >
          <div style="float: left; width: 70%">
            <h3>
              Tester
            </h3>
            <b-form-checkbox
              v-show="!hasRun"
              id="switch_supported"
              v-model="showNotSupportedTests"
              size="sm"
              aria-describedby="testCases"
              aria-controls="testCases"
              @change="toggleAllSupportedTests"
            >
              {{ "Vis tester som ikke er implementert" }}
            </b-form-checkbox>
          </div>
          <div class="radioAndButton" style="float: left; width: 30%">
            <b-button
              variant="primary"
              v-on:click="runSelectedTests"
              v-if="!hasRun || running"
              :disabled="running"
              class="runAllButton"
            >
              Kjør valgte tester
            </b-button>
            <b-form-checkbox
              v-show="!hasRun"
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
          </div>
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
          <TestCase
            v-for="testCase in computedTestCases"
            v-bind:key="testCase.testName"
            :testName="testCase.testName"
            :messageType="testCase.messageType"
            :payloadFileName="testCase.payloadFileName"
            :payloadAttachmentFileNames="testCase.payloadAttachmentFileNames"
            :description="testCase.description"
            :testStep="testCase.testStep"
            :operation="testCase.operation"
            :situation="testCase.situation"
            :expectedResult="testCase.expectedResult"
            :supported="testCase.supported"
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

require("dotenv").config()

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
      showNotSupportedTests: false,
      tmpTests: []
    };
  },
  
  computed: {
    computedTestCases: function() {
      if (!this.showNotSupportedTests) {
        return this.testCases.filter(testCase => {
          return testCase.supported === !this.showNotSupportedTests;
        });
      } else return this.testCases;
    }
  },
  
  methods: {
    getTests: async function() {
      this.loading = true;
      const response = await axios.get(process.env.VUE_APP_API_URL + "/api/TestCases");
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
      
      const response = await axios.post(process.env.VUE_APP_API_URL + "/api/TestSessions", params);
      this.resultData = response.data;
      this.hasRun = true;
      this.running = false;
      this.$router.push({ path: process.env.VUE_APP_API_URL + "/TestSession/" + response.data.id });
    },
    
    toggleAll(checked) {
      if (checked) {
        this.selectedTests = [];
        const tests = this.testCases.filter(testCase => {
          return testCase.supported === true;
        });
        tests.forEach(test => {
          this.tmpTests.push(test.testName);
        });
        this.selectedTests = this.tmpTests;
        this.tmpTests = [];
      } else {
        this.selectedTests = [];
      }
    },
    
    toggleAllSupportedTests(checked) {
      this.showNotSupportedTests = checked;
    }
  },
  created() {
    this.getTests();
  },
  watch: {
    selectedTests(newVal) {
      const length = this.testCases.filter(testCase => {
        return testCase.supported === true;
      }).length;
      if (newVal.length === 0) {
        this.allTestsSelected = false;
      } else if (newVal.length === length) {
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
.radioAndButton {
  display: flex;
  flex-direction: column;
  text-align: center;
  margin-bottom: 8px;
}
.runAllButton {
  margin-bottom: 8px;
}
</style>