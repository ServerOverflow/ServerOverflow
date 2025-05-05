export default function (name, replacement) {
    if (name === null || name === '')
        return replacement;
    return name;
}