<template>
  <h2 class="text-2xl font-bold">Bot Profiles</h2>
  <p class="text mt-1">Minecraft accounts used for bot joining</p>
  <div class="divider my-2"></div>
  <div v-if="!profiles">
    <div v-if="error" class="alert alert-error alert-soft">
      <span>Failed to fetch profiles from the backend</span>
    </div>
    <div v-if="!error" class="alert alert-error alert-soft">
      <span>No results were found for your query</span>
    </div>
    <div v-if="!error">
      <button class="btn !btn-primary mr-2" :class="{ 'btn-disabled': polling }" @click="add">
        Add account via Microsoft
        <Icon v-if="!polling" name="fa6-solid:plus"/>
        <span v-else class="loading loading-spinner loading-sm"></span>
      </button>
      <div class="block mt-2 sm:inline-block sm:mt-0">
        <button class="btn btn-secondary btn-soft mr-2" v-if="polling" @click="open">
          Open
          <Icon name="fa6-solid:arrow-up-right-from-square" />
        </button>
        <button class="btn btn-error btn-soft mr-2" v-if="polling" @click="cancel">
          Cancel
          <Icon name="fa6-solid:xmark" />
        </button>
      </div>
    </div>
  </div>
  <div v-else>
    <table class="table">
      <thead>
      <tr>
        <th></th>
        <th>Username</th>
        <th class="hidden sm:table-cell">Valid</th>
        <th class="w-full text-right">
          <button v-if="!polling || !loginUrl" class="btn btn-sm btn-accent btn-outline w-19" :class="{ 'btn-disabled': polling }" @click="add">
            New
            <Icon v-if="!polling" name="fa6-solid:plus"/>
            <span v-else class="loading loading-spinner loading-sm"></span>
          </button>
          <div v-if="polling && loginUrl" class="join">
            <a class="join-item btn btn-sm btn-primary btn-outline" :href="loginUrl" target=”_blank”>
              <Icon name="fa6-solid:arrow-up-right-from-square" class="icon-xs"/>
            </a>
            <button class="join-item btn btn-sm btn-error btn-outline" @click="cancel">
              <Icon name="fa6-solid:xmark" class="icon-xs"/>
            </button>
          </div>
        </th>
      </tr>
      </thead>
      <tbody>
      <tr class="whitespace-nowrap" v-for="profile in profiles">
        <td>
          <div class="avatar">
            <a class="mask mask-squircle h-12 w-12" :href="'https://namemc.com/profile/'+profile.uuid">
              <NuxtImg :src="'https://minotar.net/helm/'+profile.uuid"/>
            </a>
          </div>
        </td>
        <td>
          <a class="font-bold">{{ profile.username }}</a>
        </td>
        <td class="hidden sm:table-cell">
          <input type="checkbox" class="checkbox checkbox-primary pointer-events-none" :checked="profile.valid" />
        </td>
        <th class="w-full text-right">
          <div class="join">
            <a class="join-item btn btn-sm btn-primary btn-outline" :href="'https://namemc.com/profile/'+profile.uuid" target=”_blank”>
              <Icon name="fa6-solid:arrow-up-right-from-square" class="icon-xs"/>
            </a>
            <div class="join-item">
              <button class="join-item btn btn-sm btn-error btn-outline" @click="deleteDialog.open(profile)">
                <Icon name="fa6-solid:trash" class="icon-xs"/>
              </button>
            </div>
          </div>
        </th>
      </tr>
      </tbody>
    </table>
  </div>
  <ProfileDelete ref="deleteDialog" :update="refresh"/>
</template>

<script setup>
const { data, error, refresh } = await useAuthFetch(`/profile/list`)
const { $axios } = useNuxtApp();
const toast = useToast();

const profiles = ref(data);
const loginUrl = ref(null);
const polling = ref(false);
const interval = ref(null);

const deleteDialog = useTemplateRef('deleteDialog');

async function add() {
  polling.value = true;
  try {
    const res = await $axios.get('/profile/add');
    if (res.data.device_code === null) {
      polling.value = false;
      toast.add({
        title: res.message,
        color: 'error'
      });
    }

    interval.value = setInterval(poll,
        res.data.interval * 1000,
        Date.now() + res.data.expires_in * 1000,
        res.data.device_code)

    loginUrl.value = `${res.data.verification_uri}?otc=${res.data.user_code}`
    window.open(loginUrl.value);
    toast.add({
      title: 'Please log into your Microsoft account',
      color: 'success'
    });
  } catch (error) {
    polling.value = false;
    handleAxiosError(error, toast);
  }
}

function cancel() {
  clearInterval(interval.value);
  loginUrl.value = null;
  polling.value = false;
  toast.add({
    title: 'Successfully cancelled the operation',
    color: 'success'
  });
}

async function poll(expiresAt, deviceCode) {
  if (Date.now() > expiresAt) {
    toast.add({
      title: 'Session expired, please try to log in again',
      color: 'warning'
    });

    clearInterval(interval.value);
    loginUrl.value = null;
    polling.value = false;
    return;
  }

  try {
    const res = await $axios.get(`/profile/poll/${deviceCode}`);
    if (res.status === 204) return;

    clearInterval(interval.value);
    loginUrl.value = null;
    polling.value = false;
    toast.add({
      title: 'Successfully added a new Minecraft account',
      color: 'success'
    });

    const { data2 } = await useAuthFetch(`/profile/list`)
    profiles.value = data2;
  } catch (error) {
    polling.value = false;
    handleAxiosError(error, toast);
  }
}
</script>