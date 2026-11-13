import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import { LocaleProvider } from './shared/i18n/LocaleProvider'
import { SessionProvider } from './shared/SessionProvider'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <LocaleProvider>
      <SessionProvider>
        <App />
      </SessionProvider>
    </LocaleProvider>
  </StrictMode>,
)
