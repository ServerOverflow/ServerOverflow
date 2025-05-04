<template>
  <ul>
    <li v-for="profile in profiles">
      <a href="'https://namemc.com/profile/{{ profile.uuid }}">{{ profile.username }}</a>
    </li>
  </ul>
</template>

<script setup>
const config = useRuntimeConfig()
const { data } = await useFetch(`${config.public.apiBase}profile/list`)
const profiles = data.value || [];
</script>