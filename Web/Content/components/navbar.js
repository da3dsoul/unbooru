import React, { Component} from 'react';
import { Link } from 'react-router-dom';

export default class Nav extends Component {
    render() {
        return (
            <nav className="Nav">
                <div className="Nav__container">
                    <Link to="/" className="Nav__brand">
                        <img src="/favicon.png" className="Nav__logo"  alt=""/>
                    </Link>

                    <div className="Nav__right">
                        <div className="Nav__item-wrapper">
                            <div className="Nav__item">
                                <Link className="Nav__link" to="/Search">Search</Link>
                            </div>
                            <div className="Nav__item">
                                <Link className="Nav__link" to="/Artists">Artists</Link>
                            </div>
                            <div className="Nav__item">
                                <Link className="Nav__link" to="/Tags">Tags</Link>
                            </div>
                            <div className="Nav__item">
                                <Link className="Nav__link" to="/Random">Random</Link>
                            </div>
                        </div>
                    </div>
                </div>
            </nav>
        );
    }
}
