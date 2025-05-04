export default function (permission) {
    const user = useState('user', () => null)
    return user.value?.permissions?.includes(permission) || false;
}