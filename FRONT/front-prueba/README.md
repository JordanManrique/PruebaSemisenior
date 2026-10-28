# Recepción de muestras — Frontend

Aplicación Angular para la prueba de ascenso a Desarrollador Semi Senior. Permite listar las muestras del laboratorio con paginación y registrar nuevas muestras. Consume la API .NET de `BACK/back_prueba`.

| Ruta | Pantalla |
|---|---|
| `/` | Redirige a `/samples` |
| `/samples` | Listado de muestras con paginación |
| `/samples/new` | Formulario de registro de muestra |
| `/form-samples` | Ruta antigua; redirige a `/samples/new` |
| `**` | Página "Dato no encontrado" |

## Requisitos previos

| Herramienta | Versión |
|---|---|
| Node.js | 18.19+ (probado con 18.20.8) |
| npm | 9+ (incluido con Node.js) |
| Angular CLI | 17.3.x (`npm install -g @angular/cli@17`) |
| Backend | API de `BACK/back_prueba` corriendo en `http://localhost:5159` (requiere .NET SDK 10 y SQL Server 2019+; ver el README de la raíz) |

## Pasos para ejecutar

1. **Base de datos y backend.** Crear la base, configurar la cadena de conexión y levantar la API siguiendo el [README de la raíz](../../README.md). En resumen:

   ```sql
   CREATE DATABASE Muestras;
   -- luego ejecutar SQL/schema.sql sobre Muestras
   ```

   ```bash
   dotnet run --project BACK/back_prueba/back_prueba --launch-profile http
   ```

   El backend permite por CORS el origen `http://localhost:4200` (`Cors:AllowedOrigins`).

2. **Instalar dependencias del frontend** (desde `FRONT/front-prueba`):

   ```bash
   npm install
   ```

3. **Configurar la URL de la API** (solo si el backend no corre en el puerto por defecto). Editar [src/environments/environment.ts](src/environments/environment.ts):

   ```ts
   export const environment = {
     apiUrl: 'http://localhost:5159/api'
   };
   ```

4. **Ejecutar en modo desarrollo**:

   ```bash
   npm start
   ```

   Abrir `http://localhost:4200/`.

### Otros comandos

| Comando | Qué hace |
|---|---|
| `npm run build` | Compila a `dist/front-prueba` (browser + server, con prerender) |
| `npm run serve:ssr:front-prueba` | Sirve la versión compilada con SSR (Express) en `http://localhost:4000` |
| `npm test` | Pruebas unitarias con Karma/Jasmine |

## Estructura

```
src/
├── environments/environment.ts        → URL base de la API (apiUrl)
└── app/
    ├── app.config.ts                  → router, HttpClient, animaciones y opciones de Material
    ├── app.routes.ts                  → rutas raíz (lazy loading)
    ├── core/
    │   ├── models/sample.models.ts    → interfaces de solicitudes y respuestas de la API
    │   └── services/sample.service.ts → única clase que usa HttpClient
    ├── features/
    │   ├── samples.route.ts           → rutas /samples y /samples/new
    │   └── pages/
    │       ├── samples/               → listado con tabla y paginación
    │       └── form-samples/          → formulario reactivo de registro
    └── shared/
        ├── validators/                → notFutureDateValidator, uniqueValuesValidator
        └── dato_no_encotnrado/        → página 404
```

## Funcionalidades

### Listado de muestras (`/samples`)

- Tabla (`mat-table`) con número de documento, paciente, tipo de muestra, fecha de toma, estado y cantidad de exámenes.
- Botones **Anterior** y **Siguiente**. Se deshabilitan cuando no hay página anterior (`page <= 1`) o siguiente (`page >= totalPages`), y también mientras se carga.
- Spinner con el texto "Cargando muestras..." mientras llega la respuesta.
- Muestra la página actual, el total de páginas y el total de muestras.
- Si la API falla o no está disponible, muestra un mensaje y un botón **Reintentar**. Si no hay registros, lo indica.

### Formulario de registro (`/samples/new`)

Construido con Reactive Forms (`FormBuilder.nonNullable`):

| Campo | Validaciones |
|---|---|
| Número de documento | Obligatorio, solo dígitos, entre 6 y 12 caracteres |
| Nombre del paciente | Obligatorio, mínimo 3 y máximo 150 caracteres |
| Tipo de muestra | Obligatorio, seleccionado de una lista (`Sangre`, `Orina`, `Heces`, `Hisopado`) |
| Fecha y hora de toma | Obligatoria y no futura (validador personalizado `notFutureDateValidator`) |
| Códigos de exámenes | `FormArray` dinámico para agregar y quitar filas; mínimo 1 código, cada uno obligatorio, sin repetidos (`uniqueValuesValidator`) |

- Cada error se muestra debajo de su campo con `mat-error`.
- El botón **Registrar** se deshabilita si el formulario es inválido o mientras se envía (muestra un spinner).
- Al registrar con éxito navega a `/samples` y muestra un `MatSnackBar` con "Muestra registrada correctamente.".

### Comunicación con la API

- Todas las llamadas HTTP están en `SampleService` (`getPage`, `create`); los componentes no usan `HttpClient`.
- Los contratos están tipados en `sample.models.ts` (`SampleListItem`, `SampleDetail`, `PagedResponse<T>`, `CreateSampleRequest`, `ErrorResponse`).
- La URL base se lee de `environment.apiUrl`; no está escrita en componentes ni servicios.

### Manejo de errores

- **400 al registrar**: se muestran todos los mensajes de `errors` que devuelve el backend (por ejemplo, códigos de examen inexistentes).
- **Otros errores con cuerpo** (409, 500…): se muestra el `message` del backend.
- **API no disponible**: se muestra un mensaje genérico para el usuario.
- Los indicadores de carga se apagan con `finalize`, así que ningún error deja el spinner activo ni la pantalla en blanco.

## Decisiones y supuestos

- **Paginación**: el listado pide `page = 1` y `pageSize = 10`, iguales a los valores por defecto del backend. El tamaño de página es fijo en el front.
- **Componentes standalone y lazy loading**: no hay `NgModule`; cada página se carga bajo demanda con `loadComponent`.
- **Signals** para el estado de las pantallas (carga, errores, página actual), en lugar de variables sueltas.
- **Nueva sintaxis de control de flujo** (`@if`, `@for`) de Angular 17.
- **SSR**: el proyecto se generó con `--ssr`. El listado solo consulta la API en el navegador (`isPlatformBrowser`) para que el prerender no intente llamar al backend.
- **Fecha de toma**: se usa `<input type="datetime-local">` con `max` en la hora actual. El valor se envía como `yyyy-MM-ddTHH:mm` en hora local, el mismo formato que espera el backend (ISO 8601 sin zona).
- **Normalización**: el nombre se envía sin espacios al inicio y al final, y los códigos de examen en mayúsculas y sin espacios. Los repetidos se detectan sin distinguir mayúsculas.
- **Mínimo un código**: no se puede quitar la última fila de exámenes (el botón queda deshabilitado).
- **Campos de Material**: todos los `mat-form-field` usan `appearance: 'outline'` y la etiqueta siempre arriba (`MAT_FORM_FIELD_DEFAULT_OPTIONS`), para que no se monte sobre el placeholder.
- **Ruta 404**: cualquier ruta desconocida muestra "Dato no encontrado" con un botón para volver al listado.

## Paquetes npm adicionales

Además de los que genera `ng new` (Angular, RxJS, Zone.js, Karma/Jasmine):

| Paquete | Motivo |
|---|---|
| `@angular/material` | Componentes de UI (tabla, campos de formulario, select, botones, spinner, snackbar, toolbar) con estilos consistentes y accesibles |
| `@angular/cdk` | Dependencia requerida por Angular Material |
| `@angular/ssr`, `@angular/platform-server`, `express` | Agregados por `ng new --ssr` para el renderizado del lado del servidor |

## Pendiente o conocido como incompleto

- **Cambio de estado desde el front**: la prueba no lo exige en Angular y no está implementado en la interfaz. El tipo `updateSampleStatusRequest` ya existe en los modelos, pero falta el método en `SampleService` y la pantalla. El endpoint `PATCH /api/samples/{id}/status` se prueba desde Swagger.
- **Pruebas unitarias**: solo existe el spec por defecto de `AppComponent`; no hay pruebas de los componentes, del servicio ni de los validadores.
- **Entorno de producción**: solo hay un `environment.ts`; no hay `fileReplacements` para otra URL de API.
