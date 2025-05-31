<template>
  <div class="card bg-base-300/40 shadow-xl">
    <div class="card-body p-4">
      <div class="flex flex-row items-start gap-4 mb-1">
        <div class="avatar">
          <div class="w-18 lg:w-22 rounded">
            <img
                class="w-full h-full object-contain"
                :src="`${config.public.apiBase}server/${server.id}.png`"
                :style="{ background: 'url(/img/default.png)', backgroundSize: 'cover' }"
                onerror="this.src='/img/default.png';"
                loading="lazy"
                alt=""
            />
          </div>
        </div>
        <div class="flex-1 min-w-0">
          <h2 class="card-title text-xl link link-primary link-hover">
            <a :href="'/server/' + server.id">
              {{ server.ip }}:{{ server.port }}
            </a>
          </h2>
          <div class="w-full truncate">
            <MinecraftText :payload="server.ping.description" class="text-md lg:text-lg"/>
          </div>
        </div>
      </div>
      <div class="flex gap-2 overflow-x-auto whitespace-nowrap -mb-3 pb-3">
        <div class="join">
          <div class="join-item badge badge-neutral text-xs">Protocol</div>
          <div class="join-item badge text-xs" :class="server.ping.version?.protocol ? 'badge-success' : 'badge-warning'">
            <span v-if="server.ping.version?.protocol">{{ server.ping.version.protocol }}</span>
            <span v-else><Icon name="fa6-solid:question"/></span>
          </div>
        </div>

        <div class="join">
          <div class="join-item badge badge-neutral text-xs">Version</div>
          <div class="join-item badge text-xs" :class="server.ping.version?.name ? 'badge-success' : 'badge-warning'">
            <span v-if="server.ping.version?.name">{{ server.ping.version.name }}</span>
            <span v-else><Icon name="fa6-solid:question"/></span>
          </div>
        </div>

        <div class="join">
          <div class="join-item badge badge-neutral text-xs">Players</div>
          <div class="join-item badge text-xs" :class="(server.ping.players?.online != null && server.ping.players?.max != null) ? 'badge-success' : 'badge-warning'">
            <span v-if="server.ping.players?.online != null && server.ping.players?.max != null">{{ server.ping.players.online }}/{{ server.ping.players.max }}</span>
            <span v-else><Icon name="fa6-solid:question"/></span>
          </div>
        </div>

        <div v-if="server.ping.chatReporting" class="badge badge-info text-xs">Chat Reporting</div>
        <div v-if="server.ping.isForge" class="badge badge-info text-xs">Forge</div>
      </div>
    </div>
  </div>
</template>

<script setup>
const config = useRuntimeConfig();

defineProps({
  server: {
    type: Object,
    required: true
  }
})
</script>