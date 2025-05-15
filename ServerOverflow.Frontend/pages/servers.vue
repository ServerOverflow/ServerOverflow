<template>
  <div class="p-5 xl:p-10">
    <span ref="scrollTarget"></span>
    <div class="join w-full mb-2">
      <label class="join-item input !outline-none w-full">
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
        Found {{ servers?.totalMatches || 0 }} servers ({{ formattedLatency }})
      </span>
      <button class="flex-1 justify-end flex flex-row select-none items-center link link-hover" @click="queryDocs.open()">
        <Icon name="fa6-solid:circle-question" class="icon-xs" />
        <span class="opacity-80 ml-1">How do I use operators?</span>
      </button>
    </div>
    <div v-if="!servers">
      <div v-if="error" class="alert alert-error alert-soft">
        <span>Failed to fetch servers from the backend</span>
      </div>
      <div v-if="!error" class="alert alert-error alert-soft">
        <span>No results were found for your query</span>
      </div>
    </div>
    <div v-else>
      <Pagination :data="servers" :open-page="openPage"/>
      <div class="grid grid-cols-1 2xl:grid-cols-2 gap-x-2 gap-y-4 my-4">
        <div
            v-for="server in servers.items"
            :key="server.id"
            class="card bg-base-300/40 shadow-xl"
        >
          <div class="card-body p-4">
            <div class="flex flex-row items-start gap-4 mb-1">
              <div class="avatar">
                <div class="w-18 lg:w-22 rounded">
                  <img
                      class="w-full h-full object-contain"
                      :src="`${config.public.apiBase}server/${server.id}.png`"
                      alt="Favicon"
                  />
                </div>
              </div>
              <div class="flex-1 min-w-0">
                <h2 class="card-title text-xl link link-primary link-hover">
                  <a @click="notImplemented" href="javascript:void">
                    {{ server.ip }}:{{ server.port }}
                  </a>
                </h2>
                <div class="w-full truncate">
                  <MinecraftText :payload="server.ping.description" class="text-md lg:text-lg"/>
                </div>
              </div>
            </div>
            <div class="flex gap-2 overflow-x-auto whitespace-nowrap -mb-3 pb-3">
              <div class="join">
                <div class="join-item badge badge-neutral text-xs">Protocol</div>
                <div class="join-item badge text-xs" :class="server.ping.version?.protocol ? 'badge-success' : 'badge-warning'">
                  <span v-if="server.ping.version?.protocol">{{ server.ping.version.protocol }}</span>
                  <span v-else><Icon name="fa6-solid:question"/></span>
                </div>
              </div>

              <div class="join">
                <div class="join-item badge badge-neutral text-xs">Version</div>
                <div class="join-item badge text-xs" :class="server.ping.version?.name ? 'badge-success' : 'badge-warning'">
                  <span v-if="server.ping.version?.name">{{ server.ping.version.name }}</span>
                  <span v-else><Icon name="fa6-solid:question"/></span>
                </div>
              </div>

              <div class="join">
                <div class="join-item badge badge-neutral text-xs">Players</div>
                <div class="join-item badge text-xs" :class="(server.ping.players?.online != null && server.ping.players?.max != null) ? 'badge-success' : 'badge-warning'">
                  <span v-if="server.ping.players?.online != null && server.ping.players?.max != null">{{ server.ping.players.online }}/{{ server.ping.players.max }}</span>
                  <span v-else><Icon name="fa6-solid:question"/></span>
                </div>
              </div>

              <div v-if="server.ping.chatReporting" class="badge badge-info text-xs">Chat Reporting</div>
              <div v-if="server.ping.isForge" class="badge badge-info text-xs">Forge</div>
            </div>
          </div>
        </div>
      </div>
      <Pagination :data="servers" :open-page="openPage" :scroll-to="scrollTarget"/>
    </div>
  </div>
  <QueryDocs ref="queryDocs"/>
</template>

<script setup>
const config = useRuntimeConfig();
const { $axios } = useNuxtApp();
const router = useRouter();
const route = useRoute();
const toast = useToast();

const scrollTarget = useTemplateRef('scrollTarget');
const queryDocs = useTemplateRef('queryDocs');
const lastQuery = ref(route.query.query);
const query = ref(route.query.query);
const fetching = ref(false);

const start = performance.now();
const { data: servers, error: error } = await useAuthFetch(`/server/search`, {
  method: 'POST',
  query: {
    page: route.query.page || '1',
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

async function updateRoute(page) {
  await router.push({
    path: route.path,
    query: {
      page: page || route.query.page,
      query: query.value === '' ? null : query.value
    }
  });
}

async function update() {
  try {
    fetching.value = true;
    let page = route.query.page || '1';
    if (lastQuery.value !== query.value) {
      lastQuery.value = query.value;
      page = '1';
    }

    const start = performance.now();
    const response = await $axios.post('/server/search', null, {
      params: {
        page: page,
        query: query.value
      }
    });

    latency.value = performance.now() - start;
    servers.value = response.data;
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
