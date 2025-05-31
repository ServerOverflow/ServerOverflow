<template>
  <div class="flex flex-row w-full items-center mb-2" v-if="data">
    <div class="join">
      <button class="join-item btn btn-soft hidden md:inline-block" @click="open(1)">&lt;&lt;</button>
      <button class="join-item btn btn-soft" @click="openRel(-1)">&lt;</button>
    </div>
    <div class="join mx-auto">
      <button v-if="data.currentPage > 1" class="join-item btn btn-soft" @click="open(1)">1</button>
      <button v-if="data.currentPage > 1" class="join-item items-center px-4">...</button>
      <button v-if="data.currentPage - 2 > 0" class="join-item btn btn-soft hidden md:inline-block" @click="openRel(-2)">{{ data.currentPage - 2 }}</button>
      <button v-if="data.currentPage - 1 > 0" class="join-item btn btn-soft hidden sm:inline-block" @click="openRel(-1)">{{ data.currentPage - 1 }}</button>
      <button class="join-item btn btn-accent">{{ data.currentPage }}</button>
      <button v-if="data.currentPage + 1 <= data.totalPages" class="join-item btn btn-soft hidden sm:inline-block" @click="openRel(1)">{{ data.currentPage + 1 }}</button>
      <button v-if="data.currentPage + 2 <= data.totalPages" class="join-item btn btn-soft hidden md:inline-block" @click="openRel(2)">{{ data.currentPage + 2 }}</button>
      <button v-if="data.totalPages - data.currentPage > 0" class="join-item items-center px-4">...</button>
      <button v-if="data.totalPages - data.currentPage > 0" class="join-item btn btn-soft" @click="open(data.totalPages)">{{ data.totalPages }}</button>
    </div>
    <div class="join">
      <button class="join-item btn btn-soft" @click="openRel(1)">&gt;</button>
      <button class="join-item btn btn-soft hidden md:inline-block" @click="open(data.totalPages)">&gt;&gt;</button>
    </div>
  </div>
</template>

<script setup>
const { data, openPage, scrollTo } = defineProps({
  data: {
    type: Object,
    required: true
  },
  openPage: {
    type: Function,
    required: true
  },
  scrollTo: Object
})

async function open(page) {
  if (page < 1) page = 1;
  if (page > data.totalPages)
    page = data.totalPages;
  const result = openPage(page);
  if (result instanceof Promise)
    await result;
  await nextTick(() => {
    if (scrollTo)
      scrollTo.scrollIntoView({ behavior: 'smooth' });
  });
}

async function openRel(off) {
  await open(data.currentPage + off);
}
</script>