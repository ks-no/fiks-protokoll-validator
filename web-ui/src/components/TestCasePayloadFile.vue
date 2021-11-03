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

require("dotenv").config()

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
    },
    protocol: {
      required: false,
      type: String
    },
    testName: {
      required: true,
      type: String
    }
  },
  
  methods: {
    getContent: function(isTextContent) {
      let endPointUrl = this.isAttachment
          ? this.operation + "" + this.situation + "/" + "Attachement/" + this.fileName
          : this.operation + "" + this.situation + "/payload";

      let settings = {
        responseType: isTextContent ? "text" : "blob",
        responseEncoding: isTextContent ? "utf-16" : "base64"
      };

      let apiUrl = process.env.VUE_APP_API_URL + "/api/TestCasePayloadFiles";
      let resourceUrl = apiUrl + "/" + this.protocol + "/" + endPointUrl;

      axios.get(resourceUrl, settings).then(response => {
        this.payloadFileContent = response.data;
      });
    }
  }
};
</script>