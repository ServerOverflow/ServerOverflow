<template>
  <article class="prose prose-a:no-underline max-w-none">
    <ContentRenderer v-if="page" :value="page" />
    <div v-else>Failed to load FAQ page</div>
  </article>
</template>

<script setup lang="ts">
const { data: page } = await useAsyncData(() => queryCollection('content').path('/faq').first())
</script>