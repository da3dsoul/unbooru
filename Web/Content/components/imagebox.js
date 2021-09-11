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
    <div className="image" style={props.style}>
        {state.source !== null ?
            <a href={"/Image/" + image.imageId} target="_blank" rel="noopener noreferrer" style={{width:'100%', height:'100%'}}>
                <LazyLoad width="100%" height="100%">
                    <img src={"/api/Image/"+image.imageId+"/"+state.source.originalFilename} alt={state.source.title} />
                </LazyLoad>
            </a>
            :
            <div style={{width: '100%', height: '100%'}}/>}
    </div>
    );
};

ImageBox.propTypes = {
    image: imagePropType
};

export default ImageBox;
