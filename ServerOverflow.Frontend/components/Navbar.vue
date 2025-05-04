<template>
  <nav class="navbar bg-base-300/50 backdrop-blur-md sticky z-100 top-0 p-0 px-5">
    <div class="flex-1">
      <a class="inline-block text-xl align-middle mr-2">
        <NuxtImg src="/img/title.webp" class="max-h-7" :modifiers="{ animated: true }" />
      </a>
      <ul class="menu menu-horizontal items-center px-1 space-x-1 hidden lg:inline-flex">
        <li>
          <NuxtLink to="/" :class="{ 'menu-active': route.path === '/' } ">Home</NuxtLink>
        </li>
        <li>
          <NuxtLink to="/faq" :class="{ 'menu-active': route.path === '/faq' } ">FAQ</NuxtLink>
        </li>
        <li>
          <NuxtLink to="/stats" :class="{ 'menu-active': route.path === '/stats' } ">Statistics</NuxtLink>
        </li>
        <li v-if="user">
          <NuxtLink to="/manage" :class="{ 'menu-active': route.path === '/manage' } ">Manage</NuxtLink>
        </li>
        <li v-if="user">
          <NuxtLink to="/wip" :class="{ 'menu-active': route.path === '/servers' } ">Servers</NuxtLink>
        </li>
      </ul>
    </div>
    <div class="flex-none">
      <ul v-if="!user" class="menu menu-horizontal items-center px-1 space-x-1 hidden lg:flex">
        <li><button class="btn btn-ghost btn-info" @click="register.open()">Register</button></li>
        <li><button class="btn btn-outline btn-success" @click="login.open()">Login</button></li>
      </ul>
      <div class="hidden lg:inline-block" v-else>
        <div class="dropdown dropdown-end">
          <div tabindex="0" role="button" class="btn btn-ghost">
            {{ user.username }}
            <Icon name="fa6-solid:angle-down"/>
          </div>
          <ul class="menu dropdown-content bg-base-200 rounded-box mt-3 w-52 p-2 shadow">
            <li :class="{ 'menu-active': route.path === '/account' } ">
              <NuxtLink to="/wip">Account <Icon name="fa6-solid:user" class="ml-auto icon-2"/></NuxtLink>
            </li>
            <li>
              <button @click="logout">Logout <Icon name="fa6-solid:right-from-bracket" class="ml-auto icon-sm"/></button>
            </li>
          </ul>
        </div>
      </div>
      <div class="dropdown dropdown-end lg:hidden !static">
        <div tabindex="0" role="button" class="btn btn-ghost p-2">
          <Icon name="fa6-solid:angle-down" size="2em" />
        </div>
        <ul class="menu dropdown-content bg-base-300/50 backdrop-blur-md mt-0 w-screen p-2 absolute top-full left-0 text-lg">
          <li>
            <NuxtLink to="/">
              Home <Icon name="fa6-solid:house-chimney" class="ml-auto icon-sm"/>
            </NuxtLink>
          </li>
          <li>
            <NuxtLink to="/faq">
              FAQ <Icon name="fa6-solid:circle-question" class="ml-auto icon-sm"/>
            </NuxtLink>
          </li>
          <li>
            <NuxtLink to="/stats">
              Statistics <Icon name="fa6-solid:chart-simple" class="ml-auto icon-sm"/>
            </NuxtLink>
          </li>
          <li v-if="user">
            <NuxtLink to="/manage">
              Manage <Icon name="fa6-solid:screwdriver-wrench" class="ml-auto icon-sm"/>
            </NuxtLink>
          </li>
          <li v-if="user">
            <NuxtLink to="/wip">
              Servers <Icon name="fa6-solid:server" class="ml-auto icon-sm"/>
            </NuxtLink>
          </li>
          <li>
            <div class="divider divider-start m-0"></div>
          </li>
          <li v-if="!user">
            <button @click="register.open()">
              Register <Icon name="fa6-solid:plus" class="ml-auto icon-sm"/>
            </button>
          </li>
          <li v-if="!user">
            <button @click="login.open()">
              Login <Icon name="fa6-solid:arrow-right-to-bracket" class="ml-auto icon-sm"/>
            </button>
          </li>
          <li v-if="user">
            <NuxtLink to="/wip" :class="{ 'menu-active': route.path === '/account' }">
              {{ user.username }} <Icon name="fa6-solid:user" class="ml-auto icon-sm"/>
            </NuxtLink>
          </li>
          <li v-if="user">
            <button @click="logout">
              Logout <Icon name="fa6-solid:right-from-bracket" class="ml-auto icon-sm"/>
            </button>
          </li>
        </ul>
      </div>
    </div>
  </nav>
  <Register ref="register"/>
  <Login ref="login"/>
</template>

<script setup>
const user = useState('user', () => null);
const { $axios } = useNuxtApp();
const route = useRoute()
const toast = useToast();

const register = useTemplateRef('register');
const login = useTemplateRef('login');

async function logout(data) {
  try {
    await $axios.get('/user/logout', data);
    user.value = null;
    toast.add({
      title: 'Successfully logged out',
      color: 'success'
    });
  } catch (error) {
    handleAxiosError(error, toast);
  }
}
</script>