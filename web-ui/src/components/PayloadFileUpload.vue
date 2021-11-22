<template>
  <div v-if="fileName !== null">
    <form enctype="multipart/form-data">
      <div class="input-group">
        <div class="custom-file">
          <input type="file" ref="file" v-on:change="handleFileUpload()" class="custom-file-input" id="payloadUpload"/>
          <label class="custom-file-label" for="payloadUpload">{{fileUploadText}}</label>
        </div>
        <div class="input-group-append">
          <button :disabled="file === ''" v-on:click="submitFile()" class="btn btn-primary" title="Last opp egen melding"><span class="fas fa-upload fa-lg"></span><span>Last opp</span></button>
        </div>
      </div>
    </form>
  </div>
</template>

<script>

import axios from 'axios';

export default {
  name: 'PayloadFileUpload',
  props: {
    testId: String,
    fileName: String,
    protocol: String, 
  },
  data() {
    return {
      file: '',
      fileUploadText: 'Velg egendefinert melding',
      apiBaseUrl: process.env.VUE_APP_API_URL + "/api/TestCasePayloadFiles",
    };
  },
  methods: {
    handleFileUpload() {
      this.file = this.$refs.file.files[0];
      this.fileUploadText = this.$refs.file.files[0].name;
    },
    submitFile() {
      let formData = new FormData();
      formData.append('file', this.file);

      axios.post(`${this.apiBaseUrl}/${this.testId}/payload`,
          formData,
          {
            headers: {
              'Content-Type': 'multipart/form-data',
            },
            withCredentials: true,
          }
      ).then(function() {
        console.log("Success uploading file.")
      }).catch(function() {
        console.log("Uploading file failed!")
      })
    },
  }
};
</script>