<template>
  <div>
    <button class="btn btn-link" v-on:click="handleButtonOnClick()">
      {{ fileName }}
    </button>
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
            {{ content }}
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
      temporaryUrl: null
    };
  },

  props: {
    fileName: {
      required: true,
      type: String
    },
    content: {
      required: true
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
      const contentType =
        this.fileExtension === "pdf" ? { type: "application/pdf" } : null;
      const blob = new Blob([content], contentType);
      const temporaryUrl = URL.createObjectURL(blob);
      this.temporaryUrl = temporaryUrl; // Used to revoke URL
      return temporaryUrl;
    },
    getFileExtension(fileName) {
      var arr = fileName.split(".");
      return arr[arr.length - 1];
    },
    isTextFileExtension(type) {
      return ["xml", "txt", "json", "html", "csv", "md"].indexOf(type) != -1;
    },
    onClose() {
      URL.revokeObjectURL(this.temporaryUrl);
      this.temporaryUrl = null;
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
