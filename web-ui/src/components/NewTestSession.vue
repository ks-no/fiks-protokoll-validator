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
    <div class="input-group mb-3">
    <div class="input-group-prepend">
      <span class="input-group-text" id="basic-addon3">FIKS-protokoll</span>
    </div>
      <b-form-select v-model="selectedProtocol" v-on:change="getTestsByProtocol" :options="options" style="width:30%"></b-form-select>
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
              :disabled="running || !fiksAccountPresent || selectedTests.length==0"
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
        <b-alert v-model="showRequestError" variant="danger" dismissible>
          <p>Testing feilet med statuskode {{requestErrorStatusCode}}. Melding: {{requestErrorMessage}}</p>
        </b-alert>
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
            v-bind:key="testCase.testId"
            :testId="testCase.testId"
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
            :protocol="testCase.protocol"
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

//require("dotenv").config()

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
      recipientId: this.$route.query.fikskonto,
      selectedTests: [],
      fiksAccountPresent: false,
      allTestsSelected: false,
      showNotSupportedTests: false,
      showRequestError: false,
      requestErrorStatusCode: 0,
      requestErrorMessage: "",
      tmpTests: [],
      selectedProtocol: "ingen",
        options: [
          { value: "ingen", text: 'Velg en FIKS-protokoll'},
          { value: 'no.ks.fiks.arkiv.v1', text: 'no.ks.fiks.arkiv.v1' },
          { value: 'no.ks.fiks.politisk.behandling.klient.v1', text: 'no.ks.fiks.politisk.behandling.v1 (klient)' },
          { value: 'no.ks.fiks.politisk.behandling.tjener.v1', text: 'no.ks.fiks.politisk.behandling.v1 (tjener)'},
          { value: 'no.ks.fiks.gi.plan.klient', text: 'no.ks.fiks.gi.plan.klient'}
        ]
    };
  },
  
  mounted() {
    this.recipientId = this.$route.query.fikskonto;
    if(this.$route.query.fikskonto) {
      this.fiksAccountPresent = true;
    }
  },
  
  computed: {
    computedTestCases: function() {
      if(this.selectedProtocol === 'ingen') return;
      if (!this.showNotSupportedTests) {
        return this.testCases.filter(testCase => {
          return testCase.supported === !this.showNotSupportedTests;
        });
      } else {
        return this.testCases;
      }
    }
  },
  
  methods: {
    getTests: async function() {
      this.loading = true;
      const response = await axios.get(process.env.VUE_APP_API_URL + "/api/TestCases", {withCredentials: true});
      this.testCases = response.data;
      this.loading = false;
      this.hasLoaded = true;
    },
    getTestsByProtocol: async function() {
      this.loading = true;
      const response = await axios.get(process.env.VUE_APP_API_URL + "/api/TestCases/Protocol/" + this.selectedProtocol, {withCredentials: true});
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
      
      await axios.post(process.env.VUE_APP_API_URL + "/api/TestSessions", params, {withCredentials: true})
      .then(response => {
         if (response.status === 201) {
                this.resultData = response.data;
                this.hasRun = true;
                this.running = false;
            this.$router.push({ path: "/TestSession/" + response.data.id });        
        }
      })
      .catch(error => {
        this.running = false;
        this.requestErrorStatusCode = error.response.status;
        this.requestErrorMessage = error.response.data;
        this.showRequestError = true;
      })
    },
    
    toggleAll(checked) {
      if (checked) {
        this.selectedTests = [];
        const tests = this.testCases.filter(testCase => {
          return testCase.supported === true && testCase.protocol === this.selectedProtocol;
        });
        tests.forEach(test => {
          this.tmpTests.push(test.testId);
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
    //this.getTests();
  },
  watch: {
    selectedTests(newVal) {
      const length = this.testCases.filter(testCase => {
        return testCase.supported === true && testCase.protocol === this.selectedProtocol;
      }).length;
      if (newVal.length === 0) {
        this.allTestsSelected = false;
      } else if (newVal.length === length) {
        this.allTestsSelected = true;
      } else {
        this.allTestsSelected = false;
      }
    },
    recipientId(newVal){
        this.fiksAccountPresent = newVal.length > 0;
    },
    selectedProtocol(){
      this.selectedTests = [];
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