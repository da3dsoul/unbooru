import React, {useEffect} from 'react';
import ImageBox from "Content/components/imagebox";
import axios from "axios";
import ReactPaginate from "react-paginate";
import Stonemason from "@da3dsoul/react-stonemason";
import {useLocation} from "react-router-dom";

export default function Missing() {
    const imagesPerPage = 21;
    let query = useLocation().search.slice(1);
    let [state, updateState] = React.useState({
        images: [],
        page: 1,
        pages: 1
    });
    useEffect(() => window.scrollTo(0,0), [state.images]);

    useEffect(() => {
        axios.get('/api/Image/Missing/Count').then(res => {
            updateState(prevState => ({
                ...prevState,
                pages: Math.ceil(res.data / imagesPerPage)
            }));
        });

        axios.get(`/api/Image/Missing?limit=${imagesPerPage}`).then(res => {
            updateState(prevState => ({
                ...prevState,
                images: res.data
            }));
        })
    }, [query])

    function goToPage(num) {
        let pageNum = num.selected + 1;
        updateState(prevState => ({
            ...prevState,
            page: pageNum
        }));

        let url = `/api/Image/Missing?limit=${imagesPerPage}&offset=${(pageNum-1)*imagesPerPage}`;
        axios.get(url).then(res => {
            updateState(prevState => ({
                ...prevState,
                images: res.data
            }));
        })
    }

    let imageNodes = state.images.map(image => {
        if (!image.hasOwnProperty("imageId")) return;
        return (<ImageBox id={image.imageId} key={image.imageId} image={image} width={image.width} height={(image.height + 120)} />);
    });

    function getHeight(containerWidth) {
        return Math.floor(window.outerHeight * 0.99);
    }

    return (
        <div id="main" className="container-fluid">
            <Stonemason targetRowHeight={getHeight}>
                {imageNodes}
            </Stonemason>
            <ReactPaginate
                previousLabel={"<"}
                nextLabel={">"}
                breakLabel={"•••"}
                breakClassName={"break-me"}
                pageCount={state.pages}
                marginPagesDisplayed={1}
                pageRangeDisplayed={5}
                onPageChange={goToPage}
                containerClassName={"pagination"}
                subContainerClassName={"pages pagination"}
                activeClassName={"active"}/>
        </div>
    );
}
