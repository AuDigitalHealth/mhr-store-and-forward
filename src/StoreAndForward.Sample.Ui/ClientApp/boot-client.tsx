/// <reference types="@types/signalr" />
/// <reference types="@types/toastr" />
/// <reference types="signalr" />

import './css/site.css';
import 'bootstrap';
import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { AppContainer } from 'react-hot-loader';
import { Provider } from 'react-redux';
import { ConnectedRouter } from 'react-router-redux';
import { createBrowserHistory } from 'history';
import configureStore from './configureStore';

import { ApplicationState } from './store';
import * as RoutesModule from './routes';
import 'bootstrap';
import { Settings } from './appSettings'
let routes = RoutesModule.routes;

import 'popper.js';
import * as toastr from  "toastr";

import 'bootstrap/scss/bootstrap.scss'
import 'font-awesome/scss/font-awesome.scss';
import 'toastr/toastr.scss'; 
import 'signalR/jquery.signalR';


declare const $: any;

// Create browser history to use in the Redux store
const baseUrl = document.getElementsByTagName('base')[0].getAttribute('href')!;
const history = createBrowserHistory({ basename: baseUrl });

// Get the application-wide store instance, prepopulating with state from the server where available.
const initialState = (window as any).initialReduxState as ApplicationState;
const store = configureStore(history, initialState);
//test

 

async function renderApp() {
    // This code starts up the React app when it runs in a browser. It sets up the routing configuration
    // and injects the app into a DOM element.


    

    try { 
         
        var connection = $.hubConnection(Settings.endpoint + '/signalr');

        var contosoChatHubProxy = connection.createHubProxy('notificationHub');
        contosoChatHubProxy.on('addNotification', (data:any) => {
            toastr.info("Queue updated");
        });
        connection.start({withCredentials:false}).done(function () {
            toastr.success("Notifications started");
        });

    } catch (e) {
        
        toastr.error(e, "SignalR Error");
    } 
 
    ReactDOM.render(
        <AppContainer>
            <Provider store={store}>
                <ConnectedRouter history={history} children={routes} />
            </Provider>
        </AppContainer>,
        document.getElementById('react-app')
    );
}

renderApp();

// Allow Hot Module Replacement
if (module.hot) {
    module.hot.accept('./routes', () => {
        routes = require<typeof RoutesModule>('./routes').routes;
        renderApp();
    });
}

 

