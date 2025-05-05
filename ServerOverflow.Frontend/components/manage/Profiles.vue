<template>
  <h2 class="text-2xl font-bold">Bot Profiles</h2>
  <p class="text mt-1">Minecraft accounts used for bot joining</p>
  <div class="divider my-2"></div>
  <div class="my-2">
    <button class="join-item btn btn-primary" @click="notImplemented()">
      Add profile
      <Icon name="fa6-solid:plus"/>
    </button>
  </div>
  <div v-if="!profiles">
    <div role="alert" class="alert alert-error alert-soft">
      <span>Failed to fetch profiles from the backend!</span>
    </div>
  </div>
  <div v-else>
    <table class="table">
      <thead>
      <tr>
        <th></th>
        <th>Username</th>
        <th>Valid</th>
        <th></th>
      </tr>
      </thead>
      <tbody>
      <tr class="whitespace-nowrap" v-for="profile in profiles">
        <td>
          <div class="avatar">
            <div class="mask mask-squircle h-12 w-12">
              <NuxtImg :src="'https://minotar.net/helm/'+profile.uuid"/>
            </div>
          </div>
        </td>
        <td>
          <div class="flex items-center gap-3">
            <div>
              <div class="font-bold">{{ profile.username }}</div>
            </div>
          </div>
        </td>
        <td>
          <input type="checkbox" class="checkbox checkbox-primary pointer-events-none" :checked="profile.valid" />
        </td>
        <th class="w-full text-right">
          <button class="btn btn-sm btn-error btn-outline ml-2" @click="notImplemented">Delete</button>
        </th>
      </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup>
const headers = useRequestHeaders(['cookie'])
const config = useRuntimeConfig()

const { data: profiles } = await useFetch(`${config.public.apiBase}profile/list`, { headers, credentials: 'include' })
</script>