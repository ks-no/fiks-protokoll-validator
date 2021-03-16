<template>
  <li class="list-group-item">
    <span class="grow-right">
      <div
        class="ext-left"
        v-on:click="isCollapsed = !isCollapsed"
        v-b-toggle="'collapse-' + collapseId"
      >
        <strong>
          <b-icon-file-plus v-if="isCollapsed" shift-h="-5" />
          <b-icon-file-minus v-if="!isCollapsed" shift-h="-5" />
          {{ messageType }}
        </strong>
      </div>
    </span>
    <b-collapse :id="'collapse-' + collapseId" class="mt-2">
      <b-card>
        <p><strong>Mottatt: </strong>{{ formatDate(receivedAt) }}</p>
        <p v-if="payload">
          <strong>Innhold: </strong>
          <PayloadFile :fileName="payload" :content="payloadContent" />
        </p>
      </b-card>
    </b-collapse>
  </li>
</template>

<script>
import moment from "moment";
import PayloadFile from "./PayloadFile.vue";

export default {
  name: "Response",

  components: {
    PayloadFile
  },

  data() {
    return {
      isCollapsed: true,
      payloadUrl: null
    };
  },

  props: {
    collapseId: {
      required: true
    },
    receivedAt: {
      type: String
    },
    messageType: {
      type: String
    },
    payload: {
      type: String
    },
    payloadContent: {
      type: String
    }
  },

  methods: {
    formatDate: function(date) {
      return moment(date).format("DD.MM.YYYY HH:mm:ss.SSS");
    }
  }
};
</script>
