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
    apiBase: process.env.API_BASE || 'http://localhost:5000/api/',
    public: {
      apiBase: process.env.BROWSER_API_BASE || 'http://localhost:5000/api/'
    }
  },
  vue: {
    compilerOptions: {
      isCustomElement: tag => ['obf'].includes(tag)
    }
  },
  nitro: {
    preset: 'bun',
    serveStatic: true,
    routeRules: {
      "/img/**": { headers: { 'cache-control': `public,max-age=3600` } },
      "/_nuxt/**": { headers: { 'cache-control': `public,max-age=3600` } },
    },
  },
  icon: {
    clientBundle: {
      scan: {
        globInclude: ['components/**/*.vue', 'pages/**/*.vue', 'layouts/**/*.vue'],
        globExclude: ['node_modules', '.nuxt', '.output'],
      },
    },
  },
})
