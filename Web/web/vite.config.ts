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
    }
  }
})
