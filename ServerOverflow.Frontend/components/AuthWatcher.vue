<template>
  <!-- Nothing to render -->
</template>

<script setup>
const user = useState('user', () => null);
const router = useRouter();

if (user.value === null)
  throw createError({ statusCode: 403 });

watch(() => user.value, (newValue) => {
  if (newValue === null) router.push('/');
})
</script>