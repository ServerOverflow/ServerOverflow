<template>
  <div class="flex flex-col h-screen">
    <Navbar />
    <div class="flex flex-col gap-2 justify-center items-center flex-grow hero relative">
      <NuxtImg src="/img/wip.png" class="w-100"/>
      <h1 class="text-3xl" v-if="error?.statusCode == 404">Page not found</h1>
      <h1 class="text-3xl" v-else-if="error?.statusCode == 403">Access denied</h1>
      <h1 class="text-3xl" v-else-if="error?.statusCode == 500">Internal server error</h1>
      <h1 class="text-3xl" v-else>Unknown error</h1>
      <p class="mt-5 text-center" v-if="error?.statusCode == 404">
        This page has probably never existed.<br>
        Nobody here but us chickens!
      </p>
      <p class="mt-5 text-center" v-else-if="error?.statusCode == 403">
        You are not authorized to access this page.<br>
        Please login and then refresh this page.
      </p>
      <p class="mt-5 text-center" v-else-if="error?.statusCode == 500">
        [Something] [something] the backend died.<br>
        Try refreshing the page and see if it works.
      </p>
      <p class="mt-5 text-center" v-else>
        Unfortunately, an unknown error has occurred.<br>
        Now you're stuck in a limbo for eternity.
      </p>
      <button class="btn btn-lg btn-outline mt-5" @click="$router.back()">
        <a class="pr-1">Go back</a>
        <Icon name="fa6-solid:angle-right"/>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { NuxtError } from '#app'
const { $router } = useNuxtApp();

defineProps({
  error: Object as () => NuxtError
})
</script>