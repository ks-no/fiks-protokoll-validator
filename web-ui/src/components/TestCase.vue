<template>
  <div>
    <b-container>
      <b-row>
        <span class="grow-right" :class="!hasRun ? 'notHasrun' : ''">
          <b-col sm="10">
            <span
              v-on:click="isNotCollapsed = !isNotCollapsed"
              v-on:keyup.enter="isNotCollapsed = !isNotCollapsed"
              v-on:keyup.space="isNotCollapsed = !isNotCollapsed"
              v-b-toggle="'collapse-' + operation+ '' + situation"
            >
              <div>
                <h5 class="flex-title">
                  <b-icon-chevron-right
                    v-show="!isNotCollapsed"
                    class="expand-icon"
                  />
                  <b-icon-chevron-down
                    v-show="isNotCollapsed"
                    class="expand-icon"
                  />
                  <span> {{ testName }}</span>
                  <div
                    v-if="validState === 'invalid'"
                    class="text-center">
                    <b-icon-exclamation-circle-fill
                    :class="'validState ' + validState"
                    title="Ugyldig"
                  />
                  </div>
                  <div
                    v-else-if="validState === 'valid'"
                    class="text-center">
                  <b-icon-check-circle-fill
                    :class="'validState ' + validState"
                    title="Gyldig"
                  />
                  </div>
                  <div
                    v-else-if="validState === 'notValidated'"
                    class="text-center"
                    >
                  <b-icon-exclamation-circle-fill
                    :class="'validState ' + validState"
                    title="Ikke validert"
                  />
                 </div>
                </h5>
              </div>
            </span>
          </b-col>
          <b-col sm="2" style="margin-left: auto">
            <b-form-checkbox
              v-if="!hasRun && supported"
              class="ext-left"
              switch
              size="lg"
              :value="testName"
            />
          </b-col>
        </span>
        <b-collapse
          class="ml-4"
          :visible="!isCollapsed"
          :id="'collapse-' + operation+ '' + situation"
          style="width: 90%"
        >
          <b-container fluid class="collapsecolor">
            <b-row style="margin-bottom: 5px">
              <b-col cols="4">
                <strong class="header" style="float: left">Beskrivelse:</strong>
              </b-col>
              <b-col> {{ description }} </b-col>
            </b-row>
            <b-row style="margin-bottom: 5px">
              <b-col cols="4">
                <strong class="header" style="float: left">ID:</strong>
              </b-col>
              <b-col> {{ operation + "" + situation }} </b-col>
            </b-row>
            <b-row style="margin-bottom: 5px">
              <b-col cols="4">
                <strong class="header" style="float: left"
                  >Meldingstype:</strong
                >
              </b-col>
              <b-col> {{ messageType }} </b-col>
            </b-row>
            <b-row style="margin-bottom: 5px">
              <b-col cols="4">
                <strong class="header" style="float: left">
                  Meldingsinnhold:
                </strong>
              </b-col>
              <b-col>
                <TestCasePayloadFile
                  :testName="testName"
                  :fileName="payloadFileName"
                  :operation="operation"
                  :situation="situation"
                  :protocol="protocol"
                />
              </b-col>
            </b-row>
            <div v-if="payloadAttachmentFileNames">
              <b-row style="margin-bottom: 5px">
                <b-col cols="4">
                  <strong class="header" style="float: left">Vedlegg:</strong>
                </b-col>
                <b-col>
                  <div
                    v-for="attachmentFileName in payloadAttachmentFileNames.split(
                      ';'
                    )"
                    :key="attachmentFileName"
                  >
                    <TestCasePayloadFile
                      :operation="operation"
                      :situation="situation"
                      :protocol="protocol"
                      :fileName="attachmentFileName"
                      :isAttachment="true"
                    />
                  </div>
                </b-col>
              </b-row>
            </div>
          </b-container>
        </b-collapse>
        <hr />
      </b-row>
    </b-container>
    <hr />
  </div>
</template>

<script>
import TestCasePayloadFile from "./TestCasePayloadFile.vue";

export default {
  name: "testCase",

  components: {
    TestCasePayloadFile,
  },

  data() {
    return {
      isNotCollapsed: !this.isCollapsed,
      payloadFileContent: null,
      payloadFileContentIsLoaded: false,
    };
  },

  props: {
    testName: {
      required: true,
      type: String,
    },
    messageType: {
      required: true,
      type: String,
    },
    description: {
      required: true,
      type: String,
    },
    testStep: {
      required: true,
      type: String,
    },
    operation: {
      required: true,
      type: String,
    },
    expectedResult: {
      required: true,
      type: String,
    },
    situation: {
      required: true,
      type: String,
    },
    payloadFileName: {
      type: String,
    },
    payloadAttachmentFileNames: {
      type: String,
    },
    supported: {
      type: Boolean,
    },
    hasRun: {
      type: Boolean,
    },
    validState: {
      type: String,
    },
    isCollapsed: {
      required: true,
      type: Boolean,
    },
    protocol:{
      required:true,
      type: String,
    }
  },
};
</script>

<style scoped>
strong.header {
  float: right;
}

.flex-title {
  display: flex;
  justify-content: space-between;
  width: 100%;
  align-items: center;
  font-size: 16px;
}
.test-case-id {
  font-size: 14px;
}

.flex-title > * {
  display: block;
}

.flex-title span {
  flex: 1;
}

.expand-icon {
  margin-right: 6px;
}

svg.validState {
  font-size: 24px;
}

svg.valid {
  color: green;
}

svg.invalid {
  color: #cc3333;
}

svg.notValidated {
  color: rgb(231, 181, 42);
}

.collapsecolor {
  background-color: rgb(241, 241, 241);
}

.ext-right {
  float: right;
}

.grow-right {
  width: 100%;
}

.grow-right.notHasrun > span,
.grow-right.notHasrun > div {
  display: inline-block;
  vertical-align: middle;
}

hr {
  margin-top: 0px;
  margin-bottom: 0px;
  margin-left: 5%;
  margin-right: 10%;
  border: 0;
  border-top: 1px solid rgba(0, 0, 0, 0.1);
}

div span {
  display: block;
  line-height: 36px;
}

b-form-checkbox.ext-left {
  float: left;
}
</style>
