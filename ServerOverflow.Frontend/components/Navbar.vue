<template>
  <nav class="navbar bg-base-200 p-0 px-5">
    <div class="flex-1">
      <a class="inline-block text-xl align-middle mr-2">
        <NuxtImg src="/img/title.png" class="max-h-7"/>
      </a>
      <ul class="menu menu-horizontal items-center px-1 space-x-1 hidden lg:inline-flex">
        <li><NuxtLink to="/">Home</NuxtLink></li>
        <li><NuxtLink to="/faq">FAQ</NuxtLink></li>
        <li><NuxtLink to="/stats">Statistics</NuxtLink></li>
        <li><a>Servers</a></li>
      </ul>
    </div>
    <div class="flex-none">
      <ul class="menu menu-horizontal items-center px-1 space-x-1 hidden lg:flex">
        <li><button class="btn btn-ghost btn-info">Register</button></li>
        <li><button class="btn btn-outline btn-success">Login</button></li>
      </ul>
      <div class="dropdown dropdown-end lg:hidden">
        <div tabindex="0" role="button" class="btn btn-ghost p-2">
          <Icon name="fa6-solid:angle-down" size="2em" />
        </div>
        <ul class="menu dropdown-content bg-base-200 rounded-box mt-3 w-52 p-2 shadow">
          <li><NuxtLink to="/">Home <Icon name="fa6-solid:house-chimney" class="ml-auto w-5 h-5"/></NuxtLink></li>
          <li><NuxtLink to="/faq">FAQ <Icon name="fa6-solid:circle-question" class="ml-auto w-5 h-5"/></NuxtLink></li>
          <li><NuxtLink to="/stats">Statistics <Icon name="fa6-solid:chart-simple" class="ml-auto w-5 h-5"/></NuxtLink></li>
          <li><a>Servers <Icon name="fa6-solid:server" class="ml-auto w-5 h-5"/></a></li>
          <li><div class="divider divider-start m-0"></div></li>
          <li><a>Register <Icon name="fa6-solid:plus" class="ml-auto w-5 h-5"/></a></li>
          <li><a>Login <Icon name="fa6-solid:arrow-right-to-bracket" class="ml-auto w-5 h-5"/></a></li>
        </ul>
      </div>
    </div>
  </nav>
</template>
<script setup lang="ts">
</script>