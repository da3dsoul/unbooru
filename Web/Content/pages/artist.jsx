import React, {useEffect} from 'react';
import ImageBox from "Content/components/imagebox";
import axios from "axios";
import ReactPaginate from "react-paginate";
import Stonemason from "@da3dsoul/react-stonemason";
import ArtistBox from "Content/components/artistbox";

export default function ArtistList() {
    const imagesPerPage = 21;
    let [state, updateState] = React.useState({
        artists: [],
        page: 1,
        pages: 1
    });
    useEffect(() => window.scrollTo(0,0), [state.artists]);

    useEffect(() => {
        axios.get('/api/Artist/Count').then(res => {
            updateState(prevState => ({
                ...prevState,
                pages: Math.ceil(res.data / imagesPerPage)
            }));
        }).catch(() => {});

        axios.get(`/api/Artist?limit=${imagesPerPage}`).then(res => {
            updateState(prevState => ({
                ...prevState,
                artists: res.data
            }));
        }).catch(() => {});
    }, [])

    function goToPage(num) {
        let pageNum = num.selected + 1;
        updateState(prevState => ({
            ...prevState,
            page: pageNum
        }));

        let url = `/api/Artist?limit=${imagesPerPage}&offset=${(pageNum-1)*imagesPerPage}`;
        axios.get(url).then(res => {
            updateState(prevState => ({
                ...prevState,
                artists: res.data
            }));
        }).catch(() => {});
    }

    let artistNodes = state.artists?.map(artist => {
        if (!artist.hasOwnProperty("artistAccountId")) return;
        return (<ArtistBox id={artist.artistAccountId} key={artist.artistAccountId} artist={artist} />);
    });

    return (
        <div id="main" className="container-fluid">
            <div style={{display: "flex", flexDirection: "row", flexWrap: "wrap"}}>
                {artistNodes}
            </div>
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
