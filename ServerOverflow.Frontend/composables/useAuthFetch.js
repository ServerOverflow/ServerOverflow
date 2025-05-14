import { defu } from 'defu'

export default function (endpoint, options = {}) {
  const headers = useRequestHeaders(['cookie'])
  const config = useRuntimeConfig()

  const base = config.public.apiBase.replace(/\/+$/, '')
  const path = typeof endpoint === 'string'
    ? endpoint.replace(/^\/+/, '')
    : typeof endpoint === 'function'
      ? () => String(endpoint()).replace(/^\/+/, '')
      : String(endpoint).replace(/^\/+/, '')

  const url = typeof path === 'function'
    ? () => `${base}/${path()}`
    : `${base}/${path}`

  return useFetch(url, defu(options, {
    headers,
    credentials: 'include'
  }))
}