import { defineConfig, loadEnv } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '')
  const apiBase = env.VITE_API_URL || 'http://backend:8080'

  return {
    plugins: [react()],
    server: {
      host: true,         // 0.0.0.0
      port: 5173,
      strictPort: true,
      proxy: {
        '/api': {
          target: apiBase,
          changeOrigin: true,
          secure: false
        }
      }
    },
    preview: {
      host: true,
      port: 4173
    },
    test: {
      environment: 'jsdom',
      globals: true
    }
  }
})
