<template>
  <div class="p-5 xl:p-10">
    <span ref="scrollTarget"></span>
    <div class="join w-full mb-2">
      <label class="join-item input !outline-none w-full">
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
        Found {{ servers?.totalMatches || 0 }} servers ({{ formattedLatency }})
      </span>
      <button class="flex-1 justify-end flex flex-row select-none items-center link link-hover" @click="queryDocs.open()">
        <Icon name="fa6-solid:circle-question" class="icon-xs" />
        <span class="opacity-80 ml-1">How do I use operators?</span>
      </button>
    </div>
    <div v-if="!servers && status !== 'pending'">
      <div v-if="error" class="alert alert-error alert-soft">
        <span>Failed to fetch servers from the backend</span>
      </div>
      <div v-if="!error" class="alert alert-error alert-soft">
        <span>No results were found for your query</span>
      </div>
    </div>
    <div v-else>
      <Pagination :data="paginationData" :open-page="openPage"/>
      <div class="grid grid-cols-1 2xl:grid-cols-2 gap-x-2 gap-y-4 my-4">
        <div
            v-if="!servers && status === 'pending'"
            v-for="index in 50"
        >
          <div class="skeleton h-40 w-full"></div>
        </div>
        <Server
            v-else
            v-for="server in servers.items"
            :server="server"
        />
      </div>
      <Pagination :data="paginationData" :open-page="openPage" :scroll-to="scrollTarget"/>
    </div>
  </div>
  <QueryDocs ref="queryDocs"/>
</template>

<script setup>
const router = useRouter();
const route = useRoute();

const scrollTarget = useTemplateRef('scrollTarget');
const queryDocs = useTemplateRef('queryDocs');
const query = ref(route.query.query);
const params = computed(() => ({
  page: route.query.page || '1',
  query: route.query.query
}))

const { data: servers, error, status } = await useAuthFetch(`/server/search`, {
  method: 'POST', query: params, watch: [params], lazy: true
})

const paginationData = computed(() =>
    servers.value || { currentPage: 1, totalPages: 7635 }
);

const formattedLatency = computed(() => {
  const ms = servers.value?.milliseconds || 0;
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
      page: page === 1 ? undefined : page
    }
  });
}
</script>
