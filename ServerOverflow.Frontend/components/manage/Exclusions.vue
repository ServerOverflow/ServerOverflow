<template>
  <span ref="scrollTarget"></span>
  <h2 class="text-2xl font-bold">Exclusions</h2>
  <p class="text mt-1">IP range and individual address exclusions</p>
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
      Found {{ exclusions?.totalMatches || 0 }} exclusions ({{ formattedLatency }})
    </span>
    <button class="flex-1 justify-end flex flex-row select-none items-center link link-hover" @click="queryDocs.open()">
      <Icon name="fa6-solid:circle-question" class="icon-xs" />
      <span class="opacity-80 ml-1">How do I use operators?</span>
    </button>
  </div>
  <div v-if="!exclusions && status !== 'pending'">
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
    <Pagination :data="paginationData" :open-page="openPage"/>
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
      <tr v-if="!exclusions && status === 'pending'" v-for="index in 10">
        <td>
          {{ index }}
        </td>
        <td>
          <div class="skeleton h-4 w-18"></div>
        </td>
        <td class="hidden sm:table-cell">
          <div class="skeleton h-4 w-60"></div>
        </td>
        <th class="w-full">
          <div class="skeleton h-8 w-20 ml-auto"></div>
        </th>
      </tr>
      <tr v-else class="whitespace-nowrap" v-for="(exclusion, index) in exclusions.items">
        <td>
          {{ (exclusions.currentPage - 1) * 25 + index + 1 }}
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
    <Pagination :data="paginationData" :open-page="openPage" :scroll-to="scrollTarget"/>
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
const query = ref(route.query.query);
const params = computed(() => ({
  page: route.query.page || '1',
  query: route.query.query
}))

const { data: exclusions, error, status } = await useAuthFetch(`/exclusion/search`, {
  method: 'POST', query: params, watch: [params], lazy: true
})

const paginationData = computed(() =>
    exclusions.value || { currentPage: 1, totalPages: 10 }
);

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

function truncate(str) {
  return str.split("\n")[0];
}
</script>