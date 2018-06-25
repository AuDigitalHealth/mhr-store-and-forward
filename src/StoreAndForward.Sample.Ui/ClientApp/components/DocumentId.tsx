import * as React from 'react';

export class DocumentIdProps {
    documentId : string;
}

export class DocumentId extends React.Component<DocumentIdProps, {}> {

    documentId : string = this.props.documentId;

    public render() {
        return <div>
            <span title={this.props.documentId}>...{this.props.documentId.substring(this.props.documentId.length - 8, this.props.documentId.length)}</span>
        </div>;
    }
}
