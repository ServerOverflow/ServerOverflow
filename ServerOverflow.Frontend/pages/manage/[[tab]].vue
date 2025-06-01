<template>
  <div class="flex flex-col xl:flex-row max-w-420 w-full mx-auto gap-y-8 gap-x-16 xl:px-16">
    <div class="w-full xl:w-auto bg-base-300/50 backdrop-blur-md xl:bg-inherit overflow-x-auto sticky top-16 xl:h-full z-1">
      <ul class="menu lg:menu-lg menu-horizontal xl:menu-vertical gap-y-2 gap-x-1 flex w-max xl:w-80 xl:py-16">
        <li>
          <NuxtLink class="font-semibold gap-3" :class="{ 'menu-active': tab === 'audit' }" to="/manage/audit">
            <Icon name="fa6-solid:file-shield" class="icon-md" />
            Audit logs
          </NuxtLink>
        </li>
        <li>
          <NuxtLink class="font-semibold gap-3" :class="{ 'menu-active': tab === 'honeypot' }" to="/manage/honeypot">
            <Icon name="fa6-solid:cloud-bolt" class="icon-md" />
            Honeypot logs
          </NuxtLink>
        </li>
        <li>
          <NuxtLink class="font-semibold gap-3" :class="{ 'menu-active': tab === 'exclusions' }" to="/manage/exclusions">
            <Icon name="fa6-solid:list" class="icon-md" />
            Exclusions
          </NuxtLink>
        </li>
        <li>
          <NuxtLink class="font-semibold gap-3" :class="{ 'menu-active': tab === 'profiles' }" to="/manage/profiles">
            <Icon name="fa6-solid:robot" class="icon-md" />
            Bot Profiles
          </NuxtLink>
        </li>
        <li v-if="hasPermission('Administrator')">
          <NuxtLink class="font-semibold gap-3" :class="{ 'menu-active': tab === 'invitations' }" to="/manage/invitations">
            <Icon name="fa6-solid:ticket" class="icon-md" />
            Invitations
          </NuxtLink>
        </li>
        <li v-if="hasPermission('SearchAccounts')">
          <NuxtLink class="font-semibold gap-3" :class="{ 'menu-active': tab === 'accounts' }" to="/manage/accounts">
            <Icon name="fa6-solid:user" class="icon-md" />
            Accounts
          </NuxtLink>
        </li>
      </ul>
    </div>
    <div class="flex-1 overflow-auto w-full pt-0 px-8 xl:px-0 xl:py-16">
      <ManageInvitations v-if="tab === 'invitations'" />
      <ManageExclusions v-if="tab === 'exclusions'" />
      <ManageHoneypot v-if="tab === 'honeypot'" />
      <ManageAccounts v-if="tab === 'accounts'" />
      <ManageProfiles v-if="tab === 'profiles'" />
      <ManageAudit v-if="tab === 'audit'" />
    </div>
  </div>
</template>

<script setup>
const route = useRoute();
const tab = ref(route.params.tab || 'audit');

watch(() => route.params.tab, (newValue) => {
  tab.value = newValue || 'audit';
})
</script>