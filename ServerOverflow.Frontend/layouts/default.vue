<template>
  <Navbar/>
  <div class="bg-base-100 drawer mx-auto max-w-[100rem] lg:drawer-open">
    <input id="drawer" type="checkbox" class="drawer-toggle">
    <div class="drawer-content">
      <div class="w-full px-6">
        <slot />
      </div>
    </div>
  </div>
</template>