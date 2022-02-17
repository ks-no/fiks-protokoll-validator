<template>
  <div>
    <span>
      <button v-if="fileName !== null" class="btn btn-link" v-on:click="openWindow(fileUrl)"> 
        {{ fileName }}
      </button>
      <button v-if="fileName !== null" class="btn btn-primary" style="margin-left: 10px;padding: 1px 5px;" v-on:click="handleButtonOnClick()">
        Se innhold
      </button>
    </span> <br/>
    <b-modal
      v-model="modalIsOpen"
      :title="fileName"
      size="xl"
      button-size="sm"
      ok-only
      ok-variant="secondary"
      ok-title="Lukk"
      v-on:close="onClose()"
    >
      <div v-if="content">
        <div v-if="isTextContent">
          <ssh-pre
            :language="fileExtension"
            :label="fileExtension.toUpperCase()"
          >
            {{attemptDecodeBase64(content)}}
          </ssh-pre>
        </div>
        <div v-else>
          <b-embed :src="getTemporaryUrl(content)" />
        </div>
      </div>
      <div v-else>
        <b-spinner label="Loading ..."></b-spinner>
      </div>
    </b-modal>
  </div>
</template>

<script>
import SshPre from "simple-syntax-highlighter";
import "simple-syntax-highlighter/dist/sshpre.css";
const MimeTypes = require('mime-types')

export default {
  name: "PayloadFile",

  components: {
    SshPre
  },

  data() {
    return {
      modalIsOpen: false,
      fileExtension: null,
      isTextContent: false,
      temporaryUrl: null,
    };
  },
  props: {
    fileName: {
      type: String
    },
    content: {
      required: true
    },
    protocol: {
      type: String
    },
    testId: {
      type: String
    },
    fileUrl: {
      type: String,
    }
  },

  created() {
    this.fileExtension = this.getFileExtension(this.fileName);
    this.isTextContent = this.isTextFileExtension(this.fileExtension);
  },

  methods: {
    handleButtonOnClick() {
      this.modalIsOpen = true;
      if (!this.content) {
        this.$emit("get-content", this.isTextContent);
      }
    },
    getTemporaryUrl(content) {
      var blob = this.getAsBlob(content);
      const temporaryUrl = URL.createObjectURL(blob);
      this.temporaryUrl = temporaryUrl; // Used to revoke URL
      return temporaryUrl;
    },
    getFileExtension(fileName) {
      if (fileName) {
        var arr = fileName.split(".");
        return arr[arr.length - 1];
      }
    },
    isTextFileExtension(type) {
      return ["xml", "txt", "json", "html", "csv", "md"].indexOf(type) != -1;
    },

    onClose() {
      URL.revokeObjectURL(this.temporaryUrl);
      this.temporaryUrl = null;
    },
    attemptDecodeBase64(content){
      try {
        var decodedContent = atob(content);
        return decodedContent;
      } catch (error) {
        if (error.code === 5) {
          return content;
        }
      }
    },
    getAsBlob(content){
       if (content.type == "application/octet-stream") {
        const contentType = this.fileExtension === "pdf" ? { type: "application/pdf" } : null;
        return new Blob([content], contentType);
      } else {
        const decodedContent = atob(content);
        let binaryLen = decodedContent.length;

        let bytes = new Uint8Array(binaryLen);

        for (let i = 0; i < binaryLen; i++) {
            let ascii = decodedContent.charCodeAt(i);
            bytes[i] = ascii;
        }
        
        const mimeType = MimeTypes.lookup(this.fileExtension);
        const contentType = { type: mimeType };
        return new Blob([bytes], contentType);
      }
    },
    getAsFile(content) {
        var blob = this.getAsBlob(content);
        const temporaryUrl = URL.createObjectURL(blob);
        this.temporaryUrl = temporaryUrl; // Used to revoke URL
        var a = document.createElement("a");
        document.body.appendChild(a);
        a.style = "display: none";

        var url = window.URL.createObjectURL(blob);
        a.href = url;
        a.download = this.fileName;
        a.click();
        window.URL.revokeObjectURL(url);
    },
    openWindow: function (link) {
      if (!link) {
        this.getAsFile(this.content);
      }
      else{
          window.open(link, '_blank');
      }
    }
  }
};
</script>

<style scoped>
.btn {
  margin-bottom: 5px;
  padding: 0;
}
</style>
