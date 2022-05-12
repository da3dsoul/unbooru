import React, {useEffect, useState } from "react";
import ReactPaginate from "react-paginate";
import axios from "axios";
import queryString from "query-string";
import ImageBox from "Content/components/imagebox.js";
import Stonemason from "@da3dsoul/react-stonemason";

export default function Search(properties) {
    const imagesPerPage = 21;
    const location = properties.location;
    
    let query = location.slice(location.indexOf('?') + 1);
    const params = queryString.parse(query);
    let page = params.page;
    if (typeof page === 'undefined' || page === null) page = 1;
    else {
        const pageQuery = 'page=' + page;
        query = query.replace('&'+pageQuery, '').replace('?'+pageQuery, '');
    }
    
    let [state, updateState] = useState({
        images: [],
        page: page,
        pages: 1
    });

    useEffect(() => window.scrollTo(0,0), [state.images]);

    useEffect(() => {
        axios.get('/api/Search/Count' + (query === '' ? '' : '?' + query)).then(res => {
            updateState(prevState => ({
                ...prevState,
                pages: Math.ceil(res.data / imagesPerPage)
            }));
        });
    }, [query])

    function goToPage(num) {
        let pageNum = num.selected + 1;
        updateState(prevState => ({
            ...prevState,
            page: pageNum
        }));

        let url = `/api/Search?limit=${imagesPerPage}&offset=${(pageNum-1)*imagesPerPage}&${query}`;
        axios.get(url).then(res => {
            updateState(prevState => ({
                ...prevState,
                images: res.data
            }));
        })
    }

    let imageNodes = state.images.map(image => {
        if (!image.hasOwnProperty("imageId")) return;
        return (<ImageBox id={image.imageId} key={image.imageId} image={image} width={image.width} height={(image.height)} />);
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
                initialPage={page-1}
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
