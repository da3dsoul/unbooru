import React from 'react';
import * as Reactstrap from 'reactstrap';
import ImageBox from "./imagebox.js";

export function ImageList(props) {
    let [state, updateState] = React.useState({
        images: props.initialImages,
        page: props.page,
        hasMore: true,
        loadingMore: false,
    });

    function previousClicked(evt) {
        let prevPage = (state.page === 1 ? 1 : state.page - 1);
        updateState(prevState => ({
            ...prevState,
            page: prevPage,
            loadingMore: true,
        }));

        const imagesPerPage = props.imagesPerPage;
        let offset = (prevPage - 1) * imagesPerPage;
        if (offset < 0) offset = 0;
        let url = '/api/Image/' + props.browseUrl + '?limit=' + imagesPerPage + '&offset=' + offset;
        let xhr = new XMLHttpRequest();
        xhr.open('GET', url, true);
        xhr.setRequestHeader('Content-Type', 'application/json');

        xhr.onload = () => {
            let data = JSON.parse(xhr.responseText);
            updateState(prevState => ({
                ...prevState,
                images: data,
                hasMore: (data.length === imagesPerPage),
                loadingMore: false,
            }));
        };
        xhr.send();
        evt.preventDefault();
    }

    function nextClicked(evt) {
        let nextPage = state.page + 1;
        updateState(prevState => ({
            ...prevState,
            page: nextPage,
            loadingMore: true,
        }));

        const imagesPerPage = props.imagesPerPage;
        let url = '/api/Image/' + props.browseUrl + '?limit=' + imagesPerPage + '&offset=' + ((nextPage - 1) * imagesPerPage);
        let xhr = new XMLHttpRequest();
        xhr.open('GET', url, true);
        xhr.setRequestHeader('Content-Type', 'application/json');

        xhr.onload = () => {
            let data = JSON.parse(xhr.responseText);
            updateState(prevState => ({
                ...prevState,
                images: data,
                hasMore: (data.length === imagesPerPage),
                loadingMore: false,
            }));
        };
        xhr.send();
        evt.preventDefault();
    }

    let imageNodes = state.images.map(image => (
        <ImageBox image={image} />
    ));

    function renderMoreLink() {
        let previous = (
            <Reactstrap.Button onClick={previousClicked}>
                Previous
            </Reactstrap.Button>
        );
        let next = (
            <Reactstrap.Button onClick={nextClicked}>
                Next
            </Reactstrap.Button>
        );
        if (state.loadingMore) {
            return <em>Loading...</em>;
        } else if (state.hasMore && state.page > 1) {
            return (
                <div>
                    {previous}
                    {next}
                </div>
            );
        } else if (state.hasMore) {
            return next;
        } else if (state.page > 1) {
            return previous;
        } else {
            return (<em>No more images...</em>);
        }
    }

    return (
        <div id="main" className="container-fluid">
            <ol id="image-list" className="image-list">{imageNodes}</ol>
            {renderMoreLink()}
        </div>
    );
}