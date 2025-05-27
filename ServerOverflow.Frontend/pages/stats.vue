<template>
  <div class="flex flex-col h-screen">
    <Navbar />
    <div class="flex-grow flex justify-center items-center relative">
      <iframe
          src="https://stats.airblo.ws/public-dashboards/724a7bc0ae0a444faf6cc1fa21443547"
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
