import axios from 'axios'

export default defineNuxtPlugin((nuxtApp) => {
    const instance = axios.create({
        baseURL: nuxtApp.$config.public.apiBase,
        withCredentials: true
    })

    nuxtApp.provide('axios', instance)
})