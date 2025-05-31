<template>
  <ul>
    <li v-if="status === 'pending'">
      Loading...
    </li>
    <li v-else-if="profiles" v-for="profile in profiles">
      <a :href="'https://namemc.com/profile/'+profile.uuid">{{ profile.username }}</a>
    </li>
    <li v-else>
      Failed to load the list of profiles, please bug TheAirBlow
    </li>
  </ul>
</template>

<script setup>
const { data: profiles, status } = await useAuthFetch(`/profile/list`, { lazy: true })
</script>