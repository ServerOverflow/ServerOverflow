<template>
  <dialog ref="dialog" class="modal modal-bottom sm:modal-middle">
    <div class="modal-box flex flex-col">
      <form method="dialog">
        <button class="btn btn-sm btn-circle btn-ghost absolute right-5 top-5">
          <Icon name="fa6-solid:xmark" size="2em"></Icon>
        </button>
      </form>
      <h3 class="text-lg font-bold">Register a new account</h3>
      <div class="divider divider-start m-0 mb-2"/>

      <FormKit type="form" submit-label="Register" :actions="false" @submit="submit">
        <FormKit type="text" name="username" label="Username" validation="required">
          <template #prefixIcon><Icon name="fa6-solid:user" class="mr-2"/></template>
        </FormKit>
        <FormKit type="password" name="password" label="Password" validation="required|length:6">
          <template #prefixIcon><Icon name="fa6-solid:key" class="mr-2"/></template>
        </FormKit>
        <FormKit type="password" name="inviteCode" label="Invitation code" help="A member has to invite you manually for you to register" validation="required">
          <template #prefixIcon><Icon name="fa6-solid:ticket" class="mr-2"/></template>
        </FormKit>
        <FormKit type="submit" label="Register" class="w-full" />
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
    const res = await $axios.post('/user/register', data);
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