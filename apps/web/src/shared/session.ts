import { createContext, useContext } from 'react'

export type Session = {
  displayName: string
  email?: string
  locale?: 'az' | 'en'
  isAdmin?: boolean
  canCreateListing: boolean
  hasOwnerProfile?: boolean
  accountApprovalStatus?: string | null
  accountApprovalReason?: string | null
}

export type SessionState = {
  session: Session | null
  /** loading: first check running; ready: known (signed in or not); error: could not check. */
  status: 'loading' | 'ready' | 'error'
  refresh: () => Promise<void>
  signOut: () => Promise<void>
}

export const SessionContext = createContext<SessionState | null>(null)

export function useSession(): SessionState {
  const context = useContext(SessionContext)
  if (!context) throw new Error('useSession must be used inside SessionProvider.')
  return context
}

/** True for owner/company accounts, whatever their approval state. */
export const isOwnerAccount = (session: Session | null) =>
  !!session && (session.hasOwnerProfile === true || !!session.accountApprovalStatus)
