<template>
  <nav class="navbar bg-base-300/50 backdrop-blur-sm sticky z-100 top-0 p-0 px-5">
    <div class="flex-1">
      <a class="inline-block text-xl align-middle mr-2">
        <NuxtImg src="/img/title.png" class="max-h-7"/>
      </a>
      <ul class="menu menu-horizontal items-center px-1 space-x-1 hidden lg:inline-flex">
        <li><NuxtLink to="/">Home</NuxtLink></li>
        <li><NuxtLink to="/faq">FAQ</NuxtLink></li>
        <li><NuxtLink to="/stats">Statistics</NuxtLink></li>
        <li v-if="user.value"><NuxtLink to="/">Manage</NuxtLink></li>
        <li v-if="user.value"><NuxtLink to="/">Servers</NuxtLink></li>
        <li v-if="user.value"><NuxtLink to="/api">API</NuxtLink></li>
      </ul>
    </div>
    <div class="flex-none">
      <ul v-if="!user.value" class="menu menu-horizontal items-center px-1 space-x-1 hidden lg:flex">
        <li><button class="btn btn-ghost btn-info" onclick="register.showModal()">Register</button></li>
        <li><button class="btn btn-outline btn-success" onclick="login.showModal()">Login</button></li>
      </ul>
      <div class="inline-block" v-else>
        <div class="dropdown dropdown-end">
          <div tabindex="0" role="button" class="btn btn-ghost">
            {{ user.value.username }}
            <Icon name="fa6-solid:angle-down"/>
          </div>
          <ul class="menu dropdown-content bg-base-200 rounded-box mt-3 w-52 p-2 shadow">
            <li><NuxtLink to="/">Profile <Icon name="fa6-solid:user" class="ml-auto w-5 h-5"/></NuxtLink></li>
            <li><button @click="logout">Logout <Icon name="fa6-solid:right-from-bracket" class="ml-auto w-5 h-5"/></button></li>
          </ul>
        </div>
      </div>
      <div class="dropdown dropdown-end lg:hidden">
        <div tabindex="0" role="button" class="btn btn-ghost p-2">
          <Icon name="fa6-solid:angle-down" size="2em" />
        </div>
        <ul class="menu dropdown-content bg-base-200 rounded-box mt-3 w-52 p-2 shadow">
          <li><NuxtLink to="/">Home <Icon name="fa6-solid:house-chimney" class="ml-auto w-5 h-5"/></NuxtLink></li>
          <li><NuxtLink to="/faq">FAQ <Icon name="fa6-solid:circle-question" class="ml-auto w-5 h-5"/></NuxtLink></li>
          <li><NuxtLink to="/stats">Statistics <Icon name="fa6-solid:chart-simple" class="ml-auto w-5 h-5"/></NuxtLink></li>
          <li v-if="user.value"><NuxtLink to="/">Management <Icon name="fa6-solid:screwdriver-wrench" class="ml-auto w-5 h-5"/></NuxtLink></li>
          <li v-if="user.value"><NuxtLink to="/">Servers <Icon name="fa6-solid:server" class="ml-auto w-5 h-5"/></NuxtLink></li>
          <li v-if="user.value"><NuxtLink to="/api">API <Icon name="fa6-solid:book-skull" class="ml-auto w-5 h-5"/></NuxtLink></li>
          <li v-if="!user.value"><div class="divider divider-start m-0"></div></li>
          <li v-if="!user.value"><button onclick="register.showModal()">Register <Icon name="fa6-solid:plus" class="ml-auto w-5 h-5"/></button></li>
          <li v-if="!user.value"><button onclick="login.showModal()">Login <Icon name="fa6-solid:arrow-right-to-bracket" class="ml-auto w-5 h-5"/></button></li>
        </ul>
      </div>
    </div>
  </nav>
  <Register/>
  <Login/>
</template>

<script>
export default {
  computed: {
    user() {
      return useState('user', () => null)
    },
    toast() {
      return useToast()
    }
  },
  methods: {
    async logout(data, node) {
      await this.$axios.get('/user/logout', data);
      this.user.value = null;
      this.toast.add({
        title: 'Successfully logged out',
        color: 'success'
      });
    }
  }
}
</script>