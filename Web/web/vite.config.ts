import { defineConfig } from 'vite'
import path from 'path'
import tailwindcss from '@tailwindcss/vite'
import react from '@vitejs/plugin-react'
import fs from 'fs'

const certKeyPath = './.vite/cert/localhost+2-key.pem'
const certPath = './.vite/cert/localhost+2.pem'
const https =
  fs.existsSync(certKeyPath) && fs.existsSync(certPath)
    ? {
        key: fs.readFileSync(certKeyPath),
        cert: fs.readFileSync(certPath),
      }
    : undefined

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
    https,
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
  },
})
