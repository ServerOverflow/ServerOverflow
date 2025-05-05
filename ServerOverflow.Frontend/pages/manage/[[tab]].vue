<template>
  <div class="flex flex-col xl:flex-row max-w-420 w-full mx-auto gap-y-8 gap-x-16 xl:px-16">
    <div class="w-full xl:w-auto bg-base-300/50 backdrop-blur-md xl:bg-inherit overflow-x-auto sticky top-16 xl:h-full">
      <ul class="menu lg:menu-lg menu-horizontal xl:menu-vertical gap-y-2 gap-x-1 flex w-max xl:w-80 xl:py-16">
        <li>
          <NuxtLink class="font-semibold gap-3" :class="{ 'menu-active': tab === 'accounts' }" to="/manage/accounts">
            <Icon name="fa6-solid:user" class="icon-md" />
            Accounts
          </NuxtLink>
        </li>
        <li v-if="hasPermission('Administrator')">
          <NuxtLink class="font-semibold gap-3" :class="{ 'menu-active': tab === 'invitations' }" to="/manage/invitations">
            <Icon name="fa6-solid:ticket" class="icon-md" />
            Invitations
          </NuxtLink>
        </li>
        <li>
          <NuxtLink class="font-semibold gap-3" :class="{ 'menu-active': tab === 'profiles' }" to="/manage/profiles">
            <Icon name="fa6-solid:robot" class="icon-md" />
            Bot Profiles
          </NuxtLink>
        </li>
        <li>
          <NuxtLink class="font-semibold gap-3" :class="{ 'menu-active': tab === 'exclusions' }" to="/manage/exclusions">
            <Icon name="fa6-solid:list" class="icon-md" />
            Exclusions
          </NuxtLink>
        </li>
      </ul>
    </div>
    <div class="flex-1 overflow-auto w-full pt-0 px-8 xl:px-0 xl:py-16">
      <ManageInvitations v-if="tab === 'invitations'" />
      <ManageExclusions v-if="tab === 'exclusions'" />
      <ManageAccounts v-if="tab === 'accounts'" />
      <ManageProfiles v-if="tab === 'profiles'" />
    </div>
  </div>
  <AuthWatcher/>
</template>

<script setup>
const route = useRoute();
const tab = ref(route.params.tab || 'accounts');

watch(() => route.params.tab, (newValue) => {
  tab.value = newValue || 'accounts';
})
</script>