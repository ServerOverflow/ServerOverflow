<template>
  <h1 class="text-3xl p-4 pt-0 mb-2 font-bold">
    Frequently asked questions
  </h1>
  <ContentRenderer v-if="page" :value="page" class="join join-vertical bg-base-100 shadow-lg" />
  <div v-else>Failed to load FAQ page</div>
</template>

<script setup>
const { data: page } = await useAsyncData(() => queryCollection('content').path('/faq').first())

definePageMeta({
  layout: 'basic'
})
</script>