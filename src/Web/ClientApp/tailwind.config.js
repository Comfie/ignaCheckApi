/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: "selector",
  content: ["./src/**/*.{html,ts}"],
  theme: {
    extend: {
      colors: {
        // Primary theme colors from template
        primary: {
          DEFAULT: '#006666',
          50: '#e6f5f5',
          100: '#ccebeb',
          200: '#99d6d6',
          300: '#66c2c2',
          400: '#33adad',
          500: '#006666',
          600: '#005252',
          700: '#003d3d',
          800: '#002929',
          900: '#001414',
        },
        secondary: {
          DEFAULT: '#FE6A49',
          50: '#fff4f1',
          100: '#fee9e3',
          200: '#fdd3c7',
          300: '#fcbdab',
          400: '#fb938f',
          500: '#FE6A49',
          600: '#fe4e27',
          700: '#e43310',
          800: '#b1280c',
          900: '#7e1c09',
        },

        // ===================
        // Semantic Color Names
        // ===================

        // Page backgrounds
        'page-bg': {
          light: '#EEF1F6',
          dark: '#1A1C23',
        },

        // Surface colors (cards, panels, dropdowns)
        'surface': {
          light: '#ffffff',
          dark: '#262932',
        },

        // Surface hover states
        'surface-hover': {
          light: '#F5F6F9',
          dark: '#1d1e26',
        },

        // Sidebar colors
        'sidebar': {
          light: '#006666',         // Primary teal
          'light-hover': '#005252', // Darker teal
          dark: '#22242B',          // Dark sidebar
          'dark-hover': '#1d1e26',  // Darker on hover
        },

        // Header/Navbar colors
        'header': {
          light: '#ffffff',
          dark: '#22242B',
        },

        // Text colors
        'text': {
          primary: '#051A1A',           // Light mode headings
          'primary-dark': '#ffffff',    // Dark mode headings
          secondary: '#86909C',         // Light mode body text
          'secondary-dark': 'rgba(255, 255, 255, 0.7)', // Dark mode body text
          'sidebar-light': 'rgba(255, 255, 255, 0.8)',  // Sidebar text on teal
          'sidebar-submenu': 'rgba(255, 255, 255, 0.7)', // Sidebar submenu text
        },

        // Border colors
        'border': {
          light: '#E6E9EB',
          'light-alt': 'rgb(229, 231, 235)', // gray-200
          dark: 'rgba(255, 255, 255, 0.1)',
        },

        // Legacy names (keep for backward compatibility)
        body: {
          DEFAULT: '#86909C',
          dark: 'rgba(255, 255, 255, 0.7)',
        },
        heading: {
          DEFAULT: '#051A1A',
          dark: '#ffffff',
        },
        'light-bg': {
          DEFAULT: '#EEF1F6',
          50: '#F5F6F9',
          100: '#F2F3F7',
          200: '#FCFCFD',
          dark: '#1d1e26',
        },
        'card-bg': {
          DEFAULT: '#ffffff',
          dark: '#262932',
        },
        'chart-bg': {
          DEFAULT: '#FCFCFD',
          dark: '#262932',
        },
        'border-color': {
          DEFAULT: '#E6E9EB',
          dark: '#374558',
        },
        'dark-bg': {
          primary: '#1A1C23',
          secondary: '#262932',
          tertiary: '#1d1e26',
        },
      },
      fontFamily: {
        sans: ['Montserrat', 'sans-serif'],
        body: ['Montserrat', 'sans-serif'],
      },
      fontSize: {
        'xs': '0.75rem',      // 12px
        'sm': '0.875rem',     // 14px
        'base': '0.875rem',   // 14px (body default)
        'lg': '0.9375rem',    // 15px
        'xl': '1rem',         // 16px
        '2xl': '1.25rem',     // 20px
        '3xl': '1.5rem',      // 24px
        '4xl': '1.625rem',    // 26px
        '5xl': '1.75rem',     // 28px
        '6xl': '2rem',        // 32px
      },
      fontWeight: {
        normal: '400',
        medium: '500',
        semibold: '600',
        bold: '700',
      },
      letterSpacing: {
        tight: '0.7px',
        normal: '1px',
      },
      lineHeight: {
        'body': '1.7',
        'heading': '1.4',
        'tight': '1',
      },
    },
  },
  plugins: [],
};

