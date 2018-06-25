import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
import { Settings } from '../appSettings';
import { ApplicationState } from './index';
// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface UploadDocumentState {
    isLoading: boolean;
    isError: boolean;
    isSuccess:boolean;
    error?: any;
    document: DocumentItem;
}

export interface DocumentItem {
    data: any,
    format_code_name: string,
    format_code: string,
    replace_id?: any
}


// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface UploadDocumentQueueAction { type: 'UPLOAD_DOCUMENT_QUEUE'; document: DocumentItem; }

interface ResetAction { type: 'RESET'; }
interface UploadStartedAction { type: 'UPLOAD_STARTED'; }
interface UploadFailedAction { type: 'UPLOAD_FAILED'; error: any }
interface UploadSuccessAction { type: 'UPLOAD_SUCCESS'; document: DocumentItem }

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = UploadDocumentQueueAction | ResetAction | UploadStartedAction | UploadFailedAction | UploadSuccessAction

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
    setError: (input: any): AppThunkAction<KnownAction> => async (dispatch, getState) => {
        dispatch({ type: 'UPLOAD_FAILED', error: input });
    },
    uploadDocumentQueue: (input: DocumentItem): AppThunkAction<KnownAction> => async (dispatch, getState) => {

        dispatch({ type: 'UPLOAD_STARTED' });

        var reader = new FileReader();
        reader.readAsDataURL(input.data);
        reader.onload = () => {

            var binaryString = reader.result;
            binaryString = binaryString.replace('data:application/x-zip-compressed;base64,', '');

            var replaceId = null;

            if (input.replace_id !== null && input.replace_id.length > 0) {
                replaceId = input.replace_id;
            }

            var data = JSON.stringify({
                data: binaryString,
                format_code: input.format_code,
                format_code_name: input.format_code_name,
                replace_id: replaceId,
            });


            fetch(Settings.endpoint + '/api/documents', {
                method: 'post',
                headers: {
                    "Content-Type": "application/json"
                },
                body: data
            }).then(response => {


                if (response.ok) {
                    console.log('Success Called');
                    var result = response.json() as Promise<DocumentItem>;
                    result.then(x => {
                        dispatch({ type: 'UPLOAD_SUCCESS', document: x });
                    });
                }

                response.text().then((x: any) => {
                    var message = JSON.parse(x);
                    dispatch({ type: 'UPLOAD_FAILED', error: message.Message });
                }).catch(() => {
                    dispatch({ type: 'UPLOAD_FAILED', error: 'Upload Failed' });
                });
                   
            }).catch((e) => {
                console.log(e); 
                dispatch({ type: 'UPLOAD_FAILED', error: 'Upload Failed' });
            });
        };
        reader.onerror = error => {
            dispatch({ type: 'UPLOAD_FAILED', error: error });
        };

    },
    resetAction: (): AppThunkAction<KnownAction> => async (dispatch, getState) => {
        console.log('Action Creator Reset called');
        dispatch({ type: 'RESET' });
    }
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const initialState: UploadDocumentState = {
    error: '',
    isSuccess:false,
    isLoading: false,
    isError: false,
    document: {
        data: '',
        format_code: '',
        format_code_name: '',
        replace_id: null
    }
};

export const reducer: Reducer<UploadDocumentState> = (state: UploadDocumentState = initialState, a: Action) => {

    const action = a as KnownAction;

    switch (action.type) {
        case 'UPLOAD_STARTED':
            console.log('Reducer UPLOAD_STARTED called');
            return {
                error: '',
                isLoading: true,
                isSuccess: false,
                isError: false,
                document: {
                    data: state.document.data,
                    format_code: state.document.format_code,
                    format_code_name: state.document.format_code_name,
                    replace_id: state.document.replace_id,
                }
            };

        case 'UPLOAD_FAILED':
            console.log('Reducer UPLOAD_FAILED called');
            return {
                error: action.error,
                isLoading: false,
                isSuccess: false,
                isError: true,
                document: {
                    data: state.document.data,
                    format_code: state.document.format_code,
                    format_code_name: state.document.format_code_name,
                    replace_id: state.document.replace_id,
                }
            };
        case 'UPLOAD_SUCCESS':
            console.log('Reducer UPLOAD_SUCCESS called');
            return {
                error: '',
                isLoading: false,
                isSuccess: true,
                isError: false,
                document: action.document
            };
        case 'RESET':
            console.log('Reducer Reset called');
            return {
                error: '',
                isLoading: false,
                isSuccess: false,
                isError: false,
                document: {
                    data: '',
                    format_code: '',
                    format_code_name: '',
                    replace_id: null,
                }
            };
        case 'UPLOAD_DOCUMENT_QUEUE':
            return {
                document: initialState.document,
                isLoading: true,
                isError: false,
                isSuccess: false,
            };

        default:
            // The following line guarantees that every action in the KnownAction union has been covered by a case above
            const exhaustiveCheck: never = action;
    }

    return state || initialState;
};
