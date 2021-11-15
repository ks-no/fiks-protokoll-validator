<template>
  <div>
    <span>
    <PayloadFile
      :fileName="fileName"
      :testId="testId"
      :protocol="protocol"
      :content="payloadFileContent"
      v-on:get-content="isTextContent => getContent(isTextContent)"
    />
    </span>
    <span>
    <PayloadFileUpload 
      :fileName="fileName"
      :testId="testId"
      :protocol="protocol"
    />
      </span>
  </div>
</template>

<script>
import PayloadFile from "./PayloadFile.vue";
import axios from "axios";
import PayloadFileUpload from "@/components/PayloadFileUpload";

require("dotenv").config()

export default {
  name: "testCasePayloadFile",
  
  components: {
    PayloadFileUpload,
    PayloadFile
  },
  
  data() {
    return {
      payloadFileContent: null,
      fileExtension: null,
      apiBaseUrl: process.env.VUE_APP_API_URL + "/api/TestCasePayloadFiles",
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
    },
    testId: {
      required: true,
      type: String
    },
    fileUrl: {
      type: String
    }
  },
  
  methods: {
    getContent: function(isTextContent) {
      let endPointUrl = this.isAttachment
          ? this.operation + "" + this.situation + "/" + "Attachement/" + this.fileName
          : this.testId + "/payload";

      let settings = {
        responseType: isTextContent ? "text" : "blob",
        responseEncoding: isTextContent ? "utf-16" : "base64"
      };

      let resourceUrl = this.apiBaseUrl + "/" + this.protocol + "/" + endPointUrl;

      axios.get(resourceUrl, settings).then(response => {
        this.payloadFileContent = response.data;
      });
    }
  }
};
</script>