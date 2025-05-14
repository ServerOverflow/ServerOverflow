<template>
  <span ref="scrollTarget"></span>
  <h2 class="text-2xl font-bold">Audit logs</h2>
  <p class="text mt-1">Logs of every action performed on ServerOverflow</p>
  <div class="divider my-2"></div>
  <div class="join w-full">
    <label class="join-item input !outline-none mb-2 w-full">
      <Icon name="ci:search-magnifying-glass" size="1.5em" class="opacity-50"/>
      <input type="search" class="grow" placeholder="Enter custom query" v-model="query" />
    </label>
    <button class="join-item btn btn-primary" :class="{ 'btn-disabled': fetching }" @click="update">
      Search
      <span v-if="fetching" class="loading loading-spinner loading-sm"></span>
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
  <div v-if="!logs">
    <div v-if="error" class="alert alert-error alert-soft">
      <span>Failed to fetch log entries from the backend</span>
    </div>
    <div v-if="!error" class="alert alert-error alert-soft">
      <span>No results were found for your query</span>
    </div>
  </div>
  <div v-else>
    <Pagination :data="logs" :open-page="openPage"/>
    <div v-for="entry in logs.items" class="border-b [&:nth-last-child(2)]:border-b-0 border-[color-mix(in_oklch,var(--color-base-content)_5%,#0000)]">
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
    <!--<table class="table">
      <tbody>
      <tr v-for="entry in logs.items">
        <td class="w-1">
          <Icon :name="iconMap[entry.action]" class="icon-md"/>
        </td>
        <td class="w-max">
          {{ entry.description }}
        </td>
        <th class="text-right">
          <div class="join">
            <button class="join-item btn btn-sm btn-primary btn-outline" @click="notImplemented">
              <Icon name="fa6-solid:list" class="icon-xs"/>
            </button>
          </div>
        </th>
      </tr>
      </tbody>
    </table>-->
    <Pagination :data="logs" :open-page="openPage" :scroll-to="scrollTarget" class="mt-2"/>
  </div>
  <QueryDocs ref="queryDocs"/>
</template>

<script setup>
const { $axios } = useNuxtApp();
const router = useRouter();
const route = useRoute();
const toast = useToast();

const scrollTarget = useTemplateRef('scrollTarget');
const queryDocs = useTemplateRef('queryDocs');
const startId = ref(route.query.start);
const lastQuery = ref(route.query.query);
const query = ref(route.query.query);
const fetching = ref(false);

const start = performance.now();
const { data: logs, error: error } = await useAuthFetch(`/audit/search`, {
  method: 'POST',
  query: {
    page: route.query.page || '1',
    startId: startId.value,
    query: query.value
  }
})

const latency = ref(performance.now() - start);
const formattedLatency = computed(() => {
  const ms = latency.value;
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

async function updateRoute(page) {
  await router.push({
    path: route.path,
    query: {
      page: page || route.query.page,
      query: query.value === '' ? null : query.value,
      startId: startId.value
    }
  });
}

async function update() {
  try {
    fetching.value = true;
    let page = route.query.page || '1';
    if (lastQuery.value !== query.value) {
      lastQuery.value = query.value;
      startId.value = null;
      page = '1';
    }

    const start = performance.now();
    const response = await $axios.post('/audit/search', null, {
      params: {
        page: page,
        startId: startId.value,
        query: query.value
      }
    });

    latency.value = performance.now() - start;
    if (!startId.value)
      startId.value = response.data.items[0].id;
    logs.value = response.data;
    fetching.value = false;
    error.value = null;
    await updateRoute(response.data.currentPage);
  } catch (err) {
    fetching.value = false;
    if (err.response) error.value = err;
    handleAxiosError(err, toast);
  }
}

async function openPage(page) {
  await updateRoute(page);
  await update();
}
</script>