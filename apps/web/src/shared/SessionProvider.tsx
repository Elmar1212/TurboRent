import { useCallback, useEffect, useMemo, useState } from 'react'
import type { ReactNode } from 'react'
import { send } from './api'
import { SessionContext } from './session'
import type { Session, SessionState } from './session'

/** Loads the signed-in user once and shares it with every page. */
export function SessionProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<Session | null>(null)
  const [status, setStatus] = useState<SessionState['status']>('loading')

  const load = useCallback(async (signal?: AbortSignal) => {
    try {
      const response = await fetch('/api/v1/auth/session', { credentials: 'same-origin', cache: 'no-store', signal })
      if (response.ok) { setSession(await response.json()); setStatus('ready') }
      // 404 means account features are disabled on this server.
      else if (response.status === 401 || response.status === 404) { setSession(null); setStatus('ready') }
      else setStatus('error')
    } catch {
      if (!signal?.aborted) setStatus('error')
    }
  }, [])

  useEffect(() => {
    const controller = new AbortController()
    void load(controller.signal)
    return () => controller.abort()
  }, [load])

  const signOut = useCallback(async () => {
    await send('POST', '/api/v1/auth/logout', {})
    setSession(null)
    setStatus('ready')
  }, [])

  const value = useMemo<SessionState>(() => ({ session, status, refresh: () => load(), signOut }),
    [session, status, load, signOut])
  return <SessionContext.Provider value={value}>{children}</SessionContext.Provider>
}
