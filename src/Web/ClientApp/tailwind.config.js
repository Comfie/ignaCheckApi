/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}",
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: 'var(--color-primary-50)',
          100: 'var(--color-primary-100)',
          200: 'var(--color-primary-200)',
          300: 'var(--color-primary-300)',
          400: 'var(--color-primary-400)',
          500: 'var(--color-primary-500)',
          600: 'var(--color-primary-600)',
          700: 'var(--color-primary-700)',
          800: 'var(--color-primary-800)',
          900: 'var(--color-primary-900)',
          950: 'var(--color-primary-950)',
        },
        sidebar: {
          bg: 'var(--color-sidebar-bg)',
          hover: 'var(--color-sidebar-hover)',
          active: 'var(--color-sidebar-active)',
          text: 'var(--color-sidebar-text)',
          'text-hover': 'var(--color-sidebar-text-hover)',
          accent: 'var(--color-sidebar-accent)',
          border: 'var(--color-sidebar-border)',
        },
        dark: {
          50: 'var(--color-dark-50)',
          100: 'var(--color-dark-100)',
          200: 'var(--color-dark-200)',
          300: 'var(--color-dark-300)',
          400: 'var(--color-dark-400)',
          500: 'var(--color-dark-500)',
          600: 'var(--color-dark-600)',
          700: 'var(--color-dark-700)',
          800: 'var(--color-dark-800)',
          900: 'var(--color-dark-900)',
          950: 'var(--color-dark-950)',
        },
      },
      boxShadow: {
        'sidebar': 'var(--shadow-sidebar)',
        'glow': 'var(--shadow-glow)',
      },
    },
  },
  plugins: [],
  // Prevent Tailwind from conflicting with Bootstrap
  corePlugins: {
    preflight: false,
  },
}
