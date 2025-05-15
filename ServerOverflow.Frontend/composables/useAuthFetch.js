import { defu } from 'defu'

export default function (url, options = {}) {
  const headers = useRequestHeaders(['cookie'])
  const config = useRuntimeConfig()
  const baseURL = import.meta.server ? config.apiBase : config.public.apiBase;
  return useFetch(url, defu(options, {
    credentials: 'include',
    headers, baseURL
  }))
}
