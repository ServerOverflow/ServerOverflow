<!--suppress HtmlUnknownTarget -->
<template>
  <div class="flex flex-col h-screen">
    <Navbar />
    <div class="flex justify-center items-center flex-grow hero relative">
      <iframe src="/swagger/index.html" class="h-full w-full"></iframe>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: false
})
</script>