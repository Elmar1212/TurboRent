import { useLocation } from './shared/router'
import RegisterPage from './features/identity/RegisterPage'
import LoginPage from './features/identity/LoginPage'
import VerifyEmailPage from './features/identity/VerifyEmailPage'
import ForgotPasswordPage from './features/identity/ForgotPasswordPage'
import ResetPasswordPage from './features/identity/ResetPasswordPage'
import HomePage from './features/home/HomePage'
import NotFoundPage from './features/home/NotFoundPage'
import CarCatalogPage from './features/marketplace/CarCatalogPage'
import ListingDetailPage from './features/marketplace/ListingDetailPage'
import OwnerListingPage from './features/marketplace/OwnerListingPage'
import OwnerListingsPage from './features/owner/OwnerListingsPage'
import OwnerBookingsPage from './features/owner/OwnerBookingsPage'
import AccountPage from './features/account/AccountPage'
import AdminListingsPage from './features/admin/AdminListingsPage'
import AdminAccountsPage from './features/admin/AdminAccountsPage'

const guid = '([0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12})'
const listingPath = new RegExp(`^/cars/${guid}/?$`, 'i')
const editPath = new RegExp(`^/owner/listings/${guid}/edit/?$`, 'i')

export default function App() {
  const { pathname } = useLocation()
  const path = pathname.length > 1 ? pathname.replace(/\/$/, '') : pathname
  const listing = path.match(listingPath)
  if (listing) return <ListingDetailPage key={listing[1]} id={listing[1]} />
  const edit = path.match(editPath)
  if (edit) return <OwnerListingPage key={edit[1]} editId={edit[1]} />
  switch (path) {
    case '/': return <HomePage />
    case '/cars': return <CarCatalogPage />
    case '/register': return <RegisterPage />
    case '/login': return <LoginPage />
    case '/verify-email': return <VerifyEmailPage />
    case '/forgot-password': return <ForgotPasswordPage />
    case '/reset-password': return <ResetPasswordPage />
    case '/account': return <AccountPage />
    case '/owner/listings': return <OwnerListingsPage />
    case '/owner/listings/new': return <OwnerListingPage key="new" />
    case '/owner/bookings': return <OwnerBookingsPage />
    case '/admin': return <AdminListingsPage />
    case '/admin/accounts': return <AdminAccountsPage />
    default: return <NotFoundPage />
  }
}
