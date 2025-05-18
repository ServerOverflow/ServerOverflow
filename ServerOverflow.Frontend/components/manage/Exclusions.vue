<template>
  <span ref="scrollTarget"></span>
  <h2 class="text-2xl font-bold">Exclusions</h2>
  <p class="text mt-1">IP range and individual address exclusions</p>
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
      Found {{ exclusions?.totalMatches || 0 }} exclusions ({{ formattedLatency }})
    </span>
    <button class="flex-1 justify-end flex flex-row select-none items-center link link-hover" @click="queryDocs.open()">
      <Icon name="fa6-solid:circle-question" class="icon-xs" />
      <span class="opacity-80 ml-1">How do I use operators?</span>
    </button>
  </div>
  <div v-if="!exclusions">
    <div v-if="error" class="alert alert-error alert-soft">
      <span>Failed to fetch exclusions from the backend</span>
    </div>
    <div v-if="!error" class="alert alert-error alert-soft">
      <span>No results were found for your query</span>
    </div>
    <button v-if="!error" class="btn btn-accent btn-outline mt-2" @click="createDialog.open">
      Create new exclusion
      <Icon name="fa6-solid:plus"/>
    </button>
  </div>
  <div v-else>
    <Pagination :data="exclusions" :open-page="openPage"/>
    <table class="table">
      <thead>
      <tr>
        <th>#</th>
        <th>Ranges</th>
        <th class="hidden sm:table-cell">Comment</th>
        <th class="w-full text-right">
          <button class="btn btn-sm btn-accent btn-outline w-19" @click="createDialog.open">
            New <Icon name="fa6-solid:plus"/>
          </button>
        </th>
      </tr>
      </thead>
      <tbody>
      <tr class="whitespace-nowrap" v-for="(exclusion, index) in exclusions.items">
        <td>
          {{ (exclusions.currentPage - 1) * 50 + index + 1 }}
        </td>
        <td>
          {{ exclusion.ranges.length }} total
        </td>
        <td class="hidden sm:table-cell max-w-70 md:max-w-100 xl:max-w-110 overflow-x-clip whitespace-nowrap text-ellipsis">
          {{ truncate(exclusion.comment) }}
        </td>
        <th class="w-full text-right">
          <div class="join">
            <button class="join-item btn btn-sm btn-primary btn-outline" @click="editDialog.open(exclusion)">
              <Icon name="fa6-solid:pencil" class="icon-xs"/>
            </button>
            <button class="join-item btn btn-sm btn-error btn-outline" @click="deleteDialog.open(exclusion)">
              <Icon name="fa6-solid:trash" class="icon-xs"/>
            </button>
          </div>
        </th>
      </tr>
      </tbody>
    </table>
    <Pagination :data="exclusions" :open-page="openPage" :scroll-to="scrollTarget"/>
  </div>
  <ExclusionCreate ref="createDialog" :update="update"/>
  <ExclusionDelete ref="deleteDialog" :update="update"/>
  <ExclusionEdit ref="editDialog" :update="update"/>
  <QueryDocs ref="queryDocs"/>
</template>

<script setup>
const { $axios } = useNuxtApp();
const router = useRouter();
const route = useRoute();
const toast = useToast();

const scrollTarget = useTemplateRef('scrollTarget');
const createDialog = useTemplateRef('createDialog');
const deleteDialog = useTemplateRef('deleteDialog');
const editDialog = useTemplateRef('editDialog');
const queryDocs = useTemplateRef('queryDocs');
const lastQuery = ref(route.query.query);
const query = ref(route.query.query);
const fetching = ref(false);

const { data: exclusions, error: error } = await useAuthFetch(`/exclusion/search`, {
  method: 'POST',
  query: {
    page: route.query.page || '1',
    query: query.value
  }
})

const formattedLatency = computed(() => {
  const ms = exclusions.value?.milliseconds || 0;
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

    const response = await $axios.post('/exclusion/search', null, {
      params: {
        page: page,
        query: query.value
      }
    });

    exclusions.value = response.data;
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

function truncate(str) {
  return str.split("\n")[0];
}
</script>