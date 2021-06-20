import React, { Component} from 'react';
import {
    BrowserRouter,
    Route,
    Switch,
    StaticRouter,
} from 'react-router-dom';
import SafeImageList from "./safe.jsx";
import ImageDetail from "./imagedetail.jsx";
import Search from "./search.jsx";
import NotFound from "./notfound.jsx";

export default class RouterComponent extends Component {
    render() {
        const app = (
            <div>
                <Switch>
                    <Route exact path="/" component={() => (
                        <SafeImageList />
                    )} />
                    <Route path="/Image/:id" component={() => (
                        <ImageDetail />
                    )} />
                    <Route path="/Search" component={() => (
                        <Search  />
                    )} />
                    <Route path="*" component={({staticContext}) => {
                        if (staticContext) staticContext.status = 404;
                        return <NotFound />
                    }}/>
                </Switch>
            </div>
        );

        if (typeof window === 'undefined') {
            return (
                <StaticRouter context={this.props.context} location={this.props.location}>
                    {app}
                </StaticRouter>
            );
        }
        return <BrowserRouter>{app}</BrowserRouter>;
    }
}