<template>
  <div class="flex flex-col gap-2 justify-center items-center flex-grow hero relative">
    <NuxtImg src="/img/wip.png" class="w-100"/>
    <h1 class="text-3xl">Page not found</h1>
    <p class="mt-5 text-center">
      This page has probably never existed.<br>
      Nobody here but us chickens!
    </p>
    <button class="btn btn-lg btn-outline mt-5" @click="$router.back()">
      <a class="pr-1">Go back</a>
      <Icon name="fa6-solid:angle-right"/>
    </button>
  </div>
</template>

<script setup>
const { $router } = useNuxtApp();
</script>