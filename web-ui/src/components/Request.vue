<template>
  <li class="list-group-item">
    <TestCase
      :testId="testCase.testId"
      :testName="testCase.testName"
      :messageType="testCase.messageType"
      :payloadFileName="customPayloadFilename != null ? customPayloadFilename : testCase.payloadFileName"
      :payloadAttachmentFileNames="testCase.payloadAttachmentFileNames"
      :description="testCase.description"
      :expectedResult="testCase.expectedResult"
      :situation="testCase.situation"
      :operation="testCase.operation"
      :testStep="testCase.testStep"
      :protocol="testCase.protocol"
      :hasRun="hasRun"
      :validState="validState"
      :isCollapsed="isCollapsed"
      :testSessionId="testSessionId"
    />
    <b-collapse :visible="!isCollapsed" :id="'collapse-' + testCase.operation+ '' + testCase.situation">
      <b-container fluid>
        <b-row style="margin-bottom: 25px">
          <b-col cols="2"> <strong class="header">Sendt: </strong> </b-col>
          <b-col> {{ formatDate(sentAt) }} </b-col>
        </b-row>
        <div v-if="testCase.fiksResponseTests != null">
          <b-row style="margin-bottom: 5px">
            <b-col cols="2">
              <strong class="header">Testspørringer:</strong>
            </b-col>
            <b-col>
              <a
                v-for="fiksResponseTest in testCase.fiksResponseTests"
                :key="fiksResponseTest.id"
              >
                {{
                  testCase.protocol != "no.ks.fiks.politisk.behandling.klient.v1" ? 
                   fiksResponseTest.payloadQuery +
                    (fiksResponseTest.valueType === 0
                      ? "/text(" + fiksResponseTest.expectedValue + ")"
                      : "[@xsi:type='" + fiksResponseTest.expectedValue + "']")
                  :
                  fiksResponseTest.payloadQuery +
                    (fiksResponseTest.valueType === 0
                      ? "/forventet verdi(" + fiksResponseTest.expectedValue + ")"
                      : "[key='" + fiksResponseTest.expectedValue + "']")
                }}
                <br />
              </a>
            </b-col>
          </b-row>
        </div>
      </b-container>
      <b-card>
        <ul class="list-group">
          <h6>
            <strong>Svarmeldinger</strong>
          </h6>
          {{
            validState !== "notValidated" ? "" : "Ingen svarmeldinger funnet.."
          }}
          <Response
            v-for="response in responses"
            :key="response.id"
            :collapseId="'collapse-' + response.id"
            :receivedAt="response.receivedAt"
            :messageType="response.type"
            :payloads="response.fiksPayloads"
            :payloadContent="response.payloadContent"
          />
        </ul>
      </b-card>
      <b-card>
        <h6><strong>Testresultat</strong></h6>
        <div v-if="validState === 'notValidated'" class="flex-result-item">
          <b-icon-exclamation-circle-fill
            :class="'validState ' + validState"
            title="Ikke validert"
          />
          <label> Validering er ikke utført </label>
        </div>
        <div v-else-if="validState === 'valid'">
          <label> Validering utført uten feil! </label>
        </div>
        <div v-else-if="validState === 'invalid'">
          <label
            class="flex-result-item"
            v-for="error in validationErrors"
            :key="error"
          >
            <b-icon-exclamation-circle-fill
              :class="'validState ' + validState"
              title="Ugyldig"
            />
            {{
              error
                .replace("/", "&lt;")
                .replace(/\//g, "> &lt;")
                .replace(" i svar", "> i svar")
            }}
          </label>
        </div>
      </b-card>
    </b-collapse>
  </li>
</template>

<script>
import moment from "moment";
import TestCase from "./TestCase.vue";
import Response from "./Response.vue";

export default {
  name: "Request",

  components: {
    TestCase,
    Response
  },

  data() {
    return {
      isCollapsed: true,
      validState: null
    };
  },

  props: {
    collapseId: {
      required: true
    },
    hasRun: {
      required: true,
      type: Boolean
    },
    sentAt: {
      required: true,
      type: String
    },
    testCase: {
      required: true,
      type: Object
    },
    responses: {
      type: Array,
      required: true
    },
    isValidated: {
      type: Boolean
    },
    validationErrors: {
      type: Array
    },
    testSessionId: {
      type: String
    },
    customPayloadFilename: {
      type: String
    }
  },

  methods: {
    formatDate: date => {
      return moment(date).format("DD.MM.YYYY [kl.]HH:mm:ss.SSS");
    }
  },

  beforeMount() {
    if (!this.isValidated) {
      this.validState = "notValidated";
    } else {
      this.validState =
        typeof this.validationErrors !== "undefined" &&
        this.validationErrors.length > 0
          ? "invalid"
          : "valid";
    }
  }
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style scoped>
.flex-result-item {
  display: flex;
  justify-content: flex-start;
  width: 100%;
  vertical-align: -webkit-baseline-middle;
}

svg.validState {
  font-size: 24px;
  margin-right: 6px;
}

svg.notValidated {
  color: rgb(231, 181, 42);
}

svg.invalid {
  color: #cc3333;
}

.ext-left {
  float: left;
  width: 85%;
}

.ext-right {
  float: right;
}

.grow-right {
  width: 100%;
}

li span {
  display: inline-block;
  vertical-align: middle;
}

b-form-checkbox {
  float: right;
  widows: 15%;
}

strong.header {
  float: right;
}
</style>
