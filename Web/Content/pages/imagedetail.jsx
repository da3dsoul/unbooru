import React, {useEffect, useState} from 'react';
import axios from "axios";
import ReactHtmlParser from 'react-html-parser'
import {isPixivSource} from "Content/modelutils";
import Related from "Content/components/relatedbox";

function ArtistLink(props) {
    const {artist} = props
    let icon = '/static/unknown.png';
    if (artist.url.includes('pixiv'))
        icon = '/static/pixiv.png';
    else if (artist.url.includes('twitter'))
        icon = '/static/twitter.png'

    return (
<div key={artist.artistAccountId} className="artist-item-container">
    <a target="_blank" rel="noopener noreferrer" href={artist.url}><img className="artist-icon" alt="icon" src={icon} /></a>
</div>
    )
}

export default function ImageDetail(properties) {
    let [state, updateState] = useState({
        image: null
    });

    useEffect(() => {
        const location = properties.location;
        const id = location.split('/').slice(-1);
        axios.get('/api/Image/' + id).then(res => {
            updateState(prevState => ({
                ...prevState,
                image: res.data
            }));
        });
    }, [])

    if (state.image?.sources === null || "undefined" ===  typeof state.image?.sources) {
        return <div id="main" className="container-fluid" style={{width: "100%", height: "100%"}} />
    }

    const pixiv = state.image.sources.find(isPixivSource);
    const artistInfo = state.image.artistAccounts.map(aa => <ArtistLink key={aa.artistAccountId} artist={aa} />);
    const tags = state.image.tagSources.filter((item, pos, self) => {
        return self.findIndex(v => v.tag.name === item.tag.name) === pos;
    }).map(tagSource => {
        let color = "white";
        if (tagSource.tag.type === 'Character') color = "cyan";
        else if (tagSource.tag.type === 'Copyright') color = "pink"
        let append = "";
        if (tagSource.source === 'DeepDanbooruModule') append = "*"
        return (<a key={tagSource.tag.imageTagId.toString()} target="_blank" rel="noopener noreferrer" style={{color: color}}
                   href={`/Search?TagID=${tagSource.tag.imageTagId}`}>{tagSource.tag.name + append}</a>)
    });

    const description = pixiv.description !== null && pixiv.description.length > 0 ?
        <div className="post-description">{ReactHtmlParser(pixiv.description)}</div> : <br/>;

    return (
        <div id="main" className="container-fluid" style={{width:"auto", height:"auto"}}>
            <div className="image-detail">
                <div className="info-container">
                    <div className="image-info">
                        <div className="post-title">
                            <a className="title-text" href={pixiv.postUrl} target="_blank" rel="noopener noreferrer">{pixiv.title}</a>
                            <span className="post-date">{pixiv.postDate}</span>
                        </div>
                        {description}
                    </div>
                    <div className="artist-container">
                        <a className="artist-info" href={`/Search?ArtistAccountID=${state.image.artistAccounts[0].artistAccountId}`} target="_blank" rel="noopener noreferrer">
                            <img src={`/api/Artist/${state.image.artistAccounts[0].artistAccountId}/Avatar`} alt={state.image.artistAccounts[0].name} className="artist-avatar" />
                            <span className="artist-text">{state.image.artistAccounts[0].name}</span>
                        </a>
                        <div className="artist-links">
                            {artistInfo}
                        </div>
                    </div>
                    <div className="tag-container">
                        {tags}
                    </div>
                    <Related ids={state.image.relatedImageIds} />
                </div>
                <div className="image-detail-image">
                    <img src={`/api/Image/${state.image.imageId}/${pixiv.originalFilename}`} alt={pixiv.title} />
                </div>
            </div>
        </div>
    );
}
