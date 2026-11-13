import { useEffect, useState } from 'react'
import { ApiError, submitTo } from '../identity/api'
import { useLocale } from '../../shared/i18n/messages'
import { SiteLayout } from '../../shared/layout/SiteLayout'
import './admin.css'

type PendingAccount = {
  userId: string
  accountType: 'owner' | 'company'
  displayName: string
  email: string
  contactPhone: string
  legalName: string | null
  voen: string | null
  submittedAtUtc: string
}

type AccountPage = { items: PendingAccount[]; total: number; page: number; pageSize: number }
const rejectionReasons = [
  'incomplete_information',
  'invalid_contact_details',
  'invalid_business_information',
  'other',
] as const

async function fetchPendingAccounts(page: number, signal?: AbortSignal): Promise<AccountPage | null> {
  const response = await fetch(`/api/v1/admin/accounts?page=${page}&pageSize=20`, {
    credentials: 'same-origin',
    cache: 'no-store',
    signal,
  })
  if (response.status === 401) {
    location.assign('/login?returnTo=%2Fadmin%2Faccounts')
    return null
  }
  if (!response.ok) throw new Error('adminAccountsLoadError')
  return await response.json() as AccountPage
}

export default function AdminAccountsPage() {
  const language = useLocale()
  const { t } = language
  const [accounts, setAccounts] = useState<PendingAccount[]>([])
  const [total, setTotal] = useState(0)
  const [page, setPage] = useState(1)
  const [pageCount, setPageCount] = useState(1)
  const [authorized, setAuthorized] = useState(false)
  const [checked, setChecked] = useState(false)
  const [busyId, setBusyId] = useState('')
  const [reasonById, setReasonById] = useState<Record<string, string>>({})
  const [notice, setNotice] = useState('')

  function showAccounts(result: AccountPage) {
    setAccounts(result.items)
    setTotal(result.total)
    setPageCount(Math.max(1, Math.ceil(result.total / result.pageSize)))
    setNotice('')
  }

  useEffect(() => {
    const controller = new AbortController()
    fetch('/api/v1/auth/session', {
      credentials: 'same-origin', cache: 'no-store', signal: controller.signal,
    }).then(async response => {
      if (response.status === 401) {
        location.assign('/login?returnTo=%2Fadmin%2Faccounts')
        return
      }
      if (!response.ok) throw new Error('adminAccountsLoadError')
      const session = await response.json()
      if (session.isAdmin !== true) {
        setChecked(true)
        return
      }
      setAuthorized(true)
      setChecked(true)
    }).catch(() => {
      if (!controller.signal.aborted) {
        setNotice('adminAccountsLoadError')
        setChecked(true)
      }
    })
    return () => controller.abort()
  }, [])

  useEffect(() => {
    if (!authorized) return
    const controller = new AbortController()
    fetchPendingAccounts(page, controller.signal).then(result => {
      if (result && !controller.signal.aborted) showAccounts(result)
    }).catch(() => {
      if (!controller.signal.aborted) setNotice('adminAccountsLoadError')
    })
    return () => controller.abort()
  }, [authorized, page])

  async function review(account: PendingAccount, decision: 'approve' | 'reject') {
    setBusyId(account.userId)
    setNotice('')
    try {
      const reasonCode = reasonById[account.userId]
      if (decision === 'reject' && (
        !rejectionReasons.includes(reasonCode as typeof rejectionReasons[number]) ||
        (account.accountType === 'owner' && reasonCode === 'invalid_business_information')
      )) {
        setNotice('invalid_account_rejection_reason')
        return
      }
      await submitTo(`/api/v1/admin/accounts/${account.userId}/${decision}`,
        decision === 'reject' ? { reasonCode } : {})
      const result = await fetchPendingAccounts(page)
      if (!result) return
      showAccounts(result)
      setNotice('adminAccountActionSuccess')
    } catch (error) {
      if (error instanceof ApiError && error.code === 'account_already_reviewed') {
        setNotice('adminAccountReviewConflict')
      } else if (error instanceof ApiError && error.code === 'company_voen_already_approved') {
        setNotice('adminCompanyVoenAlreadyApproved')
      } else {
        setNotice('adminAccountActionError')
      }
    } finally {
      setBusyId('')
    }
  }

  if (!checked) return <SiteLayout><p role="status">{t('loading')}</p></SiteLayout>
  if (!authorized) return <main className="admin-page">
    <h1>{t('adminPanel')}</h1>
    <p role="alert">{t('adminAccessDenied')}</p>
    <a href="/login?returnTo=%2Fadmin%2Faccounts">{t('login')}</a>
  </main>

  return <SiteLayout wide>
    <div className="admin-page">
      <div className="admin-heading">
        <div>
          <p className="card-step">TURBORENT / ADMIN</p>
          <h1>{t('adminAccountsTitle')}</h1>
          <p>{t('adminAccountsIntro')} · {total}</p>
          <nav className="admin-section-nav" aria-label={t('adminPanel')}>
            <a href="/admin">{t('adminListingsLink')}</a>
            <span aria-current="page">{t('adminAccountsLink')}</span>
          </nav>
        </div>
      </div>
      {notice && <p className="form-notice" role="alert">{t(notice)}</p>}
      {accounts.length === 0 && !notice && <section className="admin-empty">{t('adminNoAccounts')}</section>}
      <div className="admin-list">
        {accounts.map(account => {
          const reasons = rejectionReasons.filter(reason =>
            account.accountType === 'company' || reason !== 'invalid_business_information')
          return <article className="admin-listing admin-account" key={account.userId}>
            <div className="admin-listing-content">
              <div className="admin-listing-title">
                <h2>{account.displayName}</h2>
                <span>{t(account.accountType === 'company' ? 'adminAccountCompany' : 'adminAccountOwner')}</span>
              </div>
              <p>{account.email}</p>
              <p>{t('adminAccountPhone')}: {account.contactPhone}</p>
              {account.accountType === 'company' && <>
                <p>{t('adminAccountLegalName')}: {account.legalName}</p>
                <p>{t('adminAccountVoen')}: {account.voen}</p>
              </>}
              <p className="admin-submitted">{t('adminAccountSubmittedAt')}: {new Date(account.submittedAtUtc).toLocaleString(language.locale)}</p>
            </div>
            <div className="admin-review-actions">
              <label>
                <span>{t('adminAccountReason')}</span>
                <select value={reasonById[account.userId] ?? ''} onChange={event =>
                  setReasonById(current => ({ ...current, [account.userId]: event.target.value }))}>
                  <option value="">—</option>
                  {reasons.map(reason => <option value={reason} key={reason}>{t(`adminAccountReason_${reason}`)}</option>)}
                </select>
              </label>
              <div className="admin-review-buttons">
                <button type="button" className="admin-approve" disabled={busyId === account.userId}
                  onClick={() => review(account, 'approve')}>{t('adminAccountApprove')}</button>
                <button type="button" className="admin-reject"
                  disabled={busyId === account.userId || !reasonById[account.userId]}
                  onClick={() => review(account, 'reject')}>{t('adminAccountReject')}</button>
              </div>
            </div>
          </article>
        })}
      </div>
      {pageCount > 1 && <nav className="admin-pagination" aria-label="Pagination">
        <button type="button" disabled={page <= 1} onClick={() => setPage(current => current - 1)}>←</button>
        <span>{page} / {pageCount}</span>
        <button type="button" disabled={page >= pageCount} onClick={() => setPage(current => current + 1)}>→</button>
      </nav>}
    </div>
  </SiteLayout>
}
