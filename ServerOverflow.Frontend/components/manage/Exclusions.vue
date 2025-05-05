<template>
  <span ref="scrollTarget"></span>
  <h2 class="text-2xl font-bold">Exclusions</h2>
  <p class="text mt-1">IP range and individual address exclusions</p>
  <div class="divider my-2"></div>
  <div class="join w-full">
    <label class="join-item input mb-2 w-full">
      <Icon name="ci:search-magnifying-glass" size="1.5em" class="opacity-50"/>
      <input type="search" class="grow" placeholder="Enter custom query" v-model="query" />
    </label>
    <button class="join-item btn btn-primary text-2xl" @click="notImplemented()">+</button>
  </div>
  <div v-if="!exclusions">
    <div v-if="error" class="alert alert-error alert-soft">
      <span>Failed to fetch exclusions from the backend</span>
    </div>
    <div v-else class="alert alert-error alert-soft">
      <span>No results were found for your query</span>
    </div>
  </div>
  <div v-else>
    <Pagination :data="exclusions" :open-page="openPage"/>
    <table class="table">
      <thead>
      <tr>
        <th>#</th>
        <th>Ranges</th>
        <th class="hidden sm:table-cell">Comment</th>
        <th></th>
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
        <td class="hidden sm:table-cell max-w-70 md:max-w-100 xl:max-w-110 overflow-x-clip whitespace-nowrap text-ellipsis ">
          {{ truncate(exclusion.comment) }}
        </td>
        <th class="w-full text-right">
          <button class="btn btn-sm btn-primary btn-outline" @click="notImplemented">Edit</button>
          <button class="btn btn-sm btn-error btn-outline ml-2 hidden sm:inline-block" @click="notImplemented">Delete</button>
        </th>
      </tr>
      </tbody>
    </table>
    <Pagination :data="exclusions" :open-page="openPage" :scroll-to="scrollTarget"/>
  </div>
</template>

<script setup>
const headers = useRequestHeaders(['cookie']);
const config = useRuntimeConfig();
const router = useRouter();
const route = useRoute();

const scrollTarget = useTemplateRef('scrollTarget');
const query = ref(route.query.q);
const exclusions = ref(null);
const error = ref(null);

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
  const { data, error: innerError } = await useFetch(`${config.public.apiBase}exclusion/search`, {
    credentials: 'include',
    headers: headers,
    method: 'POST',
    query: {
      page: route.query.page || '1',
      query: query.value
    }
  })

  exclusions.value = data.value;
  error.value = innerError.value;
  await updateRoute();
}

async function openPage(page) {
  await updateRoute(page);
  await update();
}

await update();

function truncate(str) {
  return str.split("\n")[0];
}

watch(query, update);
</script>