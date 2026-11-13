import { useSyncExternalStore } from 'react'

/** Minimal History API router: no dependency, works with nginx's index.html fallback. */
const listeners = new Set<() => void>()

function subscribe(listener: () => void) {
  listeners.add(listener)
  window.addEventListener('popstate', listener)
  return () => {
    listeners.delete(listener)
    window.removeEventListener('popstate', listener)
  }
}

const snapshot = () => location.pathname + location.search

export function navigate(to: string, options: { replace?: boolean; keepScroll?: boolean } = {}) {
  const hash = to.includes('#') ? to.slice(to.indexOf('#') + 1) : ''
  if (to !== snapshot() + location.hash) {
    if (options.replace) history.replaceState(history.state, '', to)
    else history.pushState(null, '', to)
    listeners.forEach(listener => listener())
  }
  // Wait for the new page to render before scrolling to an anchor.
  if (hash) requestAnimationFrame(() => document.getElementById(hash)?.scrollIntoView({ behavior: 'smooth' }))
  else if (!options.keepScroll) window.scrollTo({ top: 0 })
}

export function useLocation() {
  const href = useSyncExternalStore(subscribe, snapshot, snapshot)
  const url = new URL(href, location.origin)
  return { pathname: url.pathname, search: url.search, params: url.searchParams }
}

/** Returns a page-relative return path for the sign-in page, limited to this site. */
export function safeReturnTo(value: string | null): string | null {
  return value && value.startsWith('/') && !value.startsWith('//') ? value : null
}
