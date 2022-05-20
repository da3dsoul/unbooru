import React, {useEffect, useState} from "react";
import {imagePropType, isPixivSource} from "Content/modelutils";
import LazyLoad from "react-lazy-load";
import axios from "axios";

 const ArtistBox = function ArtistBox(props) {
    const artist = props.artist;
     let [state, updateState] = useState({
         source: null,
         image: null,
     });

     useEffect(() => {
         axios.get(`api/Artist/${artist.artistAccountId}/LatestImage`).then(res => {
             updateState(() => ({
                 source: res.data.sources.find(isPixivSource),
                 image: res.data
             }));
         }).catch(() => {});
     }, [artist.artistAccountId]
     )

    return (
        state.source !== null && state.image !== null ?
            <a style={{...props.style, maxWidth: 400, margin: '0.5rem', border: "1px solid #505060", borderRadius: "3px"}} href={"/Search?ArtistAccountId=" + artist.artistAccountId} target="_blank" rel="noopener noreferrer">
                <div style={{display: "flex", flexDirection: "column", width: "100%", height: "100%"}}>
                    <LazyLoad className="image" width="100%" height="auto">
                        <img src={"/api/Image/"+state.image.imageId+"/"+state.source.originalFilename} alt={state.source.title} />
                    </LazyLoad>
                    <div className="artist-container" style={{marginTop: "auto", border: "none"}}>
                        <div className="artist-info">
                            <img src={`/api/Artist/${artist.artistAccountId}/Avatar`} alt={artist.name} className="artist-avatar" />
                            <span className="artist-text">{artist.name}</span>
                        </div>
                    </div>
                </div>
            </a>
            :
            <div />
    );
};

ArtistBox.propTypes = {
    image: imagePropType
};

export default ArtistBox;
