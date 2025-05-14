<template>
  <div class="flex flex-col h-screen">
    <Navbar />
    <div class="flex-grow flex justify-center items-center relative">
      <iframe
          src="https://stats.airblo.ws/public-dashboards/bb99e59c801d4e779fd9d2916883273d"
          class="w-full h-full border-0"
      ></iframe>
    </div>
  </div>
</template>

<script setup>
definePageMeta({
  layout: false
})
</script>