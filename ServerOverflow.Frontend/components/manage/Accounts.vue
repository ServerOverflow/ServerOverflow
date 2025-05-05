<template>
  <h2 class="text-2xl font-bold">Accounts</h2>
  <p class="text mt-1">User accounts on ServerOverflow</p>
  <div class="divider my-2"></div>
  <div v-if="!accounts">
    <div role="alert" class="alert alert-error alert-soft">
      <span>Failed to fetch accounts from the backend!</span>
    </div>
  </div>
  <div v-else>
    <table class="table">
      <thead>
      <tr>
        <th></th>
        <th>Username</th>
        <th class="hidden md:table-cell">Permissions</th>
        <th>Invited by</th>
        <th></th>
      </tr>
      </thead>
      <tbody>
      <tr class="whitespace-nowrap" v-for="account in accounts">
        <td>
          <div class="avatar">
            <div class="mask mask-squircle h-12 w-12">
              <NuxtImg src="/img/user.png"/>
            </div>
          </div>
        </td>
        <td>
          <div class="flex items-center gap-3">
            <div>
              <div class="font-bold">{{ account.username }}</div>
              <div class="badge badge-primary badge-xs">{{ account.badgeText }}</div>
            </div>
          </div>
        </td>
        <td class="hidden md:table-cell truncate">
          {{ replace(account.permissions.join(", "), 'None') }}
        </td>
        <td class="truncate">
          <a class="link link-hover link-primary" @click="notImplemented">
            <obf v-if="account.inviteeUsername === null">God himself</obf>
            <span v-else>{{ account.inviteeUsername }}</span>
          </a>
        </td>
        <th class="w-full text-right">
          <button class="btn btn-sm btn-primary btn-outline" @click="notImplemented">Edit</button>
          <button class="btn btn-sm btn-error btn-outline ml-2 hidden sm:inline-block" @click="notImplemented">Delete</button>
        </th>
      </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup>
const headers = useRequestHeaders(['cookie'])
const config = useRuntimeConfig()

const { data: accounts } = await useFetch(`${config.public.apiBase}user/list`, { headers, credentials: 'include' })
</script>