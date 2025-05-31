<template>
  <div v-if="!keys && status !== 'pending'">
    <div v-if="error" class="alert alert-error alert-soft">
      <span>Failed to fetch profiles from the backend</span>
    </div>
    <div v-if="!error" class="alert alert-error alert-soft mb-2">
      <span>No results were found for your query</span>
    </div>
    <div v-if="!error">
      <button class="btn !btn-primary mr-2" @click="createDialog.open()">
        Add new API key
        <Icon name="fa6-solid:plus"/>
      </button>
    </div>
  </div>
  <div v-else>
    <table class="table">
      <thead>
      <tr>
        <th>#</th>
        <th>Name</th>
        <th class="hidden sm:table-cell">Expires</th>
        <th class="hidden md:table-cell">Permissions</th>
        <th class="w-full text-right">
          <button class="btn btn-sm btn-accent btn-outline w-19" @click="createDialog.open()">
            New
            <Icon name="fa6-solid:plus"/>
          </button>
        </th>
      </tr>
      </thead>
      <tbody>
      <tr v-if="status === 'pending'" v-for="index in 10">
        <td>
          <div class="skeleton mask mask-squircle h-12 w-12"></div>
        </td>
        <td>
          <div class="skeleton h-4 w-32"></div>
        </td>
        <td class="hidden sm:table-cell">
          <div class="skeleton mask mask-squircle h-6 w-6"></div>
        </td>
        <th class="w-full">
          <div class="skeleton h-8 w-20 ml-auto"></div>
        </th>
      </tr>
      <tr v-else class="whitespace-nowrap" v-for="(key, index) in keys">
        <td>
          {{ index + 1 }}
        </td>
        <td>
          {{ key.name }}
        </td>
        <td class="hidden sm:table-cell">
          {{ moment(key.expireAt).fromNow() }}
        </td>
        <td class="hidden md:table-cell whitespace-normal break-words">
          {{ replace(key.permissions.join(", "), 'None') }}
        </td>
        <th class="w-full text-right">
          <div class="join">
            <button class="join-item btn btn-sm btn-primary btn-outline" @click="editDialog.open(key)">
              <Icon name="fa6-solid:pencil" class="icon-xs"/>
            </button>
            <button class="join-item btn btn-sm btn-error btn-outline" @click="deleteDialog.open(key)">
              <Icon name="fa6-solid:trash" class="icon-xs"/>
            </button>
          </div>
        </th>
      </tr>
      </tbody>
    </table>
  </div>
  <ApiKeyCreate ref="createDialog" :update="refresh"/>
  <ApiKeyDelete ref="deleteDialog" :update="refresh"/>
  <ApiKeyEdit ref="editDialog" :update="refresh"/>
</template>

<script setup>
import moment from 'moment'

const { data: keys, error, refresh, status } = await useAuthFetch(`/key/list`, { lazy: true })
const createDialog = useTemplateRef('createDialog');
const deleteDialog = useTemplateRef('deleteDialog');
const editDialog = useTemplateRef('editDialog');
</script>