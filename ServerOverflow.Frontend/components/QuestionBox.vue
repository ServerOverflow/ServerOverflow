<template>
  <div class="collapse collapse-arrow join-item bg-base-200/10 border-base-300 border">
    <input type="checkbox" />
    <div class="collapse-title font-semibold">
      <slot />
    </div>
    <div class="collapse-content text-sm">
      <article class="prose responsive-breaks">
        <slot name="content" mdc-unwrap="p" />
      </article>
    </div>
  </div>
</template>