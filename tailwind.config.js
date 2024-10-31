/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Pages/**/*.cshtml',
    './Views/**/*.cshtml',
    './Areas/**/Pages/**/*.cshtml',
    './Areas/**/Views/**/*.cshtml'
  ],
  theme: {
    extend: {},
  },
  plugins: [],
  blocklist: [
    'collapse',
  ]
}

