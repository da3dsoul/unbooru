import PropTypes from 'prop-types';

export function isPixivSource(o) {
    return (typeof (o) !== 'undefined' && o !== null && typeof (o.source) !== 'undefined' && o.source !== null && (o.source === 'Pixiv' || o.source === 'pixiv'));
}

export const sourcePropType = PropTypes.shape({
    imageSourceId: PropTypes.number.isRequired,
    source: PropTypes.string,
    originalFilename: PropTypes.string,
    uri: PropTypes.string.isRequired,
    postUrl: PropTypes.string.isRequired,
    title: PropTypes.string,
    description: PropTypes.string
});

export const artistPropType = PropTypes.shape({
    artistAccountId: PropTypes.number.isRequired,
    id: PropTypes.string.isRequired,
    name: PropTypes.string.isRequired,
    url: PropTypes.string.isRequired
});

export const tagPropType = PropTypes.shape({
    imageTagId: PropTypes.number.isRequired,
    name: PropTypes.string.isRequired,
    description: PropTypes.string,
    type: PropTypes.string,
    safety: PropTypes.string
})

export const imagePropType = PropTypes.shape({
    imageId: PropTypes.number.isRequired,
    width: PropTypes.number.isRequired,
    height: PropTypes.number.isRequired,
    sources: PropTypes.arrayOf(sourcePropType),
    artistAccounts: PropTypes.arrayOf(artistPropType),
    tags: PropTypes.arrayOf(tagPropType)
});