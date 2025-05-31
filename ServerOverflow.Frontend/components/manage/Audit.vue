<template>
  <span ref="scrollTarget"></span>
  <h2 class="text-2xl font-bold">Audit logs</h2>
  <p class="text mt-1">Logs of every action performed on ServerOverflow</p>
  <div class="divider my-2"></div>
  <div class="join w-full">
    <label class="join-item input !outline-none mb-2 w-full">
      <Icon name="fa6-solid:magnifying-glass" size="1.5em" class="opacity-50"/>
      <input type="search" class="grow" placeholder="Enter custom query" v-model="query" />
    </label>
    <button class="join-item btn btn-primary" :class="{ 'btn-disabled': status === 'pending' }" @click="update">
      Search
      <span v-if="status === 'pending'" class="loading loading-spinner loading-sm"></span>
    </button>
  </div>
  <div class="mb-2 flex flex-row text-xs">
    <span>
      Found {{ logs?.totalMatches || 0 }} log entries ({{ formattedLatency }})
    </span>
    <button class="flex-1 justify-end flex flex-row select-none items-center link link-hover" @click="queryDocs.open()">
      <Icon name="fa6-solid:circle-question" class="icon-xs" />
      <span class="opacity-80 ml-1">How do I use operators?</span>
    </button>
  </div>
  <div v-if="!logs && status !== 'pending'">
    <div v-if="error" class="alert alert-error alert-soft">
      <span>Failed to fetch log entries from the backend</span>
    </div>
    <div v-if="!error" class="alert alert-error alert-soft">
      <span>No results were found for your query</span>
    </div>
  </div>
  <div v-else>
    <Pagination :data="paginationData" :open-page="openPage"/>
    <div
        v-if="!logs && status === 'pending'"
        v-for="index in 50"
        class="border-b [&:nth-last-child(2)]:border-b-0 border-[color-mix(in_oklch,var(--color-base-content)_5%,#0000)]"
    >
      <div tabindex="0" class="collapse collapse-arrow">
        <div class="collapse-title font-semibold flex flex-row flex-stretch items-center gap-2 p-0">
          <div class="skeleton h-6 w-6"></div>
          <div class="skeleton h-4 w-80"></div>
        </div>
      </div>
    </div>
    <div
        v-else
        v-for="entry in logs.items"
        class="border-b [&:nth-last-child(2)]:border-b-0 border-[color-mix(in_oklch,var(--color-base-content)_5%,#0000)]"
    >
      <div tabindex="0" class="collapse collapse-arrow">
        <div class="collapse-title font-semibold flex flex-row flex-stretch items-center gap-2 p-0">
          <Icon :name="iconMap[entry.action]" class="icon-md"/>
          <div class="flex-1">
            {{ entry.description }}
          </div>
        </div>
        <div class="collapse-content text-sm">
          <ul class="prose">
            <li class="!my-0">
              <code>action: {{ entry.action }}</code>
            </li>
            <li class="!my-0">
              <code>timestamp: {{ entry.timestamp }}</code>
            </li>
            <li v-for="(value, key) in entry.data" class="!my-0">
              <code>{{ key }}: {{ value }}</code>
            </li>
          </ul>
        </div>
      </div>
    </div>
    <Pagination :data="paginationData" :open-page="openPage" :scroll-to="scrollTarget" class="mt-2"/>
  </div>
  <QueryDocs ref="queryDocs"/>
</template>

<script setup>
const router = useRouter();
const route = useRoute();

const scrollTarget = useTemplateRef('scrollTarget');
const queryDocs = useTemplateRef('queryDocs');
const query = ref(route.query.query);
const startId = ref(route.query.startId);
const params = computed(() => ({
  page: route.query.page || '1',
  query: route.query.query,
  startId: startId.value
}))

const { data: logs, error, status } = await useAuthFetch(`/audit/search`, {
  method: 'POST', query: params, watch: [params], lazy: true
})

const paginationData = computed(() =>
    logs.value || { currentPage: 1, totalPages: 20 }
);

const formattedLatency = computed(() => {
  const ms = logs.value?.milliseconds || 0;
  if (ms < 1000) {
    return `${ms.toFixed(0)} ms`;
  } else if (ms < 60_000) {
    return `${(ms / 1000).toFixed(2)} s`;
  } else if (ms < 3_600_000) {
    return `${(ms / 60000).toFixed(2)} min`;
  } else {
    return `${(ms / 3_600_000).toFixed(2)} h`;
  }
})

const iconMap = {
  'CreatedApiKey': 'fa6-solid:key',
  'UpdatedApiKey': 'fa6-solid:key',
  'DeletedApiKey': 'fa6-solid:trash-can',
  'CreatedInvitation': 'fa6-solid:ticket',
  'UpdatedInvitation': 'fa6-solid:ticket',
  'DeletedInvitation': 'fa6-solid:trash-can',
  'CreatedExclusion': 'fa6-solid:list',
  'UpdatedExclusion': 'fa6-solid:list',
  'DeletedExclusion': 'fa6-solid:trash-can',
  'CreatedProfile': 'fa6-solid:robot',
  'DeletedProfile': 'fa6-solid:trash-can',
  'DeletedAccount': 'fa6-solid:trash-can',
  'UpdatedAccount': 'fa6-solid:user',
  'Registered': 'fa6-solid:plus',
  'LoggedIn': 'fa6-solid:right-to-bracket',
  'SearchedServers': 'fa6-solid:magnifying-glass'
};

async function update() {
  if (status === 'pending') return;
  await router.push({
    path: route.path,
    query: {
      query: query.value === '' ? undefined : query.value
    }
  });
}

async function openPage(page) {
  if (status === 'pending') return;
  await router.push({
    path: route.path,
    query: {
      query: route.query.query,
      page: page === 1 ? undefined : page,
      startId: startId.value
    }
  });
}

watch(() => logs.value, () => {
  if (!startId.value && logs.value)
    startId.value = logs.value.items[0].id;
})
</script>