import { Component, Fragment } from 'react';
import {
    Link,
    BrowserRouter,
    Route,
    Switch,
    StaticRouter,
    Redirect,
} from 'react-router-dom';
import {ImageList} from "./imagelist.jsx";
import {ImageDetail} from "./imagedetail.jsx";

export default class ImageLatestComponent extends Component {
    render() {
        const app = (
            <div>
                <Switch>
                    <Route exact path="/" component={() => (
                        <ImageList initialImages={this.props.initialImages} page={this.props.page} imagesPerPage={this.props.imagesPerPage} browseUrl="Latest" />
                    )} />
                    <Route exact path="/Safe" component={() => (
                        <ImageList initialImages={this.props.initialImages} page={this.props.page} imagesPerPage={this.props.imagesPerPage} browseUrl="Safe" />
                    )} />
                    <Route path="/Image/:id" component={() => (
                        <ImageDetail initialImages={this.props.initialImages} page={this.props.page} />
                    )} />
                    <Route
                        path="*"
                        component={({ staticContext }) => {
                            if (staticContext) staticContext.status = 404;

                            return <h1>Not Found :(</h1>;
                        }}
                    />
                </Switch>
            </div>
        );

        if (typeof window === 'undefined') {
            return (
                <StaticRouter
                    context={this.props.context}
                    location={this.props.location}
                >
                    {app}
                </StaticRouter>
            );
        }
        return <BrowserRouter>{app}</BrowserRouter>;
    }
}