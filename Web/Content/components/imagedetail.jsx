import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import axios from "axios";
import {isPixivSource} from "../modelutils";

function ArtistLink(props) {
    const {artist} = props
    let icon = '/static/unknown.png';
    if (artist.url.includes('pixiv'))
        icon = '/static/pixiv.png';
    else if (artist.url.includes('twitter'))
        icon = '/static/twitter.png'

    return (
<div key={artist.artistAccountId} className="artist-item-container">
    <img className="artist-icon" alt="icon" src={icon} />
    <a className="artist-text" target="_blank" rel="noopener noreferrer" href={artist.url}>{artist.name}</a>
</div>
    )
}

export default function ImageDetail() {
    let { id } = useParams();
    let [state, updateState] = useState({
        image: null,
        id: id
    });

    useEffect(() => {
        axios.get('/api/Image/' + id).then(res => {
            updateState(prevState => ({
                ...prevState,
                image: res.data
            }));
        });
    }, [id])

    if (state.image?.sources === null || "undefined" ===  typeof state.image?.sources) {
        return <div id="main" className="container-fluid" style={{width: "100%", height: "100%"}}></div>
    }

    const pixiv = state.image.sources.find(isPixivSource);
    const artistInfo = state.image.artistAccounts.map(aa => <ArtistLink key={aa.artistAccountId} artist={aa} />);
    const tags = state.image.tags.map(tag => (<a key={tag.imageTagId.toString()} target="_blank" rel="noopener noreferrer" href={"/Search?tagid=" + tag.imageTagId}>{tag.name}</a> ));

    return (
        <div id="main" className="container-fluid" style={{width:"auto", height:"auto"}}>
            <div className="image-detail">
                <div className="info-container">
                    <div className="artist-container">
                        {artistInfo}
                    </div>
                    <div className="tag-container">
                        {tags}
                    </div>
                </div>
                <img className="image-detail-image" src={"/api/Image/"+state.image.imageId+"/"+pixiv.originalFilename} alt={pixiv.title} />
            </div>
        </div>
    );
}
