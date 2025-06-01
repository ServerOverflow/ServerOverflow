<template>
  <FormKit type="form" :actions="false" @submit="submit" v-model="model" :disabled="disabled">
    <FormKit type="text" name="name" label="Name" validation="required">
      <template #prefixIcon><Icon name="fa6-solid:user" class="icon-sm mr-2"/></template>
    </FormKit>
    <FormKit
        v-if="props.showExpire"
        type="radio"
        name="expireIn"
        label="Expire in"
        :options="[
          { label: '1 year', value: '31556952000' },
          { label: '6 months', value: '15552000000' },
          { label: '3 months', value: '7776000000' },
          { label: '1 month', value: '2592000000' }
        ]"
        validation="required"
    />
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
          'HoneypotEvents': 'Honeypot events'
        }"
    />
    <FormKit type="submit" :label="submitLabel || 'Submit'" class="w-full" v-if="!disabled"/>
  </FormKit>
</template>

<script setup>
const props = defineProps({
  showExpire: Boolean,
  submitLabel: String,
  disabled: Boolean,
  submit: Function,
  apiKey: {}
})

const model = ref(props.apiKey);

// HACK: you can't use v-model with props
watch(() => props.apiKey, () => model.value = props.apiKey);
</script>
