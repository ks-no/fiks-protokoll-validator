/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{vue,js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        // Validation states
        valid: {
          DEFAULT: '#22c55e',
          light: '#dcfce7',
          dark: '#166534'
        },
        invalid: {
          DEFAULT: '#ef4444',
          light: '#fee2e2',
          dark: '#991b1b'
        },
        notValidated: {
          DEFAULT: '#eab308',
          light: '#fef9c3',
          dark: '#854d0e'
        },
        // Brand colors
        primary: {
          50: '#eff6ff',
          100: '#dbeafe',
          200: '#bfdbfe',
          300: '#93c5fd',
          400: '#60a5fa',
          500: '#3b82f6',
          600: '#2563eb',
          700: '#1d4ed8',
          800: '#1e40af',
          900: '#1e3a8a'
        }
      },
      fontFamily: {
        sans: ['Avenir', 'Helvetica', 'Arial', 'sans-serif']
      }
    },
  },
  plugins: [],
}
