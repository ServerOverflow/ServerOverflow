import { defu } from 'defu'

export default function (url, options = {}) {
  const headers = useRequestHeaders(['cookie'])
  const config = useRuntimeConfig()
  const base = import.meta.server ? config.apiBase : config.public.apiBase;
  return useFetch(url, defu(options, {
    headers,
    credentials: 'include',
    baseURL: base
  }))
}
