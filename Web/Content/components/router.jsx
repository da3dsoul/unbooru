import React, { Component} from 'react';
import {
    BrowserRouter,
    Route,
    Switch,
    StaticRouter,
} from 'react-router-dom';
import {ImageList} from "./imagelist.jsx";
import {ImageDetail} from "./imagedetail.jsx";
import {Search} from "./search.jsx";

export default class RouterComponent extends Component {
    render() {
        const app = (
            <div>
                <Switch>
                    <Route exact path="/" component={() => (
                        <ImageList browseUrl="Latest" />
                    )} />
                    <Route exact path="/Safe" component={() => (
                        <ImageList browseUrl="Safe" />
                    )} />
                    <Route path="/Image/:id" component={() => (
                        <ImageDetail />
                    )} />
                    <Route path="/Search" component={() => (
                        <Search  />
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