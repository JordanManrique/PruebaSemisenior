import { Routes } from '@angular/router';

export const routes: Routes = [

    { path: '', redirectTo: 'samples', pathMatch: 'full' },

    { path: 'samples', loadChildren: () => import('./features/samples.route').then(m => m.SAMPLES_ROUTES) },

    // Ruta antigua del formulario: se conserva redirigiendo a la nueva.
    { path: 'form-samples', redirectTo: 'samples/new', pathMatch: 'full' },

    {
        path: '**',
        loadComponent: () => import('./shared/dato_no_encotnrado/dato_no_encontrado.component').then(m => m.DatoNoEncontradoComponent),
        title: 'Dato no encontrado'
    }
];
