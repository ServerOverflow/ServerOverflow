<template>
  <dialog ref="dialog" class="modal modal-bottom sm:modal-middle">
    <div class="modal-box flex flex-col">
      <form method="dialog">
        <button class="btn btn-sm btn-circle btn-ghost absolute right-5 top-5">
          <Icon name="fa6-solid:xmark" size="2em"></Icon>
        </button>
      </form>
      <h3 class="text-lg font-bold">Delete an account</h3>
      <div class="divider divider-start m-0 mb-2"/>
      <div class="overflow-y-auto flex-1 pr-4 -mr-4">
        <p class="mb-2">Are you sure you want to delete this account?</p>
        <AccountForm :account="account" disabled/>
        <div class="flex flex-row gap-2 w-full">
          <button class="btn btn-secondary btn-soft flex-1" @click="submit">Yes, delete it</button>
          <button class="btn btn-primary flex-1" @click="close">No, keep it</button>
        </div>
      </div>
    </div>
    <form method="dialog" class="modal-backdrop">
      <button>close</button>
    </form>
  </dialog>
</template>

<script setup>
const props = defineProps({
  update: Function
});

const { $axios } = useNuxtApp();
const toast = useToast();

const dialog = useTemplateRef('dialog');
const account = ref({});

async function submit() {
  try {
    await $axios.delete(`/user/${account.value.id}`);
    toast.add({
      title: `Successfully deleted ${account.value.username}'s account`,
      color: 'success'
    });
    close();
    const result = props.update();
    if (result instanceof Promise)
      await result;
  } catch (error) {
    handleAxiosError(error, toast);
  }
}

function close() {
  if (dialog.value) dialog.value.close();
}

function open(value) {
  if (!hasPermission('ModifyAccounts')) {
    toast.add({
      title: 'Modify Accounts permission is required',
      color: 'error'
    });
    return;
  }

  if (dialog.value) dialog.value.showModal();
  account.value = Object.assign({}, value);
}

defineExpose({ open, close });
</script>