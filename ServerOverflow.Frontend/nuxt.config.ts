import tailwindcss from "@tailwindcss/vite";

export default defineNuxtConfig({
  compatibilityDate: '2024-11-01',
  devtools: { enabled: true },
  vite: {
    plugins: [tailwindcss()],
  },
  modules: [
    '@formkit/nuxt',
    '@nuxt/content',
    '@nuxtjs/seo',
    '@nuxt/image',
    '@nuxt/fonts',
    '@nuxt/icon',
    '@nuxt/ui'
  ],
  css: ["~/assets/app.css"],
  formkit: {
    autoImport: true
  },
  app: {
    head: {
      title: 'ServerOverflow',
      titleTemplate: '%s',
      htmlAttrs: {
        lang: 'en',
      },
      link: [
        { rel: 'icon', type: 'image/x-icon', href: '/favicon.ico' },
      ]
    }
  },
  runtimeConfig: {
    public: {
      apiBase: process.env.API_BASE_URL || 'http://localhost:5000/api/'
    }
  },
  vue: {
    compilerOptions: {
      isCustomElement: tag => ['obf'].includes(tag)
    }
  }
})