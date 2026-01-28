import type { App, Plugin } from 'vue'

import UiButton from './UiButton.vue'
import UiAlert from './UiAlert.vue'
import UiSpinner from './UiSpinner.vue'
import UiCheckbox from './UiCheckbox.vue'
import UiCheckboxGroup from './UiCheckboxGroup.vue'
import UiModal from './UiModal.vue'

const UIComponents: Plugin = {
  install(app: App) {
    app.component('UiButton', UiButton)
    app.component('UiAlert', UiAlert)
    app.component('UiSpinner', UiSpinner)
    app.component('UiCheckbox', UiCheckbox)
    app.component('UiCheckboxGroup', UiCheckboxGroup)
    app.component('UiModal', UiModal)
  }
}

export default UIComponents

export {
  UiButton,
  UiAlert,
  UiSpinner,
  UiCheckbox,
  UiCheckboxGroup,
  UiModal
}
