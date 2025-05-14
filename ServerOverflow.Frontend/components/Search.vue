<template>
  <div class="relative w-full">
    <div class="input input-bordered flex flex-wrap items-center gap-2 !outline-none min-h-[3rem] w-full"
         :class="{ 'rounded-none': showSuggestions && filteredOperators.length, 'rounded-t': showSuggestions && filteredOperators.length }">
      <div
          v-for="(token, index) in tokens"
          :key="index"
          class="flex items-center bg-base-200 px-2 py-1 rounded text-sm"
      >
        <Icon
            v-if="operatorMap[token.operator]"
            :name="operatorMap[token.operator].icon"
            class="mr-1 w-4 h-4"
        />
        <span class="mr-1">{{ token.operator }} {{ token.negated ? '!' : '=' }}= </span>

        <input
            v-if="activeTokenIndex === index"
            v-model="tokens[index].value"
            :ref="el => inputRefs[index] = el"
            class="bg-transparent border-none outline-none p-0 m-0 w-auto"
            @keydown.enter.prevent="commitActiveToken"
            @blur="commitActiveToken"
        />
        <span v-else class="text-ellipsis overflow-hidden max-w-[8rem]" @click="setActiveToken(index)">
          {{ token.value }}
        </span>

        <button class="ml-1 text-error" @click="removeToken(index)">×</button>
      </div>

      <input
          v-show="activeTokenIndex === null"
          v-model="rawInput"
          @keydown.enter.prevent="addOperator"
          @focus="showSuggestions = true"
          @blur="showSuggestions = false"
          placeholder="Type operator..."
          class="flex-1 bg-transparent !outline-none border-none min-w-[6rem]"
          ref="mainInput"
      />
    </div>
    <ul
        v-if="showSuggestions && (filteredOperators.length || !rawInput)"
        class="absolute z-10 mt-0 w-full border-1 border-t-0 shadow"
        :class="{ 'rounded': !showSuggestions, 'rounded-b': showSuggestions }"
    >
      <template v-if="rawInput">
        <li
            v-for="op in filteredOperators"
            :key="op.name"
            class="px-3 py-2 bg-base-100 hover:bg-base-200 cursor-pointer flex items-center gap-2 rounded"
            @mousedown="selectOperator(op.name)"
        >
          <Icon :name="op.icon" class="w-4 h-4" />
          <span>{{ op.name }}</span>
          <small class="text-gray-500 ml-auto">{{ op.description }}</small>
        </li>
      </template>
      <li v-else class="px-3 py-2 bg-base-100 hover:bg-base-200 cursor-pointer flex items-center gap-2 rounded">
        <span class="text-sm">To list all operators, type ?</span>
      </li>
    </ul>
  </div>
</template>

<script setup>
const props = defineProps({
  value: String,
  operators: Array
});

const emit = defineEmits(['update:value']);

const mainInput = useTemplateRef('mainInput');
const tokens = ref([]);
const rawInput = ref('');
const activeTokenIndex = ref(null);
const showSuggestions = ref(false);
const inputRefs = ref([])

const operatorMap = computed(() => {
  const map = {};
  for (const op of props.operators)
    map[op.name] = op;
  return map;
});

const filteredOperators = computed(() => {
  const q = rawInput.value.replace('-', '').toLowerCase();
  return Object.values(operatorMap.value).filter(op =>
      op.name.toLowerCase().includes(q) || q === '?'
  );
});

function setActiveToken(index) {
  activeTokenIndex.value = index
  nextTick(() => {
    inputRefs.value[index]?.focus()
  })
}

function parseValue() {
  tokens.value = [];
  const regex = /(-)?([a-zA-Z0-9_]+):(?:"((?:[^"\\]|\\.)*)"|(\S+))/g;
  let match;
  while ((match = regex.exec(props.value || '')) !== null) {
    if (!(match[2] in operatorMap.value)) continue;
    tokens.value.push({
      negated: !!match[1],
      operator: match[2],
      value: match[3]?.replace(/\\"/g, '"') ?? match[4] ?? ''
    });
  }
}

function buildValue() {
  emit('update:value', tokens.value.map(t => {
    const quoted = t.value.includes(' ')
        ? `"${t.value.replace(/"/g, '\\"')}"`
        : t.value;
    return `${t.negated ? '-' : ''}${t.operator}:${quoted}`;
  }).join(' '));
}

function addOperator() {
  const raw = rawInput.value.trim();
  if (!raw) return;

  let negated = false;
  let op = raw;

  if (raw.startsWith('-')) {
    negated = true;
    op = raw.slice(1);
  }

  if (!(op in operatorMap.value)) return;

  tokens.value.push({ operator: op, negated, value: '' });
  rawInput.value = '';
  activeTokenIndex.value = tokens.value.length - 1;
  showSuggestions.value = false;
}

function selectOperator(name) {
  const negated = rawInput.value.startsWith('-');
  tokens.value.push({ operator: name, negated, value: '' });
  rawInput.value = '';
  activeTokenIndex.value = tokens.value.length - 1;
  showSuggestions.value = false;
  setTimeout(() => {
    inputRefs.value[tokens.value.length - 1]?.focus()
  }, 100)
}

function commitActiveToken() {
  console.log('commit')
  activeTokenIndex.value = null;
  buildValue();
}

function removeToken(index) {
  tokens.value.splice(index, 1);
  activeTokenIndex.value = null;
  buildValue();
}

onMounted(() => {
  parseValue();
  if (mainInput.value === document.activeElement)
    showSuggestions.value = true;
});
watch(() => props.value, parseValue);
</script>
