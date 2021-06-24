import React, {useEffect} from 'react';
import ReactPaginate from 'react-paginate';
import {useLocation} from "react-router-dom";
import axios from "axios";
import ImageBox from './imagebox.js'
import Masonry, {ResponsiveMasonry} from "react-responsive-masonry";

export default function Search() {
    const imagesPerPage = 21;
    let query = useLocation().search.slice(1)
    let [state, updateState] = React.useState({
        images: [],
        page: 1,
        pages: 1
    });

    useEffect(() => window.scrollTo(0,0), [state.images]);

    useEffect(() => {
        axios.get('/api/Search/Count?' + query).then(res => {
            updateState(prevState => ({
                ...prevState,
                pages: Math.ceil(res.data / imagesPerPage)
            }));
        });

        axios.get(`/api/Search?limit=${imagesPerPage}&${query}`).then(res => {
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
        return (<ImageBox id={image.imageId} key={image.imageId} image={image} />);
    });

    return (
        <div id="main" className="container-fluid">
            <ResponsiveMasonry id="image-list" columnsCountBreakPoints={{600: 1, 1000: 2, 1400: 3}}>
                <Masonry>
                    {imageNodes}
                </Masonry>
            </ResponsiveMasonry>
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