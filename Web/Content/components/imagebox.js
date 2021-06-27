import React from "react";
import {isPixivSource} from "../modelutils";
import LazyLoad from "react-lazy-load";

export default function ImageBox(props) {
    const image = props.image;
    const source = image.sources.find(isPixivSource);
    const artist = image.artistAccounts[0];

    return (
        <div className="image-container-child" style={props.style}>
            <div className="image">
                <LazyLoad width="100%">
                    <img src={"/api/Image/"+image.imageId+"/"+source.originalFilename} alt={source.title} />
                </LazyLoad>
            </div>
            <div className="info-container">
                <a target="_blank" rel="noopener noreferrer" className="title-text" href={source.postUrl}>{source.title}</a>
                <a target="_blank" rel="noopener noreferrer" className="artist-text" href={artist.url}>{artist.name}</a>
            </div>
        </div>
    );
}