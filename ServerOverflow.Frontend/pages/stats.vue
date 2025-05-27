<template>
  <div class="flex flex-col h-screen">
    <Navbar />
    <div class="flex-grow flex justify-center items-center relative">
      <iframe
          src="https://stats.airblo.ws/public-dashboards/b190b03991a041d48a80e326fcc7d6c2"
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
