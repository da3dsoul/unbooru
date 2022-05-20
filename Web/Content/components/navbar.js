import React from 'react';
import SearchInput from './searchbox/searchbox'

export default function Nav() {
    return (
        <nav className="Nav">
            <div className="Nav__container">
                <a href="/" className="Nav__brand">
                    <img src="/favicon.png" className="Nav__logo"  alt=""/>
                </a>

                <div className="Nav__right">
                    <div className="Nav__item-wrapper">
                        <SearchInput placeholder="find a public api" />
                        <div className="Nav__item">
                            <a className="Nav__link" href="/Artist">Artists</a>
                        </div>
                        <div className="Nav__item">
                            <a className="Nav__link" href="/Tag">Tags</a>
                        </div>
                        <div className="Nav__item">
                            <a className="Nav__link" href="/Random">Random</a>
                        </div>
                    </div>
                </div>
            </div>
        </nav>
    );
}
