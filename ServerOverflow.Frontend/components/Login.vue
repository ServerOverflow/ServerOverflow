<template>
  <dialog id="login" class="modal modal-bottom sm:modal-middle">
    <div class="modal-box">
      <form method="dialog">
        <button class="btn btn-sm btn-circle btn-ghost absolute right-5 top-5">
          <Icon name="fa6-solid:xmark" size="2em"></Icon>
        </button>
      </form>
      <h3 class="text-lg font-bold">Log into your account</h3>
      <div class="divider divider-start m-0 mb-2"/>

      <FormKit type="form" submit-label="Login" :actions="false" @submit="submit">
        <FormKit type="text" name="username" label="Username" validation="required">
          <template #prefixIcon><Icon name="fa6-solid:user" class="mr-2"/></template>
        </FormKit>
        <FormKit type="password" name="password" label="Password" validation="required">
          <template #prefixIcon><Icon name="fa6-solid:key" class="mr-2"/></template>
        </FormKit>
        <FormKit type="submit" label="Login" class="w-full" />
      </FormKit>
    </div>
    <form method="dialog" class="modal-backdrop">
      <button>close</button>
    </form>
  </dialog>
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
    async submit(data, node) {
      try {
        const res = await this.$axios.post('/user/login', data);
        this.user.value = res.data;
        this.toast.add({
          title: `Successfully logged in as ${res.data.username}`,
          color: 'success'
        });
        login.close();
      } catch (error) {
        const res = error.response;
        node.setErrors(res.data.detail)
      }
    }
  }
}
</script>