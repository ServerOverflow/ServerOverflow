<template>
  <div class="flex flex-col h-screen">
    <Navbar/>
    <div class="flex justify-center items-center flex-grow hero">
      <div class="hero-content flex-col-reverse xl:flex-row items-stretch">
        <video width="340" height="768" controls>
          <source src="/img/song.mp4" type="video/mp4">
          Your browser does not support the video tag.
        </video>
        <div class="flex flex-col mt-5 xl:mt-0">
          <div class="flex-1">
            <h1 class="text-5xl font-bold">Welcome to ServerOverflow</h1>
            <p class="py-6 text-xl mb-3">
              This is a <b>Minecraft server scanner</b>, which was created for data hoarding and collecting statistics.
              Check out the FAQ if you would like to opt out, or simply know more about how it all works.<br><br>
              The scanner periodically generates statistics reports that I thought would be interesting to share.
              As an example, you can see how many modded servers exist, and how many people left chat reporting enabled.
            </p>
            <div class="stats shadow-lg">
              <div class="stat">
                <div class="stat-figure text-primary">
                  <Icon name="fa6-solid:server" size="2em"/>
                </div>
                <div class="stat-title">Total servers</div>
                <div class="stat-value text-primary">350,789</div>
                <div class="stat-desc">How many servers we found</div>
              </div>

              <div class="stat">
                <div class="stat-figure text-secondary">
                  <Icon name="fa6-solid:tower-broadcast" size="2em"/>
                </div>
                <div class="stat-title">Active servers</div>
                <div class="stat-value text-secondary">165,789</div>
                <div class="stat-desc">How many servers are online</div>
              </div>

              <div class="stat">
                <div class="stat-figure text-accent">
                  <Icon name="fa6-solid:person" size="2em"/>
                </div>
                <div class="stat-title">Online players</div>
                <div class="stat-value text-accent">14,891</div>
                <div class="stat-desc">[insert subway surfers gif]</div>
              </div>
            </div>
          </div>
          <div class="mt-auto space-x-2 pt-5">
            <NuxtLink to="/faq">
              <button class="btn btn-lg btn-outline">
                <a class="pr-1">FAQ</a>
                <Icon name="fa6-solid:angle-right"/>
              </button>
            </NuxtLink>
            <NuxtLink to="/stats">
              <button class="btn btn-lg btn-outline">
                <a class="pr-1">Statistics</a>
                <Icon name="fa6-solid:angle-right"/>
              </button>
            </NuxtLink>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
  layout: false
})
</script>