<template>
  <dialog ref="dialog" class="modal modal-bottom sm:modal-middle">
    <div class="modal-box flex flex-col">
      <form method="dialog">
        <button class="btn btn-sm btn-circle btn-ghost absolute right-5 top-5">
          <Icon name="fa6-solid:xmark" size="2em"></Icon>
        </button>
      </form>
      <h3 class="text-lg font-bold">Edit an account</h3>
      <div class="divider divider-start m-0 mb-2"/>
      <div class="overflow-y-auto flex-1 pr-4 -mr-4">
        <AccountForm :submit="submit" submit-label="Save changes" :account="account"/>
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

async function submit(data, node) {
  try {
    await $axios.post(`/user/${account.value.id}`, data);
    toast.add({
      title: `Successfully modified ${account.value.username}'s account`,
      color: 'success'
    });
    close();
    const result = props.update();
    if (result instanceof Promise)
      await result;
  } catch (error) {
    handleAxiosError(error, node);
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