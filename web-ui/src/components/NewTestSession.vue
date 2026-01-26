<template>
  <div class="newTestSession max-w-7xl mx-auto px-4 py-6">
    <h1 class="text-3xl font-bold text-gray-900 mb-6">Ny Test Sesjon</h1>

    <div class="mb-6">
      <label for="account-id" class="block text-sm font-semibold text-gray-700 mb-2">
        FIKS-konto (UUID)
      </label>
      <input
        id="account-id"
        v-model="recipientId"
        type="text"
        class="w-full px-4 py-3 border-2 border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-blue-500 text-base"
        placeholder="f.eks. 76b035a9-2d73-48bf-b758-981195333191"
        aria-label="FIKS-konto UUID"
      />
      <p class="mt-2 text-sm text-gray-600">
        Angi UUID for FIKS-kontoen du vil kjøre tester mot
      </p>
    </div>

    <div class="mb-6">
      <label for="protocol-select" class="block text-sm font-semibold text-gray-700 mb-2">
        FIKS-protokoll
      </label>
      <b-form-select 
        id="protocol-select"
        v-model="selectedProtocol" 
        v-on:change="getTestsByProtocol" 
        :options="options" 
        class="w-full md:w-1/2"
      />
    </div>

    <div class="mb-6">
      <b-link
        :to="{
          name: 'newTestSession',
          query: { fikskonto: recipientId, fiksprotocol: selectedProtocol },
        }"
        class="text-blue-600 hover:text-blue-800 underline"
      >
        Direkte lenke
      </b-link>
    </div>
  
    <div class="mb-8">
      <b-form-group v-if="!hasRun">
        <div class="bg-white border border-gray-200 rounded-lg p-6 mb-4">
          <div class="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
            <div class="flex-1">
              <h3 class="text-2xl font-bold text-gray-900 mb-3">
                Tester
              </h3>
              <b-form-checkbox
                v-show="!hasRun"
                id="switch_supported"
                v-model="showNotSupportedTests"
                aria-describedby="testCases"
                aria-controls="testCases"
                @change="toggleAllSupportedTests"
              >
                Vis tester som ikke er implementert
              </b-form-checkbox>
            </div>
            
            <div class="flex flex-col sm:flex-row gap-3 items-stretch sm:items-center">
              <b-form-checkbox
                v-show="!hasRun"
                id="switch_selectAllTests"
                switch
                v-model="allTestsSelected"
                aria-describedby="testCases"
                aria-controls="testCases"
                @change="toggleAll"
                class="text-base"
              >
                {{ allTestsSelected ? "Velg ingen" : "Velg alle" }}
              </b-form-checkbox>
              
              <b-button
                variant="primary"
                v-on:click="runSelectedTests"
                v-if="!hasRun || running"
                :disabled="running || !fiksAccountPresent || selectedTests.length==0"
                class="px-6 py-2.5 text-base font-medium"
              >
                <b-spinner v-if="running" small class="mr-2"></b-spinner>
                {{ running ? 'Kjører...' : `Kjør valgte tester (${selectedTests.length})` }}
              </b-button>
            </div>
          </div>
        </div>
        
        <b-alert v-model="showRequestError" variant="danger" dismissible class="mb-4">
          <p class="font-semibold">Testing feilet med statuskode {{requestErrorStatusCode}}</p>
          <p class="text-sm mt-1">{{requestErrorMessage}}</p>
        </b-alert>
        
        <div v-if="loading && !running" class="flex items-center justify-center py-8">
          <b-spinner label="Laster tester..."></b-spinner>
          <span class="ml-3 text-gray-600">Laster tester...</span>
        </div>


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
import TestCase from "./TestCase.vue";
import BFormSelect from "@/components/ui/BFormSelect.vue";
import BLink from "@/components/ui/BLink.vue";
import BFormGroup from "@/components/ui/BFormGroup.vue";
import BFormCheckbox from "@/components/ui/BFormCheckbox.vue";
import BButton from "@/components/ui/BButton.vue";
import BSpinner from "@/components/ui/BSpinner.vue";
import BAlert from "@/components/ui/BAlert.vue";
import BFormCheckboxGroup from "@/components/ui/BFormCheckboxGroup.vue";

export default {
  name: "newTestSession",
  
  components: {
    BFormCheckboxGroup,
    BAlert,
    BSpinner,
    BButton,
    BFormCheckbox,
    BFormGroup,
    BLink,
    BFormSelect,
    TestCase
  },
  
  data() {
    const protocolOptions = [
      { value: "ingen", text: 'Velg en FIKS-protokoll'},
      { value: 'no.ks.fiks.arkiv.v1', text: 'no.ks.fiks.arkiv.v1' },
      { value: 'no.ks.fiks.plan.v2', text: 'no.ks.fiks.plan.v2'},
      { value: 'no.ks.fiks.matrikkelfoering.v2', text: 'no.ks.fiks.matrikkelfoering.v2'},
      { value: 'no.ks.fiks.saksfaser.v1', text: 'no.ks.fiks.saksfaser.v1'}
    ]
    const qproto = this.$route.query.fiksprotocol || protocolOptions[0].value;
    const selectedProtocol = (
      (qproto && protocolOptions.find((p) => p.value === qproto)) ||
      protocolOptions[0]
    ).value;
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
      selectedProtocol,
        options: protocolOptions 
    };
  },
  
  mounted() {
    this.recipientId = this.$route.query.fikskonto;
    if(this.$route.query.fikskonto) {
      this.fiksAccountPresent = true;
      if (this.selectedProtocol && this.selectedProtocol !== 'ingen') {
        this.getTestsByProtocol();
      }
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
      const response = await axios.get(import.meta.env.VITE_API_URL + "/api/TestCases", {withCredentials: true});
      this.testCases = response.data;
      this.loading = false;
      this.hasLoaded = true;
    },
    getTestsByProtocol: async function() {
      this.loading = true;
      const response = await axios.get(import.meta.env.VITE_API_URL + "/api/TestCases/Protocol/" + this.selectedProtocol, {withCredentials: true});
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
        selectedTestCaseIds: this.selectedTests,
        protocol: this.selectedProtocol
      };
      
      await axios.post(import.meta.env.VITE_API_URL + "/api/TestSessions", params, {withCredentials: true})
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

