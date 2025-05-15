<template>
  <div class="flex xl:justify-center items-center flex-grow hero">
    <div class="hero-content flex-col-reverse xl:flex-row items-stretch">
      <video width="340" height="510" controls>
        <source src="/img/song.mp4" type="video/mp4">
        Your browser does not support the video tag.
      </video>
      <div class="flex flex-col max-w-screen pr-5 mt-5 xl:mt-0">
        <div class="flex-1 flex-wrap text-wrap">
          <h1 class="font-bold text-3xl sm:text-4xl md:text-5xl">Welcome to ServerOverflow</h1>
          <p class="py-6 text-md md:text-xl">
            This is a <b>Minecraft server scanner</b>, which was created for data hoarding and collecting statistics.
            Check out the FAQ if you would like to opt out, or simply know more about how it all works.<br><br>
            The scanner periodically generates statistics reports that I thought would be interesting to share.
            As an example, you can see how many modded servers exist, and how many people left chat reporting enabled.
            All of the data is neatly visualised for your viewing pleasure.
          </p>
          <div class="stats stats-vertical lg:stats-horizontal shadow-lg w-full items-stretch">
            <div class="stat flex-1">
              <div class="stat-figure text-primary">
                <Icon name="fa6-solid:server" class="icon-xl"/>
              </div>
              <div class="stat-title">Total servers</div>
              <div class="stat-value text-primary">{{ formatNumber(stats.totalServers) }}</div>
              <div class="stat-desc">Seen at least once</div>
            </div>

            <div class="stat flex-1">
              <div class="stat-figure text-secondary">
                <Icon name="fa6-solid:power-off" class="icon-xl"/>
              </div>
              <div class="stat-title">Online servers</div>
              <div class="stat-value text-secondary">{{ formatNumber(stats.onlineServers) }}</div>
              <div class="stat-desc">Last seen in 24 hours</div>
            </div>

            <div class="stat flex-1">
              <div class="stat-figure text-accent">
                <Icon name="fa6-solid:wrench" class="icon-xl"/>
              </div>
              <div class="stat-title">Unconfigured servers</div>
              <div class="stat-value text-accent">{{ formatNumber(stats.notConfiguredServers) }}</div>
              <div class="stat-desc">With default configuration</div>
            </div>
          </div>
        </div>
        <div class="mt-auto space-x-2 pt-5">
          <NuxtLink to="/faq">
            <button class="btn btn-lg btn-outline">
              <span class="pr-1">Tell me more what the hell is this</span>
              <Icon name="fa6-solid:angle-right"/>
            </button>
          </NuxtLink>
          <NuxtLink to="/stats">
            <button class="btn btn-lg btn-outline">
              <span class="pr-1">Statistics</span>
              <Icon name="fa6-solid:angle-right"/>
            </button>
          </NuxtLink>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
const { data: stats } = await useAuthFetch(`/server/stats`);

function formatNumber(number) {
  if (number === undefined) return '[error]';
  return number.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}
</script>