import { Routes } from '@angular/router';

export const SAMPLES_ROUTES: Routes = [

    {
        path: '',
        loadComponent: () => import('./pages/samples/samples.component').then(m => m.SamplesComponent),
        title: 'Muestras'
    },

    {
        path: 'new',
        loadComponent: () => import('./pages/form-samples/form-samples.component').then(m => m.FormSamplesComponent),
        title: 'Registrar muestra'
    }

];
