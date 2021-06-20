import React from "react";
import {isPixivSource} from "../modelutils";
import {LazyLoadImage} from "react-lazy-load-image-component";

export default function ImageBox(props) {
    const image = props.image;
    const source = image.sources.find(isPixivSource);
    const artist = image.artistAccounts[0];
    const ar = image.width / image.height;
    const containerClass = ar < 1.1 ? "image-container-portrait" : "image-container-landscape";

    return (
        <li className={containerClass}>
            <div className="image-container-child">
                <div className="image">
                    <LazyLoadImage src={"/api/Image/"+image.imageId+"/"+source.originalFilename} alt={source.title} />
                </div>
                <div className="info-container">
                    <a target="_blank" rel="noopener noreferrer" className="title-text" href={source.postUrl}>{source.title}</a>
                    <a target="_blank" rel="noopener noreferrer" className="artist-text" href={artist.url}>{artist.name}</a>
                </div>
            </div>
        </li>
    );
}