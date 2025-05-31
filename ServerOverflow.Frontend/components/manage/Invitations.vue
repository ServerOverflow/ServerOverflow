<template>
  <h2 class="text-2xl font-bold">Invitations</h2>
  <p class="text mt-1">Codes generated for registration gating</p>
  <div class="divider my-2"></div>
  <div v-if="!invitations && status !== 'pending'">
    <div v-if="error" class="alert alert-error alert-soft">
      <span>Failed to fetch exclusions from the backend</span>
    </div>
    <div v-if="!error" class="alert alert-error alert-soft">
      <span>No results were found for your query</span>
    </div>
    <button v-if="!error" class="btn btn-accent btn-outline mt-2" @click="createDialog.open()">
      Create new invitation
      <Icon name="fa6-solid:plus"/>
    </button>
  </div>
  <div v-else>
    <table class="table">
      <thead>
      <tr>
        <th>#</th>
        <th>Used</th>
        <th>Code</th>
        <th class="hidden md:table-cell">Badge text</th>
        <th class="hidden md:table-cell">Creator</th>
        <th class="w-full text-right">
          <button class="btn btn-sm btn-accent btn-outline w-19" @click="createDialog.open()">
            New <Icon name="fa6-solid:plus"/>
          </button>
        </th>
      </tr>
      </thead>
      <tbody>
      <tr v-if="!invitations && status === 'pending'" v-for="index in 10">
        <td>
          {{ index }}
        </td>
        <td>
          <div class="skeleton mask mask-squircle h-6 w-6"></div>
        </td>
        <td>
          <div class="skeleton h-6 w-20"></div>
        </td>
        <td class="hidden md:table-cell">
          <div class="skeleton h-6 w-20"></div>
        </td>
        <td class="hidden md:table-cell">
          <div class="skeleton h-4 w-25"></div>
        </td>
        <th class="w-full">
          <div class="skeleton h-8 w-20 ml-auto"></div>
        </th>
      </tr>
      <tr v-else class="whitespace-nowrap" v-for="(invitation, index) in invitations">
        <td>
          {{ index + 1 }}
        </td>
        <td>
          <input type="checkbox" class="checkbox checkbox-primary pointer-events-none" :checked="invitation.used" />
        </td>
        <td>
          <div class="bg-base-300 p-1 px-2 rounded-md w-min !font-mono">
            {{ invitation.code }}
          </div>
        </td>
        <td class="hidden md:table-cell truncate">
          <div class="badge badge-primary">{{ invitation.badgeText }}</div>
        </td>
        <td class="hidden md:table-cell truncate">
          <a class="link link-hover link-primary" @click="notImplemented">
            <obf v-if="invitation.creatorUsername === null">God himself</obf>
            <span v-else>{{ invitation.creatorUsername }}</span>
          </a>
        </td>
        <th class="w-full text-right">
          <div class="join">
            <button class="join-item btn btn-sm btn-primary btn-outline" @click="editDialog.open(invitation)">
              <Icon name="fa6-solid:pencil" class="icon-xs"/>
            </button>
            <button class="join-item btn btn-sm btn-error btn-outline" @click="deleteDialog.open(invitation)">
              <Icon name="fa6-solid:trash" class="icon-xs"/>
            </button>
          </div>
        </th>
      </tr>
      </tbody>
    </table>
  </div>
  <InvitationCreate ref="createDialog" :update="refresh"/>
  <InvitationDelete ref="deleteDialog" :update="refresh"/>
  <InvitationEdit ref="editDialog" :update="refresh"/>
</template>

<script setup>
const { data: invitations, error, refresh, status } = await useAuthFetch(`/invitation/list`, { lazy: true })
const createDialog = useTemplateRef('createDialog');
const deleteDialog = useTemplateRef('deleteDialog');
const editDialog = useTemplateRef('editDialog');
</script>