import React, { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import axios from "axios";
import {isPixivSource} from "../modelutils";

export default function ImageDetail(props) {
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
    
    const pixiv = state.sources.find(isPixivSource);

    return (
        <div id="main" className="container-fluid" style={{width:"100%", height:"100%"}}>
            <img src={"/api/Image/"+state.image.imageId+"/"+pixiv.originalFilename} alt={pixiv.title} />
        </div>
    );
}