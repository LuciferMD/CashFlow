import { defineConfig } from 'vite'
import path from 'path'
import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import fs from 'fs'
export default defineConfig({
  plugins: [
    react(),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  server: {
    https: {
      key: fs.readFileSync('./.vite/cert/localhost+2-key.pem'),
      cert: fs.readFileSync('./.vite/cert/localhost+2.pem'),
    },
    proxy: {
      '/graphql': {
        target: process.env.GATEWAY_PROXY_TARGET ?? 'http://localhost:5095',
        changeOrigin: true,
      },
      '/auth': {
        target: process.env.AUTH_PROXY_TARGET ?? 'http://localhost:5046',
        changeOrigin: true,
      },
      '/hubs': {
        target: process.env.NOTIFICATION_PROXY_TARGET ?? 'http://localhost:6000',
        changeOrigin: true,
        ws: true,
      },
    },
  }
})
