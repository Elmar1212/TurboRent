import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { readFileSync } from 'node:fs'
import { fileURLToPath } from 'node:url'

const secure = process.env.TURBORENT_HTTPS === 'true'
const certDirectory = fileURLToPath(new URL('../../artifacts/dev-certs/', import.meta.url))
export default defineConfig({
  plugins: [react()],
  server: {
    host: '127.0.0.1',
    port: secure ? 5174 : 5173,
    strictPort: true,
    https: secure ? { cert: readFileSync(certDirectory + 'turborent.pem'), key: readFileSync(certDirectory + 'turborent.key') } : undefined,
    proxy: { '/api': { target: process.env.TURBORENT_API_URL ?? (secure ? 'https://localhost:7080' : 'http://127.0.0.1:5080'), secure: true } },
  },
})
