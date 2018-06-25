import * as React from 'react';
import * as moment from 'moment';


export class DocumentStatusProps {
    dateTime : string;
}

export class DateTimeDisplay extends React.Component<DocumentStatusProps, {}> {

    dateTime : string = this.props.dateTime;

    public render() {
        return <div>
            {moment(this.dateTime).format('DD-MMM-YYYY h:mm:ss a')}

            <div>
                <sub>{moment(this.dateTime).fromNow()}</sub>
            </div>
        </div>;
    }
}
