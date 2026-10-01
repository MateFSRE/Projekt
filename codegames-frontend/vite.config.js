import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// Proxy drži frontend i API na istom originu (localhost:5173) zbog httpOnly cookieja.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5080',
        changeOrigin: true,
      },
    },
  },
})
