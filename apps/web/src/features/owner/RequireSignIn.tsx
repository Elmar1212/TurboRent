import type { ReactNode } from 'react'
import { useLocale } from '../../shared/i18n/messages'
import { Link } from '../../shared/Link'
import { useSession } from '../../shared/session'

/** Renders children for signed-in users and a sign-in prompt for everyone else. */
export function RequireSignIn({ returnTo, children }: { returnTo: string; children: ReactNode }) {
  const { t } = useLocale()
  const { session, status } = useSession()
  if (status === 'loading') return <p role="status">{t('loading')}</p>
  if (!session) return <div className="empty-state">
    <h1>{t('not_authenticated')}</h1>
    <Link className="btn btn-primary" href={`/login?returnTo=${encodeURIComponent(returnTo)}`}>{t('login')}</Link>
  </div>
  return <>{children}</>
}
