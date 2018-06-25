import * as DocumentQueue from './DocumentQueueData';
import * as Timeline from './TimelineData';
import * as DocumentView from './DocumentViewData';
import * as UploadDocument from './UploadDocumentData';

// The top-level state object
export interface ApplicationState {
    documentQueue: DocumentQueue.DocumentQueueState;
    documentView: DocumentView.DocumentViewState;
    timeline: Timeline.TimelineState;
    uploadDocument: UploadDocument.UploadDocumentState;
}

// Whenever an action is dispatched, Redux will update each top-level application state property using
// the reducer with the matching name. It's important that the names match exactly, and that the reducer
// acts on the corresponding ApplicationState property type.
export const reducers = {
    documentQueue: DocumentQueue.reducer,
    documentView: DocumentView.reducer,
    timeline: Timeline.reducer,
    uploadDocument:UploadDocument.reducer
};

// This type can be used as a hint on action creators so that its 'dispatch' and 'getState' params are
// correctly typed to match your store.
export interface AppThunkAction<TAction> {
    (dispatch: (action: TAction) => void, getState: () => ApplicationState): void;
}
