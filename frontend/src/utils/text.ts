export function middleEllipsis(value: string, start = 70, end = 25) {
    if (!value) return value;
    if (value.length <= start + end + 3) return value;
    return `${value.slice(0, start)}...${value.slice(value.length - end)}`;
}
