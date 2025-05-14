<template>
  <FormKit type="form" :actions="false" @submit="submit" v-model="model" :disabled="disabled">
    <h2 class="text-zinc-700 text-sm font-bold mb-2 dark:text-zinc-300 formkit-label"
        :class="{ 'select-none opacity-50': disabled }">
      IP range list
    </h2>
    <FormKit
        type="list" name="ranges" :value="['']" dynamic
        #default="{ items, node, value }">
      <FormKit
          v-for="(item, index) in items" :key="item" :index="index" placeholder="127.0.0.1/24"
          :suffix-icon="index !== 0 ? 'trash' : ''"
          :validation="[['required'], ['matches', /^(?:[0-9]{1,3}\.){3}[0-9]{1,3}(?:\/([0-9]|[1-2][0-9]|3[0-2]))?$/]]"
          @suffix-icon-click="() => { if (index !== 0) node.input(value.filter((_, i) => i !== index)) }"
          :sections-schema="{ suffixIcon: { $el: 'button', attrs: { type: 'button' } } }"
          :validation-messages="{
                required: 'IP range cannot be empty.',
                matches: 'Enter a valid IPv4 address or CIDR'
              }"
      />
      <FormKit type="button" @click="() => node.input(value.concat(''))" v-if="!disabled">
        Add another <Icon name="fa6-solid:plus" class="ml-2"/>
      </FormKit>
    </FormKit>
    <FormKit type="textarea" name="comment" label="Comment" validation="required"
             placeholder="Please don't scan my server 188.67.56.48&#10;@theairblow on Telegram, ID 2090827378"/>
    <FormKit type="submit" :label="submitLabel || 'Submit'" class="w-full" v-if="!disabled"/>
  </FormKit>
</template>

<script setup>
const props = defineProps({
  submitLabel: String,
  disabled: Boolean,
  submit: Function,
  exclusion: {}
})

const model = ref(props.exclusion);

// HACK: you can't use v-model with props
watch(() => props.exclusion, () => model.value = props.exclusion);
</script>