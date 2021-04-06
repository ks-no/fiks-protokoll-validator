<template>
  <div>
    <h2>Resultater</h2>

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
      loading: false
    };
  },

  methods: {
    getTestSession: async function(testSessionId) {
      this.loading = true;
      const response = await axios.get(
          process.env.VUE_APP_API_URL + "/api/TestSessions/" + testSessionId
      );
      this.testSession = {
        ...response.data,
        fiksRequests: this.sortRequests(response.data.fiksRequests)
      };

      this.loading = false;
    },

    sortRequests: requests => {
      return requests
        ? requests.sort((a, b) => new Date(a.sentAt) - new Date(b.sentAt))
        : null;
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
img{
  margin-top: 50px;
}
</style>
