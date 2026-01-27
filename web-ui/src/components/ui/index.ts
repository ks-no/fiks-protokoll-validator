import type { App, Plugin } from 'vue'

// UI components
import BButton from './BButton.vue'
import BAlert from './BAlert.vue'
import BSpinner from './BSpinner.vue'
import BFormCheckbox from './BFormCheckbox.vue'
import BFormCheckboxGroup from './BFormCheckboxGroup.vue'
import BModal from './BModal.vue'

const UIComponents: Plugin = {
  install(app: App) {
    app.component('BButton', BButton)
    app.component('BAlert', BAlert)
    app.component('BSpinner', BSpinner)
    app.component('BFormCheckbox', BFormCheckbox)
    app.component('BFormCheckboxGroup', BFormCheckboxGroup)
    app.component('BModal', BModal)
  }
}

export default UIComponents

// Named exports for direct imports
export {
  BButton,
  BAlert,
  BSpinner,
  BFormCheckbox,
  BFormCheckboxGroup,
  BModal
}
