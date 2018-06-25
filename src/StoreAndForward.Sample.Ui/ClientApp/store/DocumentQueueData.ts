import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
import { Settings} from '../appSettings';
 


// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface DocumentQueueState {
    isLoading: boolean;
    documents: DocumentList;
}

export interface DocumentListItem {
    id : string;
    status: string;
    document_id: string;
    queue_date_time: string;
    format_code: string;
    format_code_name: string;
    ihi: string;
}

export interface DocumentList {
    documents: DocumentListItem[];
}




// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface RequestDocumentQueueAction {
    type: 'REQUEST_DOCUMENT_QUEUE';
}

interface ReceiveDocumentQueueAction {
    type: 'RECEIVE_DOCUMENT_QUEUE';
    documents: DocumentList;
}


// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = RequestDocumentQueueAction | ReceiveDocumentQueueAction

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
    requestDocumentQueue: (): AppThunkAction<KnownAction> => async (dispatch, getState) => {

        let fetchTask = fetch(Settings.endpoint + '/api/documents')

            .then(response => response.json() as Promise<DocumentList>)
            .then(data =>
                {
                    dispatch({ type: 'RECEIVE_DOCUMENT_QUEUE', documents: data });
                });

            addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
            dispatch({ type: 'REQUEST_DOCUMENT_QUEUE' });
    }
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: DocumentQueueState = {
     documents: {
          documents: []
     }, isLoading: false
};

export const reducer: Reducer<DocumentQueueState> = (state: DocumentQueueState, incomingAction: Action) => {

    const action = incomingAction as KnownAction;

    switch (action.type) {
        case 'REQUEST_DOCUMENT_QUEUE':
            return {
                documents: unloadedState.documents,
                isLoading: true,
            };
        case 'RECEIVE_DOCUMENT_QUEUE':
            return {
                documents: action.documents,
                isLoading: false,
            };

        default:
            // The following line guarantees that every action in the KnownAction union has been covered by a case above
            const exhaustiveCheck: never = action;
    }

    return state || unloadedState;
};
