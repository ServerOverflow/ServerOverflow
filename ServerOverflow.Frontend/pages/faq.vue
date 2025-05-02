<template>
  <article class="prose">
    <ContentRenderer v-if="page" :value="page" />
    <div v-else>Failed to load FAQ page</div>
  </article>
</template>

<script setup lang="ts">
const { data: page } = await useAsyncData(() => queryCollection('content').path('/faq').first())
</script>