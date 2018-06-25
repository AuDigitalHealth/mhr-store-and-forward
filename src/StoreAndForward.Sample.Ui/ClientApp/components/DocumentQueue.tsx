import * as React from 'react';
import { Link, RouteComponentProps } from 'react-router-dom';
import { connect } from 'react-redux';
import { ApplicationState } from '../store';
import * as DocumentQueueState from '../store/DocumentQueueData';
import { DocumentStatus } from './DocumentStatus';
import { DocumentId } from './DocumentId';
import { DateTimeDisplay} from './DateTimeDisplay';

 
type DocumentQueueProps =
    DocumentQueueState.DocumentQueueState
    & typeof DocumentQueueState.actionCreators
    & RouteComponentProps<{}>;

class DocumentQueue extends React.Component<DocumentQueueProps, {}> {

    componentWillMount() {
        this.refreshDocumentQueue.bind(this);

        this.props.requestDocumentQueue();
    }

    private refreshDocumentQueue() {
        this.props.requestDocumentQueue();
    }
    
    public render() {
        return <div>
            <div className="row">

                <div className="col">
                    <h2 className="pull-left"><i className="fa fa-file-text-o"></i> Document Queue</h2>

                    <div className="pull-right">
                        <Link to={'/upload'} className="btn btn-outline-success">
                            <i className="fa d-inline fa-plus"></i> Add Document
                        </Link>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<button onClick={() => { this.refreshDocumentQueue() }} className="btn btn-outline-primary"><i className="fa d-inline fa-refresh"></i> Refresh </button>
                    </div>
                </div>

            </div>

            <hr />

            <br />

            {this.props.isLoading && 
                <div className="spin">
                    <div>
                        <i className="fa fa-spinner fa-pulse fa-3x fa-fw"></i>
                        <br />
                        <br />
                        <span>Loading documents...</span>
                    </div>
                </div>
            }

            <div className="row">
                <div className="col">
                    <table className="table">
                        <thead>
                            <tr>
                                <th className="documentStatusColumn">Status</th>
                                <th className="dateColumn">Queue Date</th>
                                <th className="documentIdColumn">Document ID</th>
                                <th>Type</th>
                                <th>IHI</th>
                                <th></th>
                            </tr>
                        </thead>

                        <tbody>

                            {this.props.documents.documents.map(document =>
                                <tr key={document.id}>
                                    <td>
                                        <DocumentStatus documentStatus={document.status}></DocumentStatus>
                                    </td>
                                    <td>
                                        <DateTimeDisplay dateTime={document.queue_date_time}></DateTimeDisplay>
                                    </td>
                                    <td>
                                        <DocumentId documentId={document.document_id}></DocumentId>
                                    </td>
                                    <td><span title={document.format_code}>{document.format_code_name}</span> </td>
                                    <td>{document.ihi}</td>
                                    <td>
                                        <div className="pull-right">
                                            <Link to={`/documents/${document.id}`} className="btn btn-outline-primary">
                                                <i className="fa fa-file-text-o"></i> View
                                            </Link>
                                        </div>

                                    </td>
                                </tr>
                            )}

                        </tbody>

                    </table>
                </div>
            </div>

        </div>;
    }
}

export default connect(
    (state: ApplicationState) => state.documentQueue,
    DocumentQueueState.actionCreators
)(DocumentQueue) as typeof DocumentQueue;
