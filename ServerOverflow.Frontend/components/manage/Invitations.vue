<template>
  <h2 class="text-2xl font-bold">Invitations</h2>
  <p class="text mt-1">Codes generated for registration gating</p>
  <div class="divider my-2"></div>
  <div v-if="!invitations">
    <div role="alert" class="alert alert-error alert-soft">
      <span>Failed to fetch invitations from the backend!</span>
    </div>
  </div>
  <div v-else>
    <table class="table">
      <thead>
      <tr>
        <th>#</th>
        <th>Used</th>
        <th>Code</th>
        <th>Badge text</th>
        <th>Creator</th>
        <th></th>
      </tr>
      </thead>
      <tbody>
      <tr class="whitespace-nowrap" v-for="(invitation, index) in invitations">
        <td>
          {{ index + 1 }}
        </td>
        <td>
          <input type="checkbox" class="checkbox checkbox-primary pointer-events-none" :checked="invitation.used" />
        </td>
        <td class="hidden md:table-cell truncate">
          <div class="bg-base-300 p-1 px-2 rounded-md !font-mono">
            {{ invitation.code }}
          </div>
        </td>
        <td class="hidden md:table-cell truncate">
          <div class="badge badge-primary badge">{{ invitation.badgeText }}</div>
        </td>
        <td class="truncate">
          <a class="link link-hover link-primary" @click="notImplemented">
            <obf v-if="invitation.creatorUsername === null">God himself</obf>
            <span v-else>{{ invitation.creatorUsername }}</span>
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

const { data: invitations } = await useFetch(`${config.public.apiBase}invitation/list`, { headers, credentials: 'include' })
</script>