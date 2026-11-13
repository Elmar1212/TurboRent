import { useLocale } from '../../shared/i18n/messages'
import { SiteLayout } from '../../shared/layout/SiteLayout'
import { Link } from '../../shared/Link'

export default function NotFoundPage() {
  const { t } = useLocale()
  return <SiteLayout>
    <div className="empty-state not-found">
      <p className="not-found-code" aria-hidden="true">404</p>
      <h1>{t('notFoundTitle')}</h1>
      <p>{t('notFoundText')}</p>
      <div className="button-row center">
        <Link className="btn btn-primary" href="/">{t('backToHome')}</Link>
        <Link className="btn btn-ghost" href="/cars">{t('browseCars')}</Link>
      </div>
    </div>
  </SiteLayout>
}
