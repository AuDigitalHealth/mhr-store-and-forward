import { fetch, addTask } from 'domain-task';
import { Action, Reducer, ActionCreator } from 'redux';
import { AppThunkAction } from './';
import { Settings } from "../appSettings";


// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface TimelineState {
    isLoading: boolean;
    timeline: TimelineList;
}

export interface TimelineEvent {
    id : string;
    type: string;
    document_id: string;
    details: string;
    event_date_time: string;
}

export interface TimelineList {
    timeline_events: TimelineEvent[];
}




// -----------------
// ACTIONS - These are serializable (hence replayable) descriptions of state transitions.
// They do not themselves have any side-effects; they just describe something that is going to happen.

interface RequestTimelineAction {
    type: 'REQUEST_TIMELINE';
}

interface ReceiveTimelineAction {
    type: 'RECEIVE_TIMELINE';
    timeline: TimelineList;
}

// Declare a 'discriminated union' type. This guarantees that all references to 'type' properties contain one of the
// declared type strings (and not any other arbitrary string).
type KnownAction = RequestTimelineAction | ReceiveTimelineAction;

// ----------------
// ACTION CREATORS - These are functions exposed to UI components that will trigger a state transition.
// They don't directly mutate state, but they can have external side-effects (such as loading data).

export const actionCreators = {
    requestTimeline: (): AppThunkAction<KnownAction> => async (dispatch, getState) => {
       
        let fetchTask = fetch(Settings.endpoint + '/api/events')
            .then(response => response.json() as Promise<TimelineList>)
            .then(data =>
                {
                    dispatch({ type: 'RECEIVE_TIMELINE', timeline: data });
                });

            addTask(fetchTask); // Ensure server-side prerendering waits for this to complete
            dispatch({ type: 'REQUEST_TIMELINE' });
    }
};

// ----------------
// REDUCER - For a given state and action, returns the new state. To support time travel, this must not mutate the old state.

const unloadedState: TimelineState = { timeline: { timeline_events: [] }, isLoading: false };

export const reducer: Reducer<TimelineState> = (state: TimelineState, incomingAction: Action) => {

    const action = incomingAction as KnownAction;

    switch (action.type) {
        case 'REQUEST_TIMELINE':
            return {
                timeline: state.timeline,
                isLoading: true
            };
        case 'RECEIVE_TIMELINE':

            return {
                timeline: action.timeline,
                isLoading: false
            };

        default:
            // The following line guarantees that every action in the KnownAction union has been covered by a case above
            const exhaustiveCheck: never = action;
    }

    return state || unloadedState;
};
