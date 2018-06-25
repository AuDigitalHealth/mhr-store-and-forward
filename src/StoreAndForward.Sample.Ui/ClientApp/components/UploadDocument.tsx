import * as React from 'react';
import * as moment from 'moment';
import { Link, RouteComponentProps } from 'react-router-dom';
import { withRouter, RouteProps } from 'react-router';
import * as toastr from "toastr";
import * as UploadState from '../store/UploadDocumentData';
import { ApplicationState } from '../store';
import { connect } from 'react-redux';

type UploadDocumentProps =
    UploadState.UploadDocumentState
    & typeof UploadState.actionCreators
    & RouteComponentProps<{}>;

class UpdateDocument extends React.Component<UploadDocumentProps, {}> {

    private formatCode: any;
    private formatCodeName: any;
    private replaceId: any;
    private fileInput: any;
    private myFormRef: any;

    constructor(props: UploadDocumentProps) {
        super(props);
        this.handleSubmit = this.handleSubmit.bind(this);
        this.reset = this.reset.bind(this);
    }

    componentWillReceiveProps(nextProps: any) {
        if (nextProps.isSuccess === true) {
            toastr.success('Document uploaded');
            this.props.history.push('/');
        }
    }


    handleSubmit(event: any) {
        event.preventDefault();

        var e: string = '';
        if (this.fileInput.files.length === 0) {
            e += " 'CDA Package' is missing.";
        }
        if (!this.formatCode.value) {
            e += " 'Format Code'  is missing.";
        }
        if (!this.formatCodeName.value) {
            e += " 'Format Code Name' is missing.";
        }

        if (e.length > 0) {
            this.props.setError(e);
            return;
        }

        this.props.uploadDocumentQueue({
            data: this.fileInput.files[0],
            format_code_name: this.formatCodeName.value,
            format_code: this.formatCode.value,
            replace_id: this.replaceId.value
        });
    }

    reset(event: any) {

        event.preventDefault();
        this.myFormRef.reset();
        console.log('Form reset: ' + this.props.document);
        this.props.resetAction();
    }



    public render() {
        return (<div className="container" style={{ position: 'relative' }}>


            <div className="row ">
                <div className="col">
                    <h2 className="pull-left"> <i className="fa fa-upload"></i> Add Document </h2>
                    <div className="pull-right">
                        <Link to={'/'} className="btn btn-outline-primary">  <i className="fa d-inline fa-angle-left"></i> Back </Link>
                    </div>
                    <div className="clearfix"></div>
                </div>
            </div>

            <hr />

            {this.props.isLoading &&
                <div className="spin">
                    <div>
                        <i className="fa fa-spinner fa-pulse fa-3x fa-fw"></i>
                        <br />
                        <br />
                        <span>Uploading document...</span>
                    </div>
                </div>
            }

            {this.props.isError &&
                <div className="alert alert-danger" role="alert">
                    {this.props.error}
                </div>
            }


            <div className="row ">
                <div className="col-6">
                    <fieldset>

                        <form onSubmit={this.handleSubmit} ref={(el: any) => this.myFormRef = el}>
                            <div className="form-group">
                                <label ><b>CDA Package:</b></label>
                                <input type="file" className="form-control" ref={(input: any) => {
                                    this.fileInput = input;
                                }} />
                            </div>

                            <div className="form-group">
                                <label ><b>Format Code:</b></label>
                                <input type="text" className="form-control" placeholder="Enter format code" ref={
                                    (node: any) => {
                                        this.formatCode = node;
                                    }} />

                            </div>
                            <div className="form-group">
                                <label ><b>Format Code Name:</b></label>
                                <input type="text" className="form-control" placeholder="Enter format code name" ref={
                                    (node: any) => {
                                        this.formatCodeName = node;
                                    }} />
                            </div>

                            <div className="form-group">
                                <label ><b>Replacement Document ID:</b></label>
                                <input type="text" className="form-control" placeholder="Enter document to replace" ref={
                                    (node: any) => {
                                        this.replaceId = node;
                                    }} />
                            </div>

                            <div className="row">
                                <div className="col">
                                    <hr />

                                    <button type="submit" className="btn btn-success"><i className="fa fa-upload"></i> Upload Document</button> &nbsp;
                                            <button onClick={this.reset} type="reset" className="btn btn-danger"><i className="fa fa-trash-o"></i> Reset</button>&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;
                                            <Link to={'/'} className="btn btn-outline-secondary"><i className="fa d-inline fa-angle-left"></i> Cancel</Link>
                                </div>
                            </div>
                        </form>
                    </fieldset>
                </div>
            </div>


        </div>);
    }
}
export default connect(
    (state: ApplicationState) => state.uploadDocument,
    UploadState.actionCreators
)(UpdateDocument) as typeof UpdateDocument;