<template>
  <dialog ref="dialog" class="modal modal-bottom sm:modal-middle">
    <div class="modal-box !max-w-max flex flex-col">
      <form method="dialog">
        <button class="btn btn-sm btn-circle btn-ghost absolute right-5 top-5">
          <Icon name="fa6-solid:xmark" size="2em"></Icon>
        </button>
      </form>
      <h3 class="text-lg font-bold">Advanced query language</h3>
      <div class="divider divider-start m-0 mb-2"/>
      <div class="overflow-y-auto flex-1 pr-4 -mr-4 prose">
        <ContentRenderer v-if="page" :value="page" />
        <div v-else>Failed to load the AQL page</div>
      </div>
    </div>
    <form method="dialog" class="modal-backdrop">
      <button>close</button>
    </form>
  </dialog>
</template>

<script setup>
const { data: page } = await useAsyncData(() => queryCollection('content').path('/query').first())
const dialog = useTemplateRef('dialog');

function close() {
  if (dialog.value) dialog.value.close();
}

function open() {
  if (dialog.value) dialog.value.showModal();
}

defineExpose({ open, close });
</script>