<template>
  <ul>
    <li v-if="profiles" v-for="profile in profiles">
      <a :href="'https://namemc.com/profile/'+profile.uuid">{{ profile.username }}</a>
    </li>
    <li v-else>
      Failed to load the list of profiles, please bug TheAirBlow
    </li>
  </ul>
</template>

<script setup>
const { data: profiles } = await useAuthFetch(`/profile/list`, { server: true })
</script>