<template>
  <div>
    <div class="w-full px-4">
      <div class="flex flex-wrap">
        <span class="w-full" :class="!hasRun ? 'flex items-center' : ''">
          <div class="flex-1 sm:w-10/12">
            <span
              class="cursor-pointer"
              @click="isNotCollapsed = !isNotCollapsed"
              @keyup.enter="isNotCollapsed = !isNotCollapsed"
              @keyup.space="isNotCollapsed = !isNotCollapsed"
            >
              <div>
                <h5 class="flex items-center justify-between w-full text-base">
                  <font-awesome-icon
                    v-show="!isNotCollapsed"
                    icon="fa-solid fa-chevron-right"
                    class="mr-1.5"
                  />
                  <font-awesome-icon
                    v-show="isNotCollapsed"
                    icon="fa-solid fa-chevron-down"
                    class="mr-1.5"
                  />
                  <span class="flex-1">{{ testName }}</span>
                  <div
                    v-if="validState === 'invalid'"
                    class="text-center"
                  >
                    <font-awesome-icon
                      icon="fa-solid fa-circle-exclamation"
                      :class="'validState ' + validState"
                      title="Ugyldig"
                    />
                  </div>
                  <div
                    v-else-if="validState === 'valid'"
                    class="text-center"
                  >
                    <font-awesome-icon
                      icon="fa-solid fa-circle-check"
                      :class="'validState ' + validState"
                      title="Gyldig"
                    />
                  </div>
                  <div
                    v-else-if="validState === 'notValidated'"
                    class="text-center"
                  >
                    <font-awesome-icon
                      icon="fa-solid fa-circle-exclamation"
                      :class="'validState ' + validState"
                      title="Ikke validert"
                    />
                  </div>
                </h5>
              </div>
            </span>
          </div>
          <div class="sm:w-2/12 ml-auto">
            <UiCheckbox
              v-if="!hasRun && supported"
              class="float-left"
              switch
              :value="testId"
            />
          </div>
        </span>
        <div
          v-show="isNotCollapsed"
          class="ml-4"
          :id="'collapse-' + operation + situation"
          style="width: 90%"
        >
          <div class="w-full bg-gray-100 p-4 rounded grid grid-cols-[1fr_2fr] gap-y-1.5">
            <strong class="float-left">Beskrivelse:</strong>
            <span>{{ description }}</span>
            <strong class="float-left">ID:</strong>
            <span>{{ operation + situation }}</span>
            <strong class="float-left">Meldingstype:</strong>
            <span>{{ messageType }}</span>
            <strong class="float-left">Meldingsinnhold:</strong>
            <span>
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
            </span>
            <template v-if="payloadAttachmentFileNames">
              <strong class="float-left">Vedlegg:</strong>
              <span>
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
              </span>
            </template>
          </div>
        </div>
        <hr class="my-0 mx-[5%] border-0 border-t border-gray-200" />
      </div>
    </div>
    <hr class="my-0 mx-[5%] border-0 border-t border-gray-200" />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import TestCasePayloadFile from './TestCasePayloadFile.vue'
import type { ValidationState } from '@/types'

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
