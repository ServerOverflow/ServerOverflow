<template>
  <h2 class="text-2xl font-bold">Accounts</h2>
  <p class="text mt-1">User accounts on ServerOverflow</p>
  <div class="divider my-2"></div>
  <div v-if="!accounts">
    <div v-if="error" class="alert alert-error alert-soft">
      <span>Failed to fetch exclusions from the backend</span>
    </div>
    <div v-if="!error" class="alert alert-error alert-soft">
      <span>No results were found for your query</span>
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
        <td class="hidden md:table-cell whitespace-normal break-words">
          {{ replace(account.permissions.join(", "), 'None') }}
        </td>
        <td>
          <a class="link link-hover link-primary" @click="notImplemented">
            <obf v-if="account.inviteeUsername === null">God himself</obf>
            <span v-else>{{ account.inviteeUsername }}</span>
          </a>
        </td>
        <th class="w-full text-right">
          <div class="join">
            <button class="join-item btn btn-sm btn-primary btn-outline" @click="editDialog.open(account)">
              <Icon name="fa6-solid:pencil" class="icon-xs"/>
            </button>
            <button class="join-item btn btn-sm btn-error btn-outline" @click="deleteDialog.open(account)">
              <Icon name="fa6-solid:trash" class="icon-xs"/>
            </button>
          </div>
        </th>
      </tr>
      </tbody>
    </table>
  </div>
  <AccountDelete ref="deleteDialog" :update="refresh"/>
  <AccountEdit ref="editDialog" :update="refresh"/>
</template>

<script setup>
const { data: accounts, error, refresh } = await useAuthFetch(`/user/list`)
const deleteDialog = useTemplateRef('deleteDialog');
const editDialog = useTemplateRef('editDialog');
</script>