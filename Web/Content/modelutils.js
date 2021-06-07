export function isPixivSource(o) {
    return (typeof (o) !== 'undefined' && o !== null && typeof (o.source) !== 'undefined' && o.source !== null && (o.source === 'Pixiv' || o.source === 'pixiv'));
}