import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
import { Settings } from '../appSettings';


// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface DocumentViewState {
    documentIsLoading: boolean;
    documentError: boolean;
    documentDeleteSuccess: boolean;
    documentDeleteError: boolean;
    eventsIsLoading: boolean;
    document: DocumentDetail;
    eventList: EventList;
}

export interface DocumentDetail {
    id: string;
    status: string;
    document_id: string;
    queue_date_time: string;
    format_code: string;
    format_code_name: string;
    ihi: string;
    replace_id: string;
    data: string;
}

export interface EventList {
    events: DocumentEvent[]
}

export interface DocumentEvent {
    id: string;
    event_date_time: string;
    details: string;
    type: string;
    document_link: string;
}

// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface RequestDeleteDocumentAction {
    type: 'REQUEST_DELETE_DOCUMENT';
    id: string;
}

interface ReceiveDeleteDocumentAction {
    type: 'RECEIVE_DELETE_DOCUMENT';
}

interface ReceiveDeleteDocumentErrorAction {
    type: 'RECEIVE_DELETE_DOCUMENT_ERROR';
}

interface RequestDocumentViewAction {
    type: 'REQUEST_DOCUMENT_VIEW';
    id: string;
}

interface ReceiveDocumentViewAction {
    type: 'RECEIVE_DOCUMENT_VIEW';
    document: DocumentDetail;
}

interface ReceiveDocumentViewErrorAction {
    type: 'RECEIVE_DOCUMENT_VIEW_ERROR';
}

interface RequestDocumentEventsAction {
    type: 'REQUEST_DOCUMENT_EVENTS';
    id: string;
}

interface ReceiveDocumentEventsAction {
    type: 'RECEIVE_DOCUMENT_EVENTS';
    eventList: EventList;
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = RequestDocumentViewAction
                   | ReceiveDocumentViewAction
                   | ReceiveDeleteDocumentErrorAction
                   | RequestDocumentEventsAction
                   | ReceiveDocumentEventsAction
                   | ReceiveDocumentViewErrorAction
                   | RequestDeleteDocumentAction
                   | ReceiveDeleteDocumentAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
    requestDocumentView: (id :string): AppThunkAction<KnownAction> => async (dispatch, getState) => {
        let fetchTask = fetch(Settings.endpoint + `/api/documents/` + id)
            .then(response => {
                if (response.ok) {
                    return response.json() as Promise<DocumentDetail>;
                } else {
                    dispatch({ type: 'RECEIVE_DOCUMENT_VIEW_ERROR' });
                    return response;
                }
            })
            .then(data => {
                dispatch({ type: 'RECEIVE_DOCUMENT_VIEW', document: data });
            });
            //.catch(data => {
            //    dispatch({ type: 'RECEIVE_DOCUMENT_VIEW_ERROR' });
            //});

        addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
        dispatch({ type: 'REQUEST_DOCUMENT_VIEW', id: id });
    },
    requestDocumentEvents: (id: string): AppThunkAction<KnownAction> => async (dispatch, getState) => {
        let fetchTask = fetch(Settings.endpoint + `/api/documents/` + id + `/events`)
            .then(response => response.json() as Promise<EventList>)
            .then(data => {
                dispatch({ type: 'RECEIVE_DOCUMENT_EVENTS', eventList: data });
            });

        addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
        dispatch({ type: 'REQUEST_DOCUMENT_EVENTS', id: id });
    },
    requestDeleteDocument: (id: string): AppThunkAction<KnownAction> => async (dispatch, getState) => {
        let fetchTask = fetch(Settings.endpoint + `/api/documents/` + id,
                {
                    method: 'delete'
                })
            .then(response => {
                console.log("response received for delete request");

                if (response.ok) {
                    dispatch({ type: 'RECEIVE_DELETE_DOCUMENT' });
                    return response;
                } else {
                    toastr.warning("Document cannot be removed.");
                    dispatch({ type: 'RECEIVE_DELETE_DOCUMENT_ERROR' });
                    return response;
                }
            });

        addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
        dispatch({ type: 'REQUEST_DELETE_DOCUMENT', id: id });
    }
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: DocumentViewState = {
    document: {
        id: "",
        status: "",
        document_id: "",
        queue_date_time: "",
        format_code: "",
        format_code_name: "",
        ihi: "",
        replace_id: "",
        data: ""
    },
    eventList: {
        events: []
    },
    documentIsLoading: false,
    documentDeleteSuccess: false,
    documentDeleteError: false,
    eventsIsLoading: false,
    documentError: false
};

export const reducer: Reducer<DocumentViewState> = (state: DocumentViewState, incomingAction: Action) => {

    const action = incomingAction as KnownAction;
    
    switch (action.type) {
        case 'REQUEST_DOCUMENT_VIEW':
            return {
                document: state.document,
                documentIsLoading: true,
                documentDeleteSuccess: false,
                documentDeleteError: false,
                eventsIsLoading: state.eventsIsLoading,
                eventList: state.eventList,
                documentError: false
            };
        case 'RECEIVE_DOCUMENT_VIEW':
            return {
                document: action.document,
                documentIsLoading: false,
                documentDeleteSuccess: state.documentDeleteSuccess,
                documentDeleteError: state.documentDeleteError,
                eventsIsLoading: state.eventsIsLoading,
                eventList: state.eventList,
                documentError: state.documentError
            };
        case 'RECEIVE_DOCUMENT_VIEW_ERROR':
            return {
                document: state.document,
                documentIsLoading: false,
                documentDeleteSuccess: state.documentDeleteSuccess,
                documentDeleteError: state.documentDeleteError,
                eventsIsLoading: state.eventsIsLoading,
                eventList: state.eventList,
                documentError: true
            };
        case 'REQUEST_DOCUMENT_EVENTS':
            return {
                document: state.document,
                documentIsLoading: state.documentIsLoading,
                documentDeleteSuccess: false,
                documentDeleteError: false,
                eventsIsLoading: state.eventsIsLoading,
                eventList: state.eventList,
                documentError: state.documentError
            };
        case 'RECEIVE_DOCUMENT_EVENTS':
            return {
                document: state.document,
                documentIsLoading: state.documentIsLoading,
                documentDeleteSuccess: state.documentDeleteSuccess,
                documentDeleteError: state.documentDeleteError,
                eventsIsLoading: state.eventsIsLoading,
                eventList: action.eventList,
                documentError: state.documentError
            };
        case 'REQUEST_DELETE_DOCUMENT':
            return {
                document: state.document,
                documentIsLoading: state.documentIsLoading,
                documentDeleteSuccess: state.documentDeleteSuccess,
                documentDeleteError: state.documentDeleteError,
                eventsIsLoading: state.eventsIsLoading,
                eventList: state.eventList,
                documentError: state.documentError
            };
        case 'RECEIVE_DELETE_DOCUMENT':
            return {
                document: state.document,
                documentIsLoading: state.documentIsLoading,
                documentDeleteSuccess: true,
                documentDeleteError: state.documentDeleteError,
                eventsIsLoading: state.eventsIsLoading,
                eventList: state.eventList,
                documentError: state.documentError
            };
        case 'RECEIVE_DELETE_DOCUMENT_ERROR':
            return {
                document: state.document,
                documentIsLoading: state.documentIsLoading,
                documentDeleteSuccess: state.documentDeleteSuccess,
                documentDeleteError: true,
                eventsIsLoading: state.eventsIsLoading,
                eventList: state.eventList,
                documentError: state.documentError
            };
        default:
            // The following line guarantees that every action in the KnownAction union has been covered by a case above
            const exhaustiveCheck: never = action;
    }
    
    return state || unloadedState;
};
