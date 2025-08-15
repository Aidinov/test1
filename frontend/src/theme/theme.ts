import { createTheme } from '@mui/material/styles';
import { red, blue, green, amber } from '@mui/material/colors';

const prefersDark = typeof window !== 'undefined' && window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;

export const theme = createTheme({
  palette: {
    mode: prefersDark ? 'dark' : 'light',
    primary: { main: blue[700] },
    secondary: { main: amber[700] },
    error: { main: red[700] },
    warning: { main: amber[500] },
    success: { main: green[700] },
    info: { main: blue[300] }
  },
  typography: {
    fontFamily: 'Roboto, Helvetica, Arial, sans-serif',
    h1: { fontSize: '2rem' },
    h2: { fontSize: '1.5rem' },
    h3: { fontSize: '1.25rem' },
    body1: { fontSize: '1rem' }
  },
  shape: {
    borderRadius: 8
  },
  components: {
    MuiButton: {
      defaultProps: { variant: 'contained' }
    },
    MuiTextField: {
      defaultProps: { size: 'small', variant: 'outlined' }
    },
    MuiSelect: {
      defaultProps: { size: 'small' }
    },
    MuiTabs: {
      styleOverrides: { root: { minHeight: 40 } }
    },
    MuiChip: {
      defaultProps: { size: 'small' }
    },
    MuiBadge: {
      styleOverrides: { badge: { fontSize: '0.75rem' } }
    }
  }
});

export default theme;
