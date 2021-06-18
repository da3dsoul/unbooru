import React from "react";
import PropTypes from "prop-types";
import {isPixivSource} from "../modelutils";
import {LazyLoadImage} from "react-lazy-load-image-component";

export default class ImageBox extends React.Component {
    static propTypes = {
        image: PropTypes.object.isRequired,
    };

    render() {
        const image = this.props.image;
        const source = image.sources.find(isPixivSource);
        const artist = image.artistAccounts[0];
        let containerClass = image.width / image.height < 1.2 ? "image-container-portrait" : "image-container-landscape";
        return (
            <li className={containerClass}>
                <div className="image-container-child">
                    <LazyLoadImage wrapperClassName="image" src={"/api/Image/" + image.imageId + "/" + source.originalFilename} alt={source.title} />
                    <div className="info-container">
                        <a target="_blank" rel="noopener noreferrer" className="title-text" href={source.postUrl}>{source.title}</a>
                        <a target="_blank" rel="noopener noreferrer" className="artist-text" href={artist.url}>{artist.name}</a>
                    </div>
                </div>
            </li>
        );
    }
}