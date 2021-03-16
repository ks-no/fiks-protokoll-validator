<template>
  <PayloadFile
    :fileName="fileName"
    :content="payloadFileContent"
    v-on:get-content="isTextContent => getContent(isTextContent)"
  />
</template>

<script>
import PayloadFile from "./PayloadFile.vue";
import axios from "axios";

export default {
  name: "testCasePayloadFile",

  components: {
    PayloadFile
  },

  data() {
    return {
      payloadFileContent: null,
      fileExtension: null
    };
  },

  props: {
    fileName: {
      required: true,
      type: String
    },
    isAttachment: {
      required: false,
      type: Boolean
    },
    operation: {
      required: false,
      type: String
    },
    situation: {
      required: false,
      type: String
    }
  },

  methods: {
    getContent: function(isTextContent) {
      var endPointUrl = this.isAttachment
        ? this.operation + "" + this.situation + "/" + this.fileName
        : this.operation + "" + this.situation;

      var settings = {
        responseType: isTextContent ? "text" : "blob",
        responseEncoding: isTextContent ? "utf-16" : "base64"
      };

      var apiUrl = "/api/TestCasePayloadFiles";
      var resourceUrl = apiUrl + "/" + endPointUrl;
      axios.get(resourceUrl, settings).then(response => {
        this.payloadFileContent = response.data;
      });
    }
  }
};
</script>
