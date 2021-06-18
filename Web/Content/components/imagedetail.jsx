import React from 'react';
import { LazyLoadImage } from 'react-lazy-load-image-component';
import { useParams } from 'react-router-dom';

export function ImageDetail(props) {
    let { id } = useParams();

    return (
        <div id="main" className="container-fluid">
        </div>
    );
}