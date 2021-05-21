<template>
  <div>
    <h2>Resultater</h2>

    <div class="input-group mb-3" v-if="!this.fetchError">
      <div class="input-group-prepend">
        <span class="input-group-text" style="align-self: baseline" id="basic-addon3">Adresse til denne testen:</span>
      </div>
      <input
        id="account-id"
        type="text"
        class="form-control"
        aria-label="UUID"
        aria-describedby="basic-addon1"
        v-model="sessionurl"
        readonly="true"
        ref="sessionUrlCopy"
      />
      <b-button
          variant="primary"
          v-on:click="copyURL()"
      >
        Kopier til utklippstavlen
      </b-button>
    </div>

    <div v-if="this.fetchError">
      <b-alert v-model="fetchError" variant="danger" dismissible>
          <p>Vi kunne ikke finne din test med SessionID:  {{this.$route.params.testSessionId}}.</p>
          <p>Statuskode: {{requestErrorStatusCode}}</p>
          <p>{{requestErrorMessage.title}}</p>
        </b-alert>
    </div>
    <b-spinner label="Loading..." v-if="loading"></b-spinner>
    &nbsp;

    <div v-if="testSession && testSession.fiksRequests">
      <Request
        v-for="request in testSession.fiksRequests"
        :key="request.testCase.id"
        :collapseId="request.messageGuid"
        :hasRun="true"
        :sentAt="request.sentAt"
        :responses="request.fiksResponses"
        :testCase="request.testCase"
        :isValidated="request.isFiksResponseValidated"
        :validationErrors="request.fiksResponseValidationErrors"
      />
    </div>
  </div>
</template>

<script>
import axios from "axios";
import Request from "./Request";

require("dotenv").config()

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
      this.loading = true;
      await axios.get(process.env.VUE_APP_API_URL + "/api/TestSessions/" + testSessionId)
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
</style>