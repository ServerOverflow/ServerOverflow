<template>
  <div v-if="error" class="flex flex-col gap-2 justify-center items-center flex-grow hero relative">
    <NuxtImg src="/img/wip.png" class="w-100"/>
    <h1 class="text-3xl">Account not found</h1>
    <p class="mt-5 text-center">
      No account with this ID exists.<br>
      Make sure you entered the right one.
    </p>
    <button class="btn btn-lg btn-outline mt-5" @click="router.back()">
      <a class="pr-1">Go back</a>
      <Icon name="fa6-solid:angle-right"/>
    </button>
  </div>
  <div v-if="!account && !error" v-for="index in 50">
    <div class="skeleton h-40 w-full"></div>
  </div>
  <Account v-if="account" :account="account"/>
  <div v-if="account" class="grid grid-cols-1 xl:grid-cols-2 gap-5">
    <div class="bg-base-300/40 shadow-xl p-4 mt-5">
      <h2 class="text-xl font-bold">Account info</h2>
      <div class="divider my-2"></div>
      <AccountForm :submit="submit" submit-label="Save changes" :account="account"/>
    </div>
    <div class="bg-base-300/40 shadow-xl p-4 mt-5">
      <h2 class="text-xl font-bold">API keys</h2>
      <div class="divider my-2"></div>
      <div v-if="user.id !== route.params.id && route.params.id !== 'me'">
        You can only view your own API keys!
      </div>
      <ManageApiKeys v-else/>
    </div>
  </div>
</template>

<script setup>
const user = useState('user', () => null);
const { $axios } = useNuxtApp();
const router = useRouter();
const route = useRoute();
const toast = useToast();

const { data: account, error } = await useAuthFetch(`/user/${route.params.id}`, { lazy: true })

async function submit(data, node) {
  try {
    await $axios.post(`/user/${route.params.id}`, data);
    toast.add({
      title: `Successfully modified ${account.value.username}'s account`,
      color: 'success'
    });
  } catch (error) {
    handleAxiosError(error, node);
  }
}

definePageMeta({
  layout: 'basic'
})
</script>