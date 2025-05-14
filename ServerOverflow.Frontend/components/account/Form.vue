<template>
  <FormKit type="form" :actions="false" @submit="submit" v-model="model" :disabled="disabled">
    <FormKit type="text" name="username" label="Badge text" validation="required">
      <template #prefixIcon><Icon name="fa6-solid:user" class="icon-sm mr-2"/></template>
    </FormKit>
    <FormKit type="text" name="badgeText" label="Badge text" validation="required">
      <template #prefixIcon><Icon name="fa6-solid:id-badge" class="icon-sm mr-2"/></template>
    </FormKit>
    <FormKit type="password" name="newPassword" label="New password" help="Do not specify to keep password unchanged" v-if="!disabled">
      <template #prefixIcon><Icon name="fa6-solid:key" class="icon-sm mr-2"/></template>
    </FormKit>
    <FormKit
        type="checkbox"
        label="Permissions"
        name="permissions"
        :options="{
          'Administrator': 'Administrator',
          'SearchServers': 'Search servers',
          'SearchPlayers': 'Search players',
          'SearchAccounts': 'Search accounts',
          'ModifyAccounts': 'Modify accounts',
          'ModifyExclusions': 'Modify exclusions',
        }"
    />
    <FormKit type="submit" :label="submitLabel || 'Submit'" class="w-full" v-if="!disabled"/>
  </FormKit>
</template>

<script setup>
const props = defineProps({
  submitLabel: String,
  disabled: Boolean,
  submit: Function,
  account: {}
})

const model = ref(props.account);

// HACK: you can't use v-model with props
watch(() => props.account, () => model.value = props.account);
</script>