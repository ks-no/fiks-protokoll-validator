export type ButtonVariant = 'primary' | 'secondary' | 'danger' | 'warning' | 'success' | 'info' | 'light' | 'dark' | 'link' | 'outline-primary' | 'outline-secondary' | 'outline-danger'

export type AlertVariant = 'primary' | 'secondary' | 'success' | 'danger' | 'warning' | 'info' | 'light' | 'dark'

export type ModalSize = 'sm' | 'md' | 'lg' | 'xl'

export type ColSize = 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 'auto'

export interface SelectOption {
  value: string
  text: string
  disabled?: boolean
}
