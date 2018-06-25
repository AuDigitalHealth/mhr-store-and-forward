import * as React from 'react';
import { RouteComponentProps } from 'react-router-dom';
import { connect } from 'react-redux';
import { ApplicationState } from '../store';
import * as TimelineState from '../store/TimelineData';
import { DocumentId } from './DocumentId';
import { EventType} from './EventType';
import { DateTimeDisplay} from './DateTimeDisplay';

type TimelineProps =
    TimelineState.TimelineState
    & typeof TimelineState.actionCreators
    & RouteComponentProps<{}>;


class Timeline extends React.Component<TimelineProps, {}> {

    componentWillMount() {
        this.props.requestTimeline();
    }

    private refreshTimeline() {
        this.props.requestTimeline();
    }

    public render() {
        return <div>

            <div className="row">
                <div className="col">
                    <h2 className="pull-left"><i className="fa fa-clock-o"></i> Timeline</h2>

                    <div className="pull-right">
                        <button href="#" onClick={() => { this.refreshTimeline() }} className="btn btn-outline-primary"><i className="fa d-inline fa-refresh"></i> Refresh </button>
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
                        <span>Loading timeline...</span>
                    </div>
                </div>
            }

            <div className="row">
                <div className="col">
                    <table className="table">
                        <thead>
                            <tr>
                                <th className="eventTypeColumn">Type</th>
                                <th className="dateColumn">Event Date</th>
                                <th className="documentIdColumn">Document ID</th>
                                <th>Details</th>
                            </tr>
                        </thead>

                        <tbody>

                            {this.props.timeline.timeline_events.map(event =>
                                <tr key={event.id}>
                                    <td>
                                        <EventType eventType={event.type}></EventType>
                                    </td>
                                    <td>
                                        <DateTimeDisplay dateTime={event.event_date_time}></DateTimeDisplay>
                                    </td>
                                    <td>
                                        <DocumentId documentId={event.document_id}></DocumentId>
                                    </td>
                                    <td>{event.details}</td>
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
    (state: ApplicationState) => state.timeline,
    TimelineState.actionCreators
)(Timeline) as typeof Timeline;
