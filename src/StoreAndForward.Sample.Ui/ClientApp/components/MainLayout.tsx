import * as React from 'react';
import { Link } from 'react-router-dom';
import { Footer } from './Footer';

export class MainLayout extends React.Component<{}, {}> {
    public render() {
        return <div>

            <nav className="navbar navbar-expand-md bg-light navbar-light">
                <div className="container">
                    <a className="navbar-brand" href="/">
                        <i className="fa d-inline fa-lg fa-cloud"></i> Store And Forward</a>
                    <button className="navbar-toggler" type="button" data-toggle="collapse" data-target="#navbar2SupportedContent"
                        aria-controls="navbar2SupportedContent" aria-expanded="false" aria-label="Toggle navigation">
                        <span className="navbar-toggler-icon"></span>
                    </button>
                    <div className="collapse navbar-collapse text-center justify-content-end" id="navbar2SupportedContent">
                        <Link to={'/'}> <i className="fa d-inline fa-file-text-o"></i> Document Queue</Link>&nbsp;&nbsp;&nbsp;<Link to={'/timeline'}> <i className="fa d-inline fa-clock-o"></i> Timeline</Link>
                    </div>
                </div>
            </nav>

            <div id="content">
                <br />
                <div className="container">
                    {this.props.children}
                </div>
            </div>

            <div className="container">
                <div className="row">
                    <div className="col">

                        <Footer></Footer>

                    </div>
                </div>
            </div>
            <br />
        </div>;
    }
}
