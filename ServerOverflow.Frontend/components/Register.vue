<template>
  <dialog id="register" class="modal modal-bottom sm:modal-middle">
    <div class="modal-box">
      <form method="dialog">
        <button class="btn btn-sm btn-circle btn-ghost absolute right-5 top-5">
          <Icon name="fa6-solid:xmark" size="2em"></Icon>
        </button>
      </form>
      <h3 class="text-lg font-bold">Register a new account</h3>
      <div class="divider divider-start m-0 mb-2"/>

      <FormKit type="form" submit-label="Register" :actions="false" @submit="submit">
        <FormKit type="text" name="username" label="Username" validation="required">
          <template #prefixIcon><Icon name="fa6-solid:user" class="mr-2"/></template>
        </FormKit>
        <FormKit type="password" name="password" label="Password" validation="required|length:6">
          <template #prefixIcon><Icon name="fa6-solid:key" class="mr-2"/></template>
        </FormKit>
        <FormKit type="password" name="inviteCode" label="Invitation code" help="A member has to invite you manually for you to register" validation="required">
          <template #prefixIcon><Icon name="fa6-solid:ticket" class="mr-2"/></template>
        </FormKit>
        <FormKit type="submit" label="Register" class="w-full" />
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
        const res = await this.$axios.post('/user/register', data);
        this.user.value = res.data;
        this.toast.add({
          title: `Successfully logged in as ${res.data.username}`,
          color: 'success'
        });
        register.close();
      } catch (error) {
        const res = error.response;
        if (res.data.title === 'Invalid invitation code')
          node.setErrors({ inviteCode: res.data.detail })
        else if (res.data.title === 'Invalid username specified')
          node.setErrors({ username: res.data.detail })
        else node.setErrors(res.data.detail)
      }
    }
  }
}
</script>