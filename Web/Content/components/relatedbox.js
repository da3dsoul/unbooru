import React, {useEffect, useState} from 'react';
import axios from "axios";

import ImageBox from "Content/components/imagebox";

export default function Related(props) {
    if (props.ids == null || props.ids.length === 0) return (<div/>)

    let [state, updateState] = useState({
        images: Object.assign({}, ...props.ids.map((x) => ({[x]: null})))
    });
    useEffect(() => {
        props.ids.map(id => {
            axios.get('/api/Image/' + id).then(res => {
                updateState(prevState => {
                    let newImages = prevState.images;
                    newImages[id.toString()] = res.data;
                    return ({
                        ...prevState,
                        images: newImages
                    })
                });
            });
        });
    }, [])

    const imageNodes = Object.values(state.images).map(image => {
        if (image == null || !image.hasOwnProperty("imageId")) return;
        return (<ImageBox id={image.imageId} key={image.imageId} image={image} width={image.width} height={(image.height)} />);
    });

    return (
        <div className="Related">
            <span style={{textAlign: "center", fontSize: "1.5rem", fontWeight: 400, padding: "0.25em"}}>Related Images</span>
            <div className="Related__container">
                {imageNodes}
            </div>
        </div>
    );
}
