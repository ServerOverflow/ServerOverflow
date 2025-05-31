<template>
  <dialog ref="dialog" class="modal modal-bottom sm:modal-middle">
    <div class="modal-box flex flex-col">
      <form method="dialog">
        <button class="btn btn-sm btn-circle btn-ghost absolute right-5 top-5">
          <Icon name="fa6-solid:xmark" size="2em"></Icon>
        </button>
      </form>
      <h3 class="text-lg font-bold">Create a new API key</h3>
      <div class="divider divider-start m-0 mb-2"/>
      <div class="overflow-y-auto flex-1 pr-4 -mr-4">
        <ApiKeyForm :submit="submit" submit-label="Create" :show-expire="true"/>
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

async function submit(data, node) {
  data.expireAt = new Date(Date.now() + parseInt(data.expireIn)).toISOString();
  try {
    const res = await $axios.post('/key/create', data);
    toast.add({
      title: `Successfully created an API key: ${res.data.key}`,
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

function open() {
  if (dialog.value) dialog.value.showModal();
}

defineExpose({ open, close });
</script>