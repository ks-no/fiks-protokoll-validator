<template>
  <div>
    <BContainer>
      <BRow>
        <span class="w-full" :class="!hasRun ? 'flex items-center' : ''">
          <BCol sm="10">
            <span
              class="cursor-pointer"
              @click="isNotCollapsed = !isNotCollapsed"
              @keyup.enter="isNotCollapsed = !isNotCollapsed"
              @keyup.space="isNotCollapsed = !isNotCollapsed"
            >
              <div>
                <h5 class="flex items-center justify-between w-full text-base">
                  <BIconChevronRight
                    v-show="!isNotCollapsed"
                    class="mr-1.5"
                  />
                  <BIconChevronDown
                    v-show="isNotCollapsed"
                    class="mr-1.5"
                  />
                  <span class="flex-1">{{ testName }}</span>
                  <div
                    v-if="validState === 'invalid'"
                    class="text-center"
                  >
                    <BIconExclamationCircleFill
                      :class="'validState ' + validState"
                      title="Ugyldig"
                    />
                  </div>
                  <div
                    v-else-if="validState === 'valid'"
                    class="text-center"
                  >
                    <BIconCheckCircleFill
                      :class="'validState ' + validState"
                      title="Gyldig"
                    />
                  </div>
                  <div
                    v-else-if="validState === 'notValidated'"
                    class="text-center"
                  >
                    <BIconExclamationCircleFill
                      :class="'validState ' + validState"
                      title="Ikke validert"
                    />
                  </div>
                </h5>
              </div>
            </span>
          </BCol>
          <BCol sm="2" class="ml-auto">
            <BFormCheckbox
              v-if="!hasRun && supported"
              class="float-left"
              switch
              :value="testId"
            />
          </BCol>
        </span>
        <BCollapse
          class="ml-4"
          :visible="isNotCollapsed"
          :id="'collapse-' + operation + situation"
          style="width: 90%"
        >
          <BContainer fluid class="bg-gray-100 p-4 rounded">
            <BRow class="mb-1.5">
              <BCol cols="4">
                <strong class="float-left">Beskrivelse:</strong>
              </BCol>
              <BCol>{{ description }}</BCol>
            </BRow>
            <BRow class="mb-1.5">
              <BCol cols="4">
                <strong class="float-left">ID:</strong>
              </BCol>
              <BCol>{{ operation + situation }}</BCol>
            </BRow>
            <BRow class="mb-1.5">
              <BCol cols="4">
                <strong class="float-left">Meldingstype:</strong>
              </BCol>
              <BCol>{{ messageType }}</BCol>
            </BRow>
            <BRow class="mb-1.5">
              <BCol cols="4">
                <strong class="float-left">Meldingsinnhold:</strong>
              </BCol>
              <BCol>
                <TestCasePayloadFile
                  :testId="testId"
                  :testName="testName"
                  :fileName="payloadFileName"
                  :operation="operation"
                  :situation="situation"
                  :protocol="protocol"
                  :testSessionId="testSessionId"
                  :hasRun="hasRun"
                />
              </BCol>
            </BRow>
            <div v-if="payloadAttachmentFileNames">
              <BRow class="mb-1.5">
                <BCol cols="4">
                  <strong class="float-left">Vedlegg:</strong>
                </BCol>
                <BCol>
                  <div
                    v-for="attachmentFileName in payloadAttachmentFileNames.split(';')"
                    :key="attachmentFileName"
                  >
                    <TestCasePayloadFile
                      :testId="testId"
                      :testName="testName"
                      :operation="operation"
                      :situation="situation"
                      :protocol="protocol"
                      :fileName="attachmentFileName"
                      :isAttachment="true"
                      :testSessionId="testSessionId"
                      :hasRun="hasRun"
                    />
                  </div>
                </BCol>
              </BRow>
            </div>
          </BContainer>
        </BCollapse>
        <hr class="my-0 mx-[5%] border-0 border-t border-gray-200" />
      </BRow>
    </BContainer>
    <hr class="my-0 mx-[5%] border-0 border-t border-gray-200" />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import TestCasePayloadFile from './TestCasePayloadFile.vue'
import BContainer from '@/components/ui/BContainer.vue'
import BRow from '@/components/ui/BRow.vue'
import BCol from '@/components/ui/BCol.vue'
import BCollapse from '@/components/ui/BCollapse.vue'
import BFormCheckbox from '@/components/ui/BFormCheckbox.vue'
import BIconChevronRight from '@/components/ui/icons/BIconChevronRight.vue'
import BIconChevronDown from '@/components/ui/icons/BIconChevronDown.vue'
import BIconCheckCircleFill from '@/components/ui/icons/BIconCheckCircleFill.vue'
import BIconExclamationCircleFill from '@/components/ui/icons/BIconExclamationCircleFill.vue'

type ValidationState = 'valid' | 'invalid' | 'notValidated'

interface Props {
  testName: string
  testId: string
  messageType: string
  description: string
  testStep: string
  operation: string
  expectedResult: string
  situation: string
  payloadFileName?: string
  payloadAttachmentFileNames?: string
  supported?: boolean
  hasRun?: boolean
  validState?: ValidationState
  isCollapsed: boolean
  protocol: string
  testSessionId?: string
}

const props = withDefaults(defineProps<Props>(), {
  supported: false,
  hasRun: false,
  isCollapsed: true
})

const isNotCollapsed = ref(!props.isCollapsed)
</script>

<style scoped>
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
</style>
