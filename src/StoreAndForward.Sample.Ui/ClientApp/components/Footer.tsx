import * as React from 'react';
import { Settings } from '../appSettings';
export class FooterProps {

}

export class Footer extends React.Component<FooterProps, {}> {

    hangfireEndpoint: string = Settings.endpoint + '/hangfire';
    swaggerEndpoint: string = Settings.endpoint + '/swagger';
    version: string = Settings.version;

    public render() {
        return <div>
            <hr />

            <span className="text-center">ADHA Store and Forward v{this.version}</span>

            <div className="pull-right">
                <a target="_blank" href={this.swaggerEndpoint}>Swagger</a>&nbsp;&nbsp;&nbsp;<a target="_blank" href={this.hangfireEndpoint}>Hangfire</a>
            </div>
        </div>;
    }
}
