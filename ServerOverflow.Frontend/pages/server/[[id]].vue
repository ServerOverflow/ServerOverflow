<template>
  <div v-if="error" class="flex flex-col gap-2 justify-center items-center flex-grow hero relative">
    <NuxtImg src="/img/wip.png" class="w-100"/>
    <h1 class="text-3xl">Server not found</h1>
    <p class="mt-5 text-center">
      No server with this ID exists.<br>
      Make sure you entered the right one.
    </p>
    <button class="btn btn-lg btn-outline mt-5" @click="router.back()">
      <a class="pr-1">Go back</a>
      <Icon name="fa6-solid:angle-right"/>
    </button>
  </div>
  <div v-if="!server && !error" v-for="index in 50">
    <div class="skeleton h-40 w-full"></div>
  </div>
  <Server v-if="server" :server="server"/>
  <div v-if="server" class="bg-base-300/40 shadow-xl p-4 mt-5">
    <pre class="whitespace-pre-wrap">{{ JSON.stringify(server, null, 2) }}</pre>
  </div>
</template>

<script setup>
const router = useRouter();
const route = useRoute();

const { data: server, error } = await useAuthFetch(`/server/${route.params.id}`, { lazy: true })

definePageMeta({
  layout: 'basic'
})
</script>