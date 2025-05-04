<template>
  <dialog ref="dialog" class="modal modal-bottom sm:modal-middle">
    <div class="modal-box">
      <form method="dialog">
        <button class="btn btn-sm btn-circle btn-ghost absolute right-5 top-5">
          <Icon name="fa6-solid:xmark" size="2em"></Icon>
        </button>
      </form>
      <h3 class="text-lg font-bold">Log into your account</h3>
      <div class="divider divider-start m-0 mb-2"/>

      <FormKit type="form" submit-label="Login" :actions="false" @submit="submit">
        <FormKit type="text" name="username" label="Username" validation="required">
          <template #prefixIcon><Icon name="fa6-solid:user" class="mr-2"/></template>
        </FormKit>
        <FormKit type="password" name="password" label="Password" validation="required">
          <template #prefixIcon><Icon name="fa6-solid:key" class="mr-2"/></template>
        </FormKit>
        <FormKit type="submit" label="Login" class="w-full" />
      </FormKit>
    </div>
    <form method="dialog" class="modal-backdrop">
      <button>close</button>
    </form>
  </dialog>
</template>

<script setup>
const user = useState('user', () => null);
const { $axios } = useNuxtApp();
const toast = useToast();

const dialog = useTemplateRef('dialog');

async function submit(data, node) {
  try {
    const res = await $axios.post('/user/login', data);
    user.value = res.data;
    toast.add({
      title: `Successfully logged in as ${res.data.username}`,
      color: 'success'
    });
    close();
  } catch (error) {
    handleAxiosError(error, node);
  }
}

function close() {
  if (dialog.value) dialog.value.close();
}

function open() {
  if (dialog.value) dialog.value.showModal();
}

defineExpose({ open, close });
</script>