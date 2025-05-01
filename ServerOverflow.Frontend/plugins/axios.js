import axios from 'axios'

export default defineNuxtPlugin((nuxtApp) => {
    const instance = axios.create({
        //  baseURL: '/api/',
        baseURL: 'http://localhost:5000/',
        withCredentials: true
    })

    nuxtApp.provide('axios', instance)
})