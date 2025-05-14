<template>
  <ul>
    <li v-for="profile in profiles">
      <a :href="'https://namemc.com/profile/'+profile.uuid">{{ profile.username }}</a>
    </li>
  </ul>
</template>

<script setup>
const { data } = await useAuthFetch(`/profile/list`)
const profiles = data.value || [];
</script>