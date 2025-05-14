<template>
  <FormKit type="form" :actions="false" @submit="submit" v-model="model" :disabled="disabled">
    <FormKit type="text" name="badgeText" label="Badge text" validation="required">
      <template #prefixIcon><Icon name="fa6-solid:id-badge" class="icon-sm mr-2"/></template>
    </FormKit>
    <FormKit type="text" name="code" label="Invitation code" help="Do not specify to automatically generate one">
      <template #prefixIcon><Icon name="fa6-solid:key" class="icon-sm mr-2"/></template>
    </FormKit>
    <FormKit type="checkbox" label="Used" name="used"/>
    <FormKit type="submit" :label="submitLabel || 'Submit'" class="w-full" v-if="!disabled"/>
  </FormKit>
</template>

<script setup>
const props = defineProps({
  submitLabel: String,
  disabled: Boolean,
  submit: Function,
  invitation: {}
})

const model = ref(props.invitation);

// HACK: you can't use v-model with props
watch(() => props.invitation, () => model.value = props.invitation);
</script>