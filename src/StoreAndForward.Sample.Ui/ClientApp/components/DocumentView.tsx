import * as React from 'react';
import { Link, RouteComponentProps, Redirect } from 'react-router-dom';
import { connect } from 'react-redux';
import { ApplicationState } from '../store';
import * as DocumentViewState from '../store/DocumentViewData';
import * as FileSaver from 'file-saver';
import { DateTimeDisplay } from './DateTimeDisplay';
import { DocumentStatus } from './DocumentStatus';
import { EventType } from './EventType';
import * as toastr from "toastr";

type DocumentViewProps =
    DocumentViewState.DocumentViewState
    & typeof DocumentViewState.actionCreators
    & RouteComponentProps<{ id: string }>;


class DocumentView extends React.Component<DocumentViewProps, {}> {

    componentWillMount() {
        this.props.requestDocumentView(this.props.match.params.id);
        this.props.requestDocumentEvents(this.props.match.params.id);
    }

    componentWillReceiveProps(nextProps: DocumentViewProps) {
        if (nextProps.documentDeleteSuccess) {
            toastr.info("Document removed.");
            this.props.history.push('/');
        }
    }

    downloadFile(data: any, documentId: string) {
        var url = "data:application/zip;base64," + data;

        fetch(url)
            .then(res => res.blob())
            .then(blob => {
                FileSaver.saveAs(blob, "cda-package-" + documentId + ".zip");
            });
    }

    deleteDocument(id: string) {
        this.props.requestDeleteDocument(id);
    }

    getStyle() {
        return "width: 50%";
    }

    public render() {
            return <div>
                       <div className="row">
                           <div className="col">
                               <h2 className="pull-left">
                                   <i className="fa fa-file-text-o"></i>
                                   &nbsp;Document
                               </h2>
                               <div className="pull-right">
                                   {this.props.document.status === "Pending"
                                       ? <button className="btn btn-outline-danger" onClick={() => {
                                           this.deleteDocument(this.props.document.id);
                                    }}> <i className="fa fa-times"></i> Remove Document
                                         </button>
                                       : null
                                   }
                                   &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                   <Link className="btn btn-outline-primary" to={'/'}>
                                       <i className="fa d-inline fa-angle-left"></i> Back
                                   </Link>

                               </div>
                               <div className="clearfix "> </div>
                           </div>
                       </div>
                       <hr/>

                       <br/>

                       {this.props.documentIsLoading &&
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

                               <ul className="nav nav-tabs" id="myTab" role="tablist">
                                   <li className="nav-item">
                                       <a className="nav-link active" id="details-tab" data-toggle="tab" aria-controls="details" aria-selected="true" href="#details"> Details</a>
                                   </li>
                                   <li className="nav-item">
                                       <a className="nav-link" id="events-tab" data-toggle="tab" aria-controls="events" aria-selected="false"
                                          href="#events"> Events <span className="badge badge-dark">{this.props
                                              .eventList.events
                                              .length}</span> </a>
                                   </li>
                               </ul>

                               <div className="tab-content" id="myTabContent">

                                   <div className="tab-pane fade show active" id="details" role="tabpanel" aria-labelledby="details-tab">

                                       {this.props.documentError &&
                                           <div>
                                               <br/>
                                               <div className="alert alert-warning" role="alert">Document does not exist.</div>
                                           </div>
                                       }

                                       {!this.props.documentIsLoading &&
                                           !this.props.documentError &&
                                           <div>
                                               <br/>

                                               <div className="row">
                                                   <div className="col">


                                                       <div className="form-group">
                                                           <label>
                                                               <b>Document ID:</b>
                                                           </label>
                                                           <br/>
                                                           {this.props.document.document_id}
                                                       </div>


                                                       <div className="form-group">
                                                           <label>
                                                               <b>Status:</b>
                                                           </label>
                                                           <br/>

                                                           <DocumentStatus documentStatus={this.props.document.status}></DocumentStatus>
                                                       </div>

                                                       <div className="form-group">
                                                           <label>
                                                               <b>Queue Date:</b>
                                                           </label>
                                                           <br/>
                                                           <DateTimeDisplay dateTime={this.props.document.queue_date_time}></DateTimeDisplay>
                                                       </div>

                                                       <div className="form-group">
                                                           <label>
                                                               <b>Document Data:</b>
                                                           </label>
                                                           <br/>
                                                           <button type="button" className="btn btn-primary" onClick={() => {
                                                    this.downloadFile(this.props.document.data,
                                                        this.props.document.document_id)
                                                }}>
                                                               <i className="fa fa-download"></i> Download
                                                           </button>

                                                       </div>

                                                   </div>
                                                   <div className="col">


                                                       <div className="form-group">
                                                           <label>
                                                               <b>Format Code:</b>
                                                           </label>
                                                           <p>
                                                               {this.props.document.format_code}
                                                           </p>
                                                       </div>

                                                       <div className="form-group">
                                                           <label>
                                                               <b>Format Code Name:</b>
                                                           </label>
                                                           <p>
                                                               {this.props.document.format_code_name}
                                                           </p>
                                                       </div>

                                                       <div className="form-group">
                                                           <label>
                                                               <b>IHI:</b>
                                                           </label>
                                                           <p>
                                                               {this.props.document.ihi}
                                                           </p>
                                                       </div>

                                                       {this.props.document.replace_id != null &&
                                                <div className="form-group">
                                                    <label>
                                                        <b>Replaced Document:</b>
                                                    </label>
                                                    <p>
                                                        <a href="ViewDocument.html">{this.props.document.replace_id
                                                        }</a>
                                                    </p>
                                                </div>
                                            }

                                                   </div>
                                               </div>
                                           </div>
                                       }
                                   </div>


                            <div className="tab-pane fade" id="events" role="tabpanel" aria-labelledby="events-tab">

                                {this.props.eventsIsLoading &&
                                    <div className="spin">
                                        <div>
                                            <i className="fa fa-spinner fa-pulse fa-3x fa-fw"></i>
                                            <br />
                                            <br />
                                            <span>Loading events...</span>
                                        </div>
                                    </div>
                                }

                                {!this.props.eventsIsLoading &&
                                    <div>
                                       <br/>
                                       <table className="table">
                                           <thead>
                                           <tr>
                                               <th className="eventTypeColumn">Status</th>
                                               <th className="dateColumn">Date</th>
                                               <th>Description</th>
                                           </tr>
                                           </thead>
                                           <tbody>
                                           {
                                               this.props.eventList.events.map(event =>
                                                   <tr key={event.id}>
                                                       <td>
                                                           <EventType eventType={event.type}></EventType>
                                                       </td>
                                                       <td>
                                                           <DateTimeDisplay dateTime={event.event_date_time}></DateTimeDisplay>
                                                       </td>
                                                       <td>
                                                           {event.details}
                                                       </td>
                                                   </tr>
                                               )}

                                           </tbody>
                                    </table>
                                    </div>
                                    }
                                   </div>
                               </div>
                           </div>
                       </div>

                   </div>;
        
    }
}


export default connect(
    (state: ApplicationState) => state.documentView,
    DocumentViewState.actionCreators
)(DocumentView) as typeof DocumentView;
