import * as React from 'react';


export class DocumentStatusProps {
    documentStatus : string;
}

export class DocumentStatus extends React.Component<DocumentStatusProps, {}> {

    documentStatus : string = this.props.documentStatus;

    public render() {
        return <div>
            {this.documentStatus === "Sent" && <div><i className="fa fa-circle fa-lg text-success"></i> Sent</div>}
            {this.documentStatus === "RetryLimitReached" && <div><i className="fa fa-circle fa-lg text-warning"></i> Retry Limit Reached</div>}
            {this.documentStatus === "Pending" && <div><i className="fa fa-circle fa-lg text-info"></i> Pending</div>}
            {this.documentStatus === "Sending" && <div><i className="fa fa-circle fa-lg text-primary"></i> Sending</div>}
            {this.documentStatus === "Removed" && <div><i className="fa fa-circle fa-lg text-danger"></i> Removed</div>}
        </div>;
    }
}
