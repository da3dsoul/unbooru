import React, {useEffect, useState} from "react";
import {imagePropType, isPixivSource} from "../modelutils";
import LazyLoad from "react-lazy-load";
import axios from "axios";

 const ImageBox = function ImageBox(props) {
    const image = props.image;
     let [state, updateState] = useState({
         source: null,
         artist: null
     });

     useEffect(() => {
             axios.get('/api/Image/' + image.imageId).then(res => {
                 updateState(prevState => ({
                     source: res.data.sources.find(isPixivSource),
                     artist: res.data.artistAccounts[0]
                 }));
             });
         }, [image.imageId]
     )

    return (
        <div className="image-container-child" style={props.style}>
            <div className="image">
                {(state.source !== null) ?
                    <LazyLoad width="100%" height="100%">
                        <img src={"/api/Image/"+image.imageId+"/"+state.source.originalFilename} alt={state.source.title} />
                    </LazyLoad>
                    :
                    <div style={{width: '100%', height: '100%'}}/>}
            </div>
            {(state.source !== null && state.artist !== null) ?
                <div className="info-container">
                    <a target="_blank" rel="noopener noreferrer" className="title-text" href={state.source.postUrl}>{state.source.title}</a>
                    <a target="_blank" rel="noopener noreferrer" className="artist-text" href={state.artist.url}>{state.artist.name}</a>
                </div>
                :
                <div className="info-container"/>}
        </div>
    );
};

ImageBox.propTypes = {
    image: imagePropType
};

export default ImageBox;
