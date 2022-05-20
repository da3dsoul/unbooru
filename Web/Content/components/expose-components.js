import React, {useState, useEffect} from 'react';
import ReactDOM from 'react-dom';
import ReactDOMServer from 'react-dom/server';

import Nav from 'Content/components/navbar';
import Search from 'Content/pages/search.jsx';
import ArtistList from "Content/pages/artist.jsx";
import ImageDetail from 'Content/pages/imagedetail.jsx';
import Missing from "Content/pages/missing.jsx";
import NotFound from "Content/pages/notfound.jsx";
import Random from "Content/pages/random.jsx";
import { ServerStyleSheet } from 'styled-components';
import { JssProvider, SheetsRegistry } from 'react-jss';
import { renderStylesToString } from 'emotion-server';
import Helmet from 'react-helmet';


function getWindowDimensions() {
    const { innerWidth: width, innerHeight: height } = window;
    return {
        width,
        height
    };
}

export function useWindowDimensions() {
    const [windowDimensions, setWindowDimensions] = useState(getWindowDimensions());

    useEffect(() => {
        function handleResize() {
            setWindowDimensions(getWindowDimensions());
        }

        window.addEventListener('resize', handleResize);
        return () => window.removeEventListener('resize', handleResize);
    }, []);

    return windowDimensions;
}

global.React = React;
global.ReactDOM = ReactDOM;
global.ReactDOMServer = ReactDOMServer;

global.Styled = { ServerStyleSheet };
global.ReactJss = { JssProvider, SheetsRegistry };
global.EmotionServer = { renderStylesToString };
global.Helmet = Helmet;

global.Components = { Nav, Search, ArtistList, ImageDetail, Missing, NotFound, Random };
