import * as React from 'react';

export class EventTypeProps {
    eventType : string;
}

export class EventType extends React.Component<EventTypeProps, {}> {

    eventType: string = this.props.eventType;

    public render() {
        return <div>
            {this.eventType === "Success" && <div><i className="fa fa-circle fa-lg text-success"></i> Success</div>}
            {this.eventType === "Deferred" && <div><i className="fa fa-circle fa-lg text-secondary"></i> Deferred</div>}
            {this.eventType === "Created" && <div><i className="fa fa-circle fa-lg text-info"></i> Created</div>}
            {this.eventType === "Failed" && <div><i className="fa fa-circle fa-lg text-warning"></i> Failed</div>}
            {this.eventType === "Removed" && <div><i className="fa fa-circle fa-lg text-danger"></i> Removed</div>}
        </div>;
    }
}
