# Prueba técnica: API REST en .NET 10

API REST con **Minimal API** que expone un CRUD de usuarios, sus direcciones (relación 1:N) y un módulo de
conversión de divisas. Usa **Entity Framework Core + SQLite**, **FluentValidation**, el patrón **CQRS**
(implementado a mano, sin MediatR) y seguridad por **API Key**.

## Stack

| | |
|---|---|
| Runtime | **.NET 10** (`net10.0`) |
| Web | ASP.NET Core Minimal API |
| Persistencia | Entity Framework Core 10 + SQLite |
| Validación | FluentValidation 12 |
| Documentación | OpenAPI nativo de .NET 10 + Swagger UI |
| Tests | xUnit + `WebApplicationFactory` |

## Requisitos

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Opcional, solo para crear migraciones nuevas: `dotnet tool install --global dotnet-ef --version 10.*`

## Cómo correr el proyecto

```bash
git clone https://github.com/oscarromero-7/prueba-tecnica.git
cd prueba-tecnica
dotnet run
```

- API: `http://localhost:5201`
- Swagger UI: `http://localhost:5201/swagger`. Para probar desde ahí, usar el botón **Authorize** y cargar la API Key.
- Health check: `http://localhost:5201/health`

La base de datos (`app.db`) se crea sola al arrancar. No hace falta ningún paso previo.

## Base de datos y migraciones

Al iniciar, la API ejecuta `Database.Migrate()`, que crea `app.db` y aplica las migraciones pendientes,
incluidas las **monedas semilla**:

| Code | Name | RateToBase |
|---|---|---|
| PYG | Guaraní paraguayo | 1 (moneda base) |
| USD | Dólar estadounidense | 7300 |
| EUR | Euro | 7900 |
| BRL | Real brasileño | 1300 |

Las migraciones están en `Infrastructure/Migrations`. Para trabajar con ellas manualmente:

```bash
# Crear una migración nueva después de cambiar el modelo
dotnet ef migrations add <NombreMigracion> -o Infrastructure/Migrations

# Aplicar las migraciones sin levantar la API
dotnet ef database update

# Empezar de cero: borrar la base y volver a levantar la API
rm app.db && dotnet run
```

## Autenticación: API Key

Todos los endpoints exigen el header **`X-API-KEY`**. El valor se configura en `appsettings.json`:

```json
"ApiKey": "prueba-tecnica-api-key-123"
```

**API Key de prueba:** `prueba-tecnica-api-key-123`

- Si el header falta o el valor es incorrecto, la respuesta es **401 Unauthorized**.
- Son públicos, sin key: **`/swagger`**, **`/openapi/v1.json`** y **`/health`**, para poder ver la documentación
  y monitorear el servicio.
- El valor se puede cambiar sin tocar el archivo, con la variable de entorno `ApiKey`.

### Ejemplos con curl

> En Windows PowerShell usar `curl.exe`, porque `curl` es un alias de `Invoke-WebRequest`.

```bash
# Sin API Key -> 401
curl -i http://localhost:5201/users

# Crear usuario -> 201 + Location
curl -i -X POST http://localhost:5201/users \
  -H "X-API-KEY: prueba-tecnica-api-key-123" \
  -H "Content-Type: application/json" \
  -d '{"name":"Juan","email":"juan@test.com"}'

# Listar usuarios activos
curl http://localhost:5201/users?isActive=true \
  -H "X-API-KEY: prueba-tecnica-api-key-123"

# Crear dirección
curl -i -X POST http://localhost:5201/users/1/addresses \
  -H "X-API-KEY: prueba-tecnica-api-key-123" \
  -H "Content-Type: application/json" \
  -d '{"street":"Calle Falsa 123","city":"Asunción","country":"Paraguay","zipCode":"9999"}'

# Convertir 100 USD a PYG -> {"fromCurrency":"USD","toCurrency":"PYG","originalAmount":100,"convertedAmount":730000}
curl -X POST http://localhost:5201/currency/convert \
  -H "X-API-KEY: prueba-tecnica-api-key-123" \
  -H "Content-Type: application/json" \
  -d '{"fromCurrencyCode":"USD","toCurrencyCode":"PYG","amount":100}'
```

El archivo [`PruebaTecnica.http`](PruebaTecnica.http) tiene ejemplos de **todas** las llamadas, incluidos los casos
de error. Se puede ejecutar desde VS Code (extensión REST Client), Visual Studio o Rider.

## Endpoints

| Método | Ruta | Descripción | Respuestas |
|---|---|---|---|
| POST | `/users` | Crear usuario | 201, 400, 409 |
| GET | `/users?isActive=true\|false` | Listar usuarios (filtro opcional) | 200 |
| GET | `/users/{id}` | Obtener usuario | 200, 404 |
| PUT | `/users/{id}` | Modificar name, email e isActive | 200, 400, 404, 409 |
| DELETE | `/users/{id}` | Eliminar usuario y sus direcciones | 204, 404 |
| POST | `/users/{userId}/addresses` | Crear dirección | 201, 400, 404 |
| GET | `/users/{userId}/addresses` | Listar direcciones del usuario | 200, 404 |
| GET | `/addresses/{id}` | Obtener dirección *(extra)* | 200, 404 |
| PUT | `/addresses/{id}` | Modificar dirección | 200, 400, 404 |
| DELETE | `/addresses/{id}` | Eliminar dirección | 204, 404 |
| GET | `/currencies` | Listar monedas | 200 |
| GET | `/currencies/{code}` | Obtener moneda *(extra)* | 200, 404 |
| POST | `/currencies` | Crear moneda | 201, 400, 409 |
| POST | `/currency/convert` | Convertir un monto entre monedas | 200, 400, 404 |
| GET | `/health` | Estado de la API y de la BD *(extra, público)* | 200, 503 |

Además, cualquier endpoint protegido responde **401** sin API Key válida.

Todos los errores se devuelven como **ProblemDetails** (RFC 9110). Los de validación incluyen el detalle por campo:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["'Name' no debería estar vacío."],
    "Email": ["'Email' no es una dirección de correo electrónico válida."]
  }
}
```

## Tests

```bash
dotnet test tests/PruebaTecnica.Tests
```

Son **40 tests de integración**. Cada clase de tests levanta la API en memoria con `WebApplicationFactory`, usando
una base SQLite temporal propia (no toca `app.db`) y un `FakeTimeProvider` para controlar las fechas de auditoría.
Cubren la API Key, los CRUD con sus casos de error (400/404/409), el email único al crear y al editar, el borrado
en cascada y la fórmula de conversión.

El workflow de **GitHub Actions** (`.github/workflows/ci.yml`) compila y corre los tests en cada push.

## Estructura

```
Application/
  Users/            Commands/ (Create, Update, Delete) · Queries/ (GetUsers, GetUserById) · UserResponse
  Addresses/        Commands/ · Queries/ · AddressResponse
  Currencies/       Commands/ (CreateCurrency) · Queries/ (GetCurrencies, GetCurrencyByCode)
  CurrencyConversion/  ConvertCurrency
  DependencyInjection.cs   registro de validadores y handlers
Common/
  Cqrs/             ICommandHandler, IQueryHandler
  Validation/       ValidationFilter<T> (FluentValidation para Minimal API)
  Middleware/       ApiKeyMiddleware
  Exceptions/       NotFound / Conflict + GlobalExceptionHandler
  OpenApi/          esquema de seguridad de la API Key para Swagger
Domain/             User, Address, Currency, IAuditable
Endpoints/          mapeo de rutas por recurso
Infrastructure/     AppDbContext, AuditInterceptor, Migrations/
tests/              PruebaTecnica.Tests
```

Cada archivo de `Application` agrupa un caso de uso completo: el command o query, su validador y su handler.

## Decisiones tomadas

**CQRS sin MediatR.** MediatR tiene licencia comercial, así que se usan dos interfaces propias
(`ICommandHandler<TCommand, TResult>` e `IQueryHandler<TQuery, TResult>`) registradas explícitamente en DI.
Los endpoints reciben el handler por inyección y lo invocan directamente.

**Validación con un endpoint filter.** `AddFluentValidationAutoValidation()` no funciona en Minimal API, por lo que
`ValidationFilter<T>` ejecuta el validador del body y devuelve `400` con `Results.ValidationProblem(...)`.
Se aplica con `.WithValidation<T>()`. Los largos máximos se validan acá porque SQLite no los hace cumplir.

**Errores de negocio con excepciones.** Los handlers lanzan `NotFoundException` o `ConflictException`, y un
`IExceptionHandler` global las convierte en 404 o 409 con ProblemDetails. Una alternativa válida es un tipo
`Result<T>`; se eligieron excepciones por simplicidad y porque centralizan el formato de error.

**Unicidad de email y código de moneda.**
- El email se guarda en minúsculas y el código de moneda en mayúsculas, así `"USD"` y `"usd"` son el mismo valor.
- Se valida en el handler (409, también al **editar**, excluyendo al propio usuario) y además hay un **índice único**
  en la BD.
- Si dos requests simultáneos pasan la validación, el índice frena al segundo y también se responde 409.

**Password opcional.** El ejemplo del enunciado para `POST /users` no incluye password, así que es opcional. Si se
envía, se guarda hasheado con `PasswordHasher<User>` de ASP.NET Core Identity (PBKDF2 con salt). Las respuestas usan
DTOs, por lo que nunca se devuelve el password ni el hash.

**`PUT /users/{id}` exige `isActive`.** Si fuera un `bool` común, omitirlo lo dejaría en `false` y desactivaría
al usuario sin que nadie lo pidiera.

**Borrado de usuario: físico y en cascada.** Al eliminar un usuario se eliminan sus direcciones mediante
`ON DELETE CASCADE` en la FK. Se eligió sobre un soft delete por simplicidad y porque el enunciado pide "eliminar".
En un sistema real convendría evaluar soft delete por trazabilidad.

**Conversión de divisas.**
- Usa la fórmula del enunciado: `montoBase = amount * from.RateToBase` y `convertedAmount = montoBase / to.RateToBase`.
- Todo el cálculo es en `decimal`.
- Si alguno de los códigos no existe, la respuesta es **404** e indica cuál falta.
- El resultado se redondea a 6 decimales, la misma precisión que `RateToBase`.
- `amount` tiene un tope de 1e12 para que la multiplicación nunca desborde un `decimal`.
- Es una *query* de CQRS (no modifica datos), aunque el endpoint sea POST porque recibe un body.

**Decimal en SQLite.** SQLite no tiene tipo decimal. EF Core guarda `RateToBase` como TEXT, sin perder precisión,
pero SQLite no puede ordenar ni comparar ese valor como número. Por eso las consultas nunca filtran ni ordenan por
`RateToBase` (el listado se ordena por `Code`) y los cálculos se hacen en C#. Un efecto visible: las monedas semilla
se muestran como `1300.0` y las creadas por la API como `7.5`. El valor numérico es el mismo.

**Auditoría.** Las tres tablas tienen `CreatedAt` y `UpdatedAt`, completados por un `SaveChangesInterceptor`, así
ningún handler tiene que acordarse de hacerlo. Las fechas se guardan y devuelven en UTC. La hora se obtiene de
`TimeProvider`, que en los tests se reemplaza por uno falso.

**Endpoints extra.** Se agregaron tres endpoints que el enunciado no pide:
- `GET /addresses/{id}` y `GET /currencies/{code}`, para que el header `Location` del `201 Created` apunte a un
  recurso que realmente existe.
- `GET /health`, como chequeo de estado para monitoreo.

**Simplificaciones propias de una prueba técnica.** En producción se harían de otra forma:
- La API Key está en `appsettings.json`. En producción iría en una variable de entorno o un gestor de secretos.
- Las migraciones se aplican al arrancar. En producción se ejecutarían como un paso del despliegue.
- Swagger UI está habilitado en todos los entornos.
- La API Key se valida con un middleware, como pide el enunciado. La alternativa idiomática en ASP.NET Core sería un
  `AuthenticationHandler`, integrado con `RequireAuthorization()`.
- Todo está en un solo proyecto organizado por carpetas, y los handlers usan el `AppDbContext` directamente (EF Core
  ya cumple el papel de repositorio y unidad de trabajo). En un sistema más grande se separaría en proyectos
  Domain / Application / Infrastructure / Api.

## Qué quedó implementado

- [x] CRUD de Users, con filtro `isActive`
- [x] CRUD de Addresses relacionado a Users (1:N), con 404 si el usuario no existe
- [x] Monedas: listado, alta con código único y conversión con `RateToBase`
- [x] Seguridad por API Key (`X-API-KEY`, 401)
- [x] EF Core + SQLite, con migraciones y datos semilla
- [x] FluentValidation para crear y editar usuario, crear y editar dirección, crear moneda y convertir
- [x] CQRS con Commands / Queries en la estructura de carpetas pedida
- [x] Respuestas 201 + Location, 204, 400, 401, 404 y 409 con ProblemDetails
- [x] Auditoría (`CreatedAt` / `UpdatedAt`)
- [x] Tests de integración y CI con GitHub Actions
- [x] Archivo `.http` con todas las llamadas

**No implementado** (fuera del alcance pedido; posibles mejoras):

- Paginación en los listados
- Concurrencia optimista (token de versión) para evitar que dos PUT simultáneos se pisen
- Rate limiting
- Autenticación por usuario (JWT); hoy la API Key es compartida
- Soft delete
- Dockerfile
