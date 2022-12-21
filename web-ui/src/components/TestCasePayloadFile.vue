<template>
  <div>
    <span>
    <PayloadFile
      :fileName="fileName"
      :testId="testId"
      :protocol="protocol"
      :content="payloadFileContent"
      :fileUrl="isAttachment ? attachmentUrl : payloadUrl"
      v-on:get-content="isTextContent => getContent(isTextContent)"
    />
    </span>
    <span v-if="!hasRun && !isAttachment">
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
      payloadUrl: this.hasRun ? process.env.VUE_APP_API_URL + "/api/TestCasePayloadFiles" + "/" + this.testSessionId + "/" + this.testId + "/payload" : process.env.VUE_APP_API_URL + "/api/TestCasePayloadFiles" + "/" + this.testId + "/payload",
      attachmentUrl: process.env.VUE_APP_API_URL + "/api/TestCasePayloadFiles" + "/" + this.testId + "/Attachment/"+ this.fileName
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
    hasRun: {
      type: Boolean
    },
    testSessionId: {
      type: String,
    }
  },
  
  methods: {
    getContent: function(isTextContent) {
      let resourceUrl = this.isAttachment
          ? this.attachmentUrl
          : this.payloadUrl;

      let settings = {
        responseType: isTextContent ? "text" : "blob",
        responseEncoding: isTextContent ? "utf-16" : "base64"
      };

      axios.get(resourceUrl, settings).then(response => {
        this.payloadFileContent = response.data;
      });
    }
  }
};
</script>