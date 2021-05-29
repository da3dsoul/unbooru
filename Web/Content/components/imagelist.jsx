import React from 'react';
import PropTypes from 'prop-types';
import * as Reactstrap from 'reactstrap';
import { LazyLoadImage } from 'react-lazy-load-image-component';

export function ImageList(props) {
    let [state, updateState] = React.useState({
        images: props.initialImages,
        page: props.page,
        hasMore: true,
        loadingMore: false,
    });

    function previousClicked(evt) {
        let prevPage = (state.page === 1 ? 1 : state.page - 1);
        let images = state.images;
        updateState(prevState => ({
            ...prevState,
            page: prevPage,
            loadingMore: true,
        }));

        let offset = (state.page - 1) * 20;
        if (offset < 0) offset = 0;
        let url = '/Image/Latest?limit=20&offset=' + offset;
        let xhr = new XMLHttpRequest();
        xhr.open('GET', url, true);
        xhr.setRequestHeader('Content-Type', 'application/json');

        xhr.onload = () => {
            let data = JSON.parse(xhr.responseText);
            updateState(prevState => ({
                ...prevState,
                images: data,
                hasMore: (data.length === 20),
                loadingMore: false,
            }));
        };
        xhr.send();
        evt.preventDefault();
    }

    function nextClicked(evt) {
        let nextPage = state.page + 1;
        let images = state.images;
        updateState(prevState => ({
            ...prevState,
            page: nextPage,
            loadingMore: true,
        }));

        let url = '/Image/Latest?limit=20&offset=' + ((state.page - 1) * 20);
        let xhr = new XMLHttpRequest();
        xhr.open('GET', url, true);
        xhr.setRequestHeader('Content-Type', 'application/json');

        xhr.onload = () => {
            let data = JSON.parse(xhr.responseText);
            updateState(prevState => ({
                ...prevState,
                images: data,
                hasMore: (data.length === 20),
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
            <ol id="image-list" className="d-flex flex-wrap">{imageNodes}</ol>
            {renderMoreLink()}
        </div>
    );
}

class ImageBox extends React.Component {
    static propTypes = {
        image: PropTypes.object.isRequired,
    };

    render() {
        return (
            <li key={this.props.image.imageId.toString()} className="list-group-item">
                <h3>{this.props.image.sources[0].title}</h3>
                <h4>By: {this.props.image.artistAccounts[0].name}</h4>
                <LazyLoadImage src={"/Image/" + this.props.image.imageId + "/file.jpg"}
                     alt={this.props.image.sources[0].title}
                     width={350} style={{"maxHeight": 650 + "pt"}}
                />
            </li>
        );
    }
}