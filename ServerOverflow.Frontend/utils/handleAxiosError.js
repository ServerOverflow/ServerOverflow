function showNotification(instance, message, target) {
    if (!('add' in instance)) {
        if (target) instance.setErrors({ [target]: message })
        else instance.setErrors(message);
        return;
    }

    instance.add({
        title: message,
        color: 'error'
    })
}

export default function (error, instance) {
    const res = error.response;
    if (res) {
        if (res.data.title === 'Invalid invitation code')
            showNotification(instance, res.data.detail, "code");
        else if (res.data.title === 'Invalid username specified')
            showNotification(instance, res.data.detail, "username");
        else showNotification(instance, res.data.detail, null);
        return;
    }

    if (["ERR_NETWORK", "ECONNABORTED", "ETIMEDOUT"].includes(error.code))
        showNotification(instance, 'The backend is down, please bug TheAirBlow', null);
    else showNotification(instance, `Failed to send request: ${error.message} (${error.code})`, null);
}