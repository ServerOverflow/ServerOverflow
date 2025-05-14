export default function (permission) {
    const user = useState('user', () => null)
    return user.value?.permissions?.includes(permission) || user.value?.permissions?.includes('Administrator') || false;
}