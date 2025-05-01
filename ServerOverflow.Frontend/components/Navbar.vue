<template>
  <nav class="navbar bg-base-200 p-0 px-5">
    <div class="flex-1">
      <ul class="menu menu-horizontal items-center px-1 space-x-1">
        <li><a class="text-xl">
          <NuxtImg src="/img/title.png" class="max-h-7"/>
        </a></li>
        <li><a>Home</a></li>
        <li><a>FAQ</a></li>
        <li><a>Statistics</a></li>
        <li><a>Servers</a></li>
      </ul>
    </div>
    <div class="flex-none">
      <ul class="menu menu-horizontal items-center px-1 space-x-1">
        <li>
          <button class="btn btn-ghost btn-info">Register</button>
        </li>
        <li>
          <button class="btn btn-outline btn-success">Login</button>
        </li>
      </ul>
    </div>
  </nav>
</template>