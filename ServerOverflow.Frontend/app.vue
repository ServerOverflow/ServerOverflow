<template>
  <UApp>
    <NuxtLayout>
      <NuxtPage />
    </NuxtLayout>
  </UApp>
</template>

<style>
div {
  font-family: Comfortaa, sans-serif;
}
</style>

<script setup>
const headers = useRequestHeaders(['cookie'])
const user = useState('user', () => null)
const config = useRuntimeConfig()

if (import.meta.server) {
  const { data } = await useFetch(`${config.public.apiBase}user/me`, { headers, credentials: 'include' });
  user.value = data.value
}

function randInt(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

onMounted(() => {
  setInterval(function () {
    const elements = document.querySelectorAll("obf");
    elements.forEach(function(el) {
      const len = el.textContent.length;
      let output = "";
      for (let i = 0; i < len; i++)
        output += String.fromCharCode(randInt(32, 125));
      el.textContent = output;
    });
  }, 50);
})
</script>