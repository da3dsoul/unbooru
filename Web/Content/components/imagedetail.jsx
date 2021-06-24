import React from 'react';
import { useParams } from 'react-router-dom';

export default function ImageDetail(props) {
    let { id } = useParams();

    return (
        <div id="main" className="container-fluid">
        </div>
    );
}