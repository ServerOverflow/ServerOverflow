<template>
  <div class="card bg-base-300/40 shadow-xl">
    <div class="card-body p-4">
      <div class="flex flex-row items-start gap-4 mb-1">
        <div class="avatar">
          <div class="w-15 rounded">
            <NuxtImg src="/img/user.png"/>
          </div>
        </div>
        <div class="flex-1 min-w-0">
          <div class="card-title text-xl link link-primary link-hover">
            <div class="text-2xl font-bold">{{ account.username }}</div>
            <div class="badge badge-primary badge-sm">{{ account.badgeText }}</div>
          </div>
          <div class="w-full text-md truncate">
            Invited by
            <a class="link link-hover link-primary" @click="notImplemented">
              <obf v-if="account.inviteeUsername === null">God himself</obf>
              <span v-else>{{ account.inviteeUsername }}</span>
            </a>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
const config = useRuntimeConfig();

defineProps({
  account: {
    type: Object,
    required: true
  }
})
</script>