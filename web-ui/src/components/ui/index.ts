import type { App, Plugin } from 'vue'

// UI components
import BButton from './BButton.vue'
import BAlert from './BAlert.vue'
import BSpinner from './BSpinner.vue'
import BCard from './BCard.vue'
import BRow from './BRow.vue'
import BCol from './BCol.vue'
import BCollapse from './BCollapse.vue'
import BContainer from './BContainer.vue'
import BFormGroup from './BFormGroup.vue'
import BFormCheckbox from './BFormCheckbox.vue'
import BFormCheckboxGroup from './BFormCheckboxGroup.vue'
import BFormSelect from './BFormSelect.vue'
import BLink from './BLink.vue'
import BModal from './BModal.vue'

// Icon components
import BIconExclamationCircleFill from './icons/BIconExclamationCircleFill.vue'
import BIconCheckCircleFill from './icons/BIconCheckCircleFill.vue'
import BIconChevronDown from './icons/BIconChevronDown.vue'
import BIconChevronRight from './icons/BIconChevronRight.vue'
import BIconFilePlus from './icons/BIconFilePlus.vue'
import BIconFileMinus from './icons/BIconFileMinus.vue'

const UIComponents: Plugin = {
  install(app: App) {
    app.component('BButton', BButton)
    app.component('BAlert', BAlert)
    app.component('BSpinner', BSpinner)
    app.component('BCard', BCard)
    app.component('BRow', BRow)
    app.component('BCol', BCol)
    app.component('BCollapse', BCollapse)
    app.component('BContainer', BContainer)
    app.component('BFormGroup', BFormGroup)
    app.component('BFormCheckbox', BFormCheckbox)
    app.component('BFormCheckboxGroup', BFormCheckboxGroup)
    app.component('BFormSelect', BFormSelect)
    app.component('BLink', BLink)
    app.component('BModal', BModal)

    // Icons
    app.component('BIconExclamationCircleFill', BIconExclamationCircleFill)
    app.component('BIconCheckCircleFill', BIconCheckCircleFill)
    app.component('BIconChevronDown', BIconChevronDown)
    app.component('BIconChevronRight', BIconChevronRight)
    app.component('BIconFilePlus', BIconFilePlus)
    app.component('BIconFileMinus', BIconFileMinus)
  }
}

export default UIComponents

// Named exports for direct imports
export {
  BButton,
  BAlert,
  BSpinner,
  BCard,
  BRow,
  BCol,
  BCollapse,
  BContainer,
  BFormGroup,
  BFormCheckbox,
  BFormCheckboxGroup,
  BFormSelect,
  BLink,
  BModal,
  BIconExclamationCircleFill,
  BIconCheckCircleFill,
  BIconChevronDown,
  BIconChevronRight,
  BIconFilePlus,
  BIconFileMinus
}
