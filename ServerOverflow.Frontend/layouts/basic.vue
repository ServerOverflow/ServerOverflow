<template>
  <Navbar/>
  <div class="bg-base-100 drawer mx-auto max-w-[100rem] lg:drawer-open mb-20">
    <input id="drawer" type="checkbox" class="drawer-toggle">
    <div class="drawer-content">
      <div class="w-full px-6 mt-10">
        <slot />
      </div>
    </div>
  </div>
</template>