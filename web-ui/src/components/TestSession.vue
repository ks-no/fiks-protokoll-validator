<template>
  <div class="max-w-7xl mx-auto px-4 py-6">
    <!-- Header -->
    <div class="mb-8">
      <h1 class="text-3xl font-bold text-gray-900 mb-2">Test Resultater</h1>
      <p class="text-gray-600">Session ID: {{ $route.params.testSessionId }}</p>
    </div>

    <!-- Session URL Copy Box -->
    <div v-if="!fetchError" class="mb-8 bg-blue-50 border border-blue-200 rounded-lg p-6">
      <label for="session-url" class="block text-sm font-semibold text-gray-700 mb-3">
        Adresse til denne testen:
      </label>
      <div class="flex gap-3">
        <input
          id="session-url"
          type="text"
          class="flex-1 px-4 py-2 border border-gray-300 rounded-lg bg-white text-gray-700 focus:ring-2 focus:ring-blue-500 focus:border-blue-500"
          v-model="sessionurl"
          readonly
          ref="sessionUrlCopy"
        />
        <b-button
          variant="primary"
          v-on:click="copyURL()"
          class="px-6 py-2 whitespace-nowrap"
        >
          📋 Kopier
        </b-button>
      </div>
    </div>

    <!-- Error Alert -->
    <div v-if="fetchError" class="mb-8">
      <b-alert v-model="fetchError" variant="danger" dismissible class="text-base">
        <p v-if="requestErrorStatusCode === 404" class="font-semibold mb-2">
          Vi kunne ikke finne din test med SessionID: {{$route.params.testSessionId}}
        </p>
        <p v-else class="font-semibold mb-2">
          Noe gikk galt med SessionID: {{$route.params.testSessionId}}
        </p>
        <p class="text-sm">Statuskode: {{requestErrorStatusCode}}</p>
        <p v-if="requestErrorStatusCode === 500" class="text-sm">{{requestErrorMessage}}</p>
        <p v-else class="text-sm">{{requestErrorMessage.title}}</p>
      </b-alert>
    </div>

    <!-- Loading Spinner -->
    <div v-if="loading" class="flex items-center justify-center py-12">
      <b-spinner label="Laster..."></b-spinner>
      <span class="ml-3 text-gray-600">Laster testresultater...</span>
    </div>

    <!-- Update Tests Section -->
    <div v-if="showUpdateButton()" class="mb-8 bg-white border border-gray-200 rounded-lg p-6">
      <div class="flex flex-col md:flex-row md:items-start md:justify-between gap-6">
        <div class="flex-1">
          <h3 class="text-lg font-semibold text-gray-900 mb-3">Statusforklaring</h3>
          <div class="space-y-2 text-sm text-gray-700">
            <p class="flex items-center gap-2">
              <b-icon-exclamation-circle-fill class="validState invalid" title="Ugyldig" />
              <span><strong>Ugyldig:</strong> Testen har feil eller mangler</span>
            </p>
            <p class="flex items-center gap-2">
              <b-icon-exclamation-circle-fill class="validState notValidated" title="Ikke validert" />
              <span><strong>Ikke validert:</strong> Har ikke mottatt svar</span>
            </p>
          </div>
          <p class="mt-4 text-sm text-gray-600">
            Oppdater testene for å validere på nytt ved rød eller gul status
          </p>
        </div>
        
        <div class="flex-shrink-0">
          <b-button 
            variant="primary"
            v-on:click="getTestSession($route.params.testSessionId)"
            class="px-6 py-2.5 text-base font-medium"
          >
            🔄 Oppdater tester
          </b-button>
        </div>
      </div>
    </div>

    <!-- Test Results -->
    <div v-if="testSession && testSession.fiksRequests" class="space-y-4">
      <h2 class="text-2xl font-bold text-gray-900 mb-4">Test Resultater</h2>
      <Request
        v-for="request in testSession.fiksRequests"
        :key="request.messageGuid"
        :collapseId="request.messageGuid"
        :hasRun="true"
        :sentAt="request.sentAt"
        :responses="request.fiksResponses"
        :testCase="request.testCase"
        :customPayloadFilename="request.customPayloadFile == null ? null : request.customPayloadFile.filename"
        :isValidated="request.isFiksResponseValidated"
        :validationErrors="request.fiksResponseValidationErrors"
        :testSessionId="testSession.id"
      />
    </div>
  </div>
</template>

<script>
import axios from "axios";
import Request from "./Request.vue";

export default {
  name: "TestSession",
  
  components: {
    Request
  },
  
  data() {
    return {
      testSession: null,
      loading: false,
      fetchError: false,
      sessionurl: window.location.href,
      requestErrorStatusCode: null,
    };
  },
  
  methods: {
    getTestSession: async function(testSessionId) {
      this.testSessionId = testSessionId;
      this.testSession = null;
      this.loading = true;
      await axios.get(import.meta.env.VITE_API_URL + "/api/TestSessions/" + testSessionId)
      .then(response => {
        if (response.status == 200) {
          this.testSession = {
        ...response.data,
        fiksRequests: this.sortRequests(response.data.fiksRequests)
      };
        localStorage.validatorLastTest = this.sessionurl;
        localStorage.createdAt = response.data.createdAt; 
        this.loading = false;
        }
      }).catch(error => {
          console.log("error: ", error);
          this.loading = false;
          this.requestErrorStatusCode = error.response.status;
          this.requestErrorMessage = error.response.data;
          this.fetchError = true;
      });
    },
    showUpdateButton() {
      let result = false;
      if (this.testSession && this.testSession.fiksRequests != null) {
        this.testSession.fiksRequests.forEach(request => {
          if (request.isFiksResponseValidated) {
            if (request.fiksResponseValidationErrors.length >0) {
              result = true;
            }
          }
          else {
            result = true;
          }
        });
      }
      return result;
    },
    
    sortRequests: requests => {
      return requests
          ? requests.sort((a, b) => new Date(a.sentAt) - new Date(b.sentAt))
          : null;
    },
    
    copyURL() {
      const copyText = this.$refs.sessionUrlCopy;
      copyText.select();
      document.execCommand("copy");
      alert("Adressen er kopiert");
    }
  },
  
  created() {
    if (this.$route.params.testSessionId) {
      this.getTestSession(this.$route.params.testSessionId);
    }
  }
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
img {
  margin-top: 50px;
}
svg.validState {
  font-size: 24px;
  margin-right: 6px;
}

svg.notValidated {
  color: rgb(231, 181, 42);
}

svg.invalid {
  color: #cc3333;
}
</style>