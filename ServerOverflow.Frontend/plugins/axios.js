import axios from 'axios'

export default defineNuxtPlugin((nuxtApp) => {
    const instance = axios.create({
        baseURL: nuxtApp.$config.public.apiBase,
        withCredentials: true,
        timeout: 5000
    })

    nuxtApp.provide('axios', instance)
})