<template>
  <UApp>
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
  </UApp>
</template>

<style>
div {
  font-family: Comfortaa, sans-serif;
}
</style>

<script setup>
const headers = useRequestHeaders(['cookie'])
const user = useState('user', () => null)
const config = useRuntimeConfig()

if (import.meta.server) {
  const { data } = await useFetch(`${config.public.apiBase}user/me`, { headers })
  user.value = data.value
}
</script>