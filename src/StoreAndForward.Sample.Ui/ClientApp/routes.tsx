import * as React from 'react';
import { Route } from 'react-router-dom';
import { MainLayout } from './components/MainLayout';
import DocumentQueue from './components/DocumentQueue';
import DocumentView from './components/DocumentView';
import Timeline from './components/Timeline';
import UpdateDocument from './components/UploadDocument';

export const routes = <MainLayout>
    <Route exact path='/' component={DocumentQueue} />
    <Route exact path='/timeline' component={Timeline} />
    <Route exact path='/upload' component={UpdateDocument} />
    <Route exact path='/documents/:id?' component={DocumentView} />
</MainLayout>;
