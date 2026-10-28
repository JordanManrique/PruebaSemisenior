# Recepción de muestras

Aplicación para registrar muestras de pacientes, listarlas y controlar su estado.

| Carpeta | Contenido |
|---|---|
| `BACK/back_prueba` | API ASP.NET Core (.NET 10) + EF Core + SQL Server |
| `FRONT/front-prueba` | Aplicación Angular 17 |
| `database/schema.sql` | Crea tablas, restricciones, índices y catálogo de exámenes sobre una BD vacía |
| `database/queries.sql` | Consultas de reporte solicitadas en la sección de base de datos |

## Requisitos previos

- .NET SDK 10 (o superior, compila `net10.0`)
- SQL Server 2019+ (o LocalDB / Express)
- Node.js 18+ y Angular CLI 17 (frontend)

## Base de datos

```sql
CREATE DATABASE Muestras;
```

Luego ejecutar `database/schema.sql` sobre `Muestras`. Las consultas de `database/queries.sql` se pueden ejecutar después sobre la misma base.

### Consultas (`database/queries.sql`)

1. **Muestras por estado del mes en curso**: filtra por el rango `[primer día del mes, primer día del mes siguiente)` en lugar de usar `MONTH()`/`YEAR()` sobre la columna, para que SQL Server pueda usar `IX_Samples_CreatedAt`.
2. **Solicitudes por examen**: `LEFT JOIN` desde `Tests` y `COUNT(st.SampleId)`, así los exámenes nunca solicitados aparecen con 0. Se ordena de mayor a menor cantidad.
3. **Pacientes con 2 o más rechazos**: `GROUP BY` paciente con `HAVING COUNT(*) >= 2` sobre las muestras en estado `Rechazada`.

### Índices

| Índice | Por qué |
|---|---|
| `IX_Samples_CreatedAt (CreatedAt DESC, Id DESC) INCLUDE (Status)` | El listado ordena por `CreatedAt DESC` y pagina con `OFFSET/FETCH`; también apoya la consulta 1 |
| `IX_Samples_PatientId INCLUDE (Status)` | Join con `Patients` en el listado y conteo de rechazos de la consulta 3 |
| `IX_SampleTests_TestId` | La PK de `SampleTests` empieza por `SampleId`; este índice apoya el conteo por examen de la consulta 2 |

## Configuración de la cadena de conexión

La cadena **no está en el código**. Se lee de la configuración con la clave `ConnectionStrings:LabDb` (`Program.cs`).

Valor por defecto en `appsettings.json` (autenticación de Windows, sin contraseña):

```json
"ConnectionStrings": {
  "LabDb": "Server=localhost;Database=Muestras;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Si tu servidor usa usuario y contraseña, no la guardes en el repositorio. Usa *user-secrets* o una variable de entorno, que tienen prioridad sobre `appsettings.json`:

```bash
dotnet user-secrets init --project BACK/back_prueba/back_prueba
dotnet user-secrets set "ConnectionStrings:LabDb" "Server=localhost;Database=Muestras;User Id=sa;Password=***;TrustServerCertificate=True;" --project BACK/back_prueba/back_prueba
```

o la variable de entorno `ConnectionStrings__LabDb`.

Los orígenes permitidos por CORS también están en configuración (`Cors:AllowedOrigins`, por defecto `http://localhost:4200`).

## Ejecutar el backend

```bash
dotnet run --project BACK/back_prueba/back_prueba --launch-profile http
```

- API: `http://localhost:5159`
- Swagger UI: `http://localhost:5159/swagger` (los 3 endpoints se pueden probar desde allí)
- OpenAPI JSON: `http://localhost:5159/swagger/v1/swagger.json` (sirve para generar tipos en el front)

---

## Contrato de la API (para el frontend)

URL base sugerida para `environment.ts`: `http://localhost:5159/api`

Todas las fechas viajan en ISO 8601 sin zona (`2026-10-28T08:30:00`) y se interpretan como hora local del servidor.

### Estructura de error (todas las respuestas 4xx/5xx)

```json
{
  "status": 400,
  "message": "La solicitud tiene errores de validación.",
  "errors": {
    "DocumentNumber": ["El número de documento debe tener solo dígitos y entre 6 y 12 caracteres."],
    "TestCodes": ["La lista de exámenes tiene códigos repetidos."]
  }
}
```

- `errors` solo aparece en 400 y agrupa los mensajes por campo. La clave es el nombre de la propiedad en PascalCase (`DocumentNumber`, `FullName`, `SampleType`, `CollectedAt`, `TestCodes`, `Status`, `RejectionReason`, `Page`, `PageSize`).
- En 404, 409 y 500 solo vienen `status` y `message`. El 500 siempre trae un mensaje genérico; el detalle queda en el log del servidor.

```ts
export interface ApiError {
  status: number;
  message: string;
  errors?: Record<string, string[]>;
}
```

### 1. Registrar muestra — `POST /api/samples`

**Body**

```json
{
  "documentNumber": "1001006",
  "fullName": "Pedro Pérez",
  "sampleType": "Sangre",
  "collectedAt": "2026-10-28T08:30:00",
  "testCodes": ["HEM01", "GLU02"]
}
```

| Campo | Regla |
|---|---|
| `documentNumber` | Obligatorio, solo dígitos, 6 a 12 caracteres |
| `fullName` | Obligatorio, mínimo 3 caracteres (máx. 150) |
| `sampleType` | Obligatorio: `Sangre`, `Orina`, `Heces` o `Hisopado` |
| `collectedAt` | Obligatorio, no puede ser futura |
| `testCodes` | Mínimo 1, sin repetidos, todos deben existir en el catálogo |

**Respuestas**

- `201 Created`: devuelve `SampleResponse` y el header `Location: /api/samples/{id}`.
- `400`: errores de formato, o códigos de examen inexistentes. En ese caso viene `errors.TestCodes`, por ejemplo `["El examen 'XYZ99' no existe."]`.

```json
{
  "id": 6,
  "documentNumber": "1001006",
  "patientName": "Pedro Pérez",
  "sampleType": "Sangre",
  "collectedAt": "2026-10-28T08:30:00",
  "status": "Registrada",
  "rejectionReason": null,
  "createdAt": "2026-10-28T09:12:44.123",
  "tests": [
    { "code": "GLU02", "name": "Glucosa en Ayunas" },
    { "code": "HEM01", "name": "Hemograma Completo" }
  ]
}
```

### 2. Listar muestras — `GET /api/samples?page=1&pageSize=10`

| Parámetro | Por defecto | Regla |
|---|---|---|
| `page` | `1` | ≥ 1 |
| `pageSize` | `10` | 1 a 50 |

Se ordena por fecha de creación, de la más reciente a la más antigua. La paginación se resuelve en SQL Server (`OFFSET/FETCH`).

**`200 OK`**

```json
{
  "items": [
    {
      "id": 6,
      "documentNumber": "1001006",
      "patientName": "Pedro Pérez",
      "sampleType": "Sangre",
      "collectedAt": "2026-10-28T08:30:00",
      "status": "Registrada",
      "testCount": 2
    }
  ],
  "totalCount": 6,
  "page": 1,
  "pageSize": 10,
  "totalPages": 1
}
```

Para los botones del front: *Anterior* se deshabilita si `page <= 1` y *Siguiente* si `page >= totalPages`.

`400` si `page` o `pageSize` están fuera de rango.

### 3. Cambiar estado — `PATCH /api/samples/{id}/status`

```json
{ "status": "Rechazada", "rejectionReason": "Muestra hemolizada" }
```

Transiciones permitidas:

| Estado actual | Puede pasar a |
|---|---|
| Registrada | Recibida, Rechazada |
| Recibida | Procesada, Rechazada |
| Procesada | — (final) |
| Rechazada | — (final) |

| Código | Cuándo |
|---|---|
| `200` | Cambio exitoso; devuelve `SampleResponse` |
| `400` | Estado inválido, o `Rechazada` sin `rejectionReason` |
| `404` | La muestra no existe |
| `409` | Transición no permitida, p. ej. `"No se permite cambiar la muestra del estado 'Procesada' al estado 'Recibida'."` |

### Tipos TypeScript sugeridos

```ts
export type SampleType = 'Sangre' | 'Orina' | 'Heces' | 'Hisopado';
export type SampleStatus = 'Registrada' | 'Recibida' | 'Procesada' | 'Rechazada';

export interface CreateSampleRequest {
  documentNumber: string;
  fullName: string;
  sampleType: SampleType;
  collectedAt: string; // ISO 8601
  testCodes: string[];
}

export interface UpdateSampleStatusRequest {
  status: SampleStatus;
  rejectionReason?: string | null;
}

export interface TestResponse { code: string; name: string; }

export interface SampleResponse {
  id: number;
  documentNumber: string;
  patientName: string;
  sampleType: SampleType;
  collectedAt: string;
  status: SampleStatus;
  rejectionReason: string | null;
  createdAt: string;
  tests: TestResponse[];
}

export interface SampleListItem {
  id: number;
  documentNumber: string;
  patientName: string;
  sampleType: SampleType;
  collectedAt: string;
  status: SampleStatus;
  testCount: number;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
```

---

## Arquitectura del backend

```
Controllers/   SamplesController      → recibe la solicitud y devuelve la respuesta (sin lógica)
Services/      SampleService          → reglas de negocio (catálogo, paciente nuevo/existente, transiciones)
Repositories/  SampleRepository       → consultas y persistencia con EF Core
Data/          LabDbContext           → mapeo de entidades a las tablas de schema.sql
Entities/                             → entidades de BD (no se exponen en la API)
Dtos/                                 → contratos de entrada/salida
Middleware/    RequestLoggingMiddleware → log por solicitud: método, ruta, código y ms
               GlobalExceptionHandler   → manejo centralizado de errores (IExceptionHandler)
Validation/                           → validadores personalizados (fecha no futura, ítems únicos)
```

## Decisiones y supuestos

- **EF Core** como acceso a datos. El esquema lo crea `schema.sql` (no se usan migraciones de EF), así que el `DbContext` solo mapea.
- **Paginación por defecto**: `page = 1`, `pageSize = 10`. Valores fuera de rango devuelven 400 (no se corrigen en silencio).
- **Paciente existente**: si el documento ya existe se reutiliza el paciente y se conserva su nombre registrado. El nombre enviado en la solicitud no lo sobrescribe.
- **Códigos de examen**: se normalizan a mayúsculas y sin espacios. Los repetidos se detectan sin distinguir mayúsculas.
- **Atomicidad**: paciente, muestra y exámenes se guardan en una transacción explícita.
- **Concurrencia en cambio de estado**: el `UPDATE` incluye `WHERE Status = <estado leído>`. Si otra solicitud cambió la muestra en medio, no se pisa el cambio y se responde 409.
- **Motivo de rechazo**: solo se guarda cuando el nuevo estado es `Rechazada`; en otros estados se ignora.
- **Fechas**: `collectedAt` se compara con la hora local del servidor para validar que no sea futura.
- **Swagger** queda habilitado en todos los entornos para poder probar la API durante la sustentación.

## Paquetes NuGet adicionales

| Paquete | Motivo |
|---|---|
| `Microsoft.EntityFrameworkCore.SqlServer` | ORM y proveedor de SQL Server (paginación en BD, transacciones, `ExecuteUpdate`) |
| `Swashbuckle.AspNetCore` | Swagger UI para probar y documentar los endpoints con los comentarios XML |
| `System.Security.Cryptography.Pkcs` | Fija una versión parcheada; la transitiva de `Microsoft.Data.SqlClient` tenía vulnerabilidades conocidas |
| `Microsoft.Data.SqlClient` | Driver de SQL Server (ya estaba en el proyecto) |

## Pendiente

- Ninguno conocido.
