# CitaMedica

Sistema de citas médicas y expediente clínico para una clínica mediana.
Cubre el ciclo completo: recepción agenda la cita, el médico atiende y
diagnostica, y todo queda archivado en el expediente del paciente.

Proyecto de la materia de .NET — Universidad Politécnica de El Salvador.

---

## Qué necesita

| | Versión | Para qué |
|---|---|---|
| **.NET SDK** | 10.0 | Los dos microservicios |
| **SQL Server** | Express o superior | La base `ClinicaDB` |
| **SSMS** | cualquiera | Correr los scripts de siembra |
| **Node.js** | 22.22.3+, 24.15+ o 26 | El frontend |
| **Visual Studio** | 2026 (o `dotnet` por consola) | Abrir `CitaMedica.slnx` |

Angular 22 no arranca con versiones de Node anteriores a las de arriba.
Para verificar: `node --version`.

---

## Cómo levantarlo

El orden importa: primero la base, después los servicios, al final el
frontend.

### 1. La base de datos

El esquema de `ClinicaDB` ya existe — este proyecto **no lo crea**. Los
scripts de `sql/` solo siembran datos sobre las tablas que ya están.

Abrí cada uno en SSMS y ejecutalo completo, **en este orden**:

| Script | Qué siembra |
|---|---|
| `01_Consultorios.sql` | Sucursal, áreas y consultorios |
| `02_Catalogos.sql` | Especialidades, médicos y pacientes |
| `03_Usuarios.sql` | Roles y usuarios del sistema |
| `04_EstadosCita.sql` | Los cinco estados de una cita |
| `05_Diagnosticos.sql` | 38 diagnósticos CIE-10 de consulta general |

Los cinco son **idempotentes**: se pueden correr varias veces sin duplicar
nada, porque cada uno inserta solo lo que falta.

> En SSMS, si hay texto seleccionado se ejecuta **solo lo seleccionado**.
> Hacé clic en cualquier parte del archivo para no dejar nada pintado, y
> después F5.

`SQLQuery1.sql` es una consulta de apoyo para inspeccionar el esquema. No
hace falta correrla.

**Cadena de conexión.** Los dos servicios apuntan a:

```
Server=.\SQLEXPRESS;Database=ClinicaDB;Trusted_Connection=True;TrustServerCertificate=True;
```

Si tu instancia de SQL Server se llama distinto, cambiala en los dos
archivos:

- `src/Api/appsettings.json` → `ConnectionStrings:ClinicaConnection`
- `src/Catalogos/Catalogos.Api/appsettings.json` → `ConnectionStrings:CatalogosConnection`

### 2. Los dos microservicios

Abrí `CitaMedica.slnx` en Visual Studio y configurá **ambos** como proyecto
de inicio: clic derecho en la solución → *Configurar proyectos de inicio* →
*Varios proyectos de inicio* → `Api` y `Catalogos.Api` en **Iniciar**.

Después F5. Se abren dos ventanas de Swagger:

| Servicio | HTTP | HTTPS | Swagger |
|---|---|---|---|
| **Catálogos y Seguridad** | 5110 | 7260 | `https://localhost:7260/swagger` |
| **Agenda y Atención** | 5104 | 7252 | `https://localhost:7252/swagger` |

Por consola, en dos terminales:

```bash
dotnet run --project src/Catalogos/Catalogos.Api
dotnet run --project src/Api
```

### 3. El frontend

Desde la raíz del repositorio:

```bash
cd citamedica-web
npm install
ng serve --open
```

O doble clic a `iniciar-frontend.bat`, que hace lo mismo.

Queda en `http://localhost:4200`.

---

## Usuarios de prueba

Los siembra `03_Usuarios.sql`. **Son cuentas de desarrollo**: antes de
cualquier uso real hay que borrarlas y crear las de verdad.

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `Admin123!` | Administrador |
| `recepcion` | `Recepcion123!` | Recepción |
| `aportillo` | `Medico123!` | Médico |
| `jhernandez` | `Medico123!` | Médico |
| `scruz` | `Medico123!` | Médico |
| `mescobar` | `Medico123!` | Médico |

### Qué puede hacer cada rol

| | Recepción | Médico | Administrador |
|---|---|---|---|
| Ver la agenda | Sí | Sí | Sí |
| Consultar pacientes | Sí | Sí | Sí |
| Dar de alta pacientes | Sí | No | Sí |
| Agendar citas | Sí | No | Sí |
| Atender y diagnosticar | No | Sí | Sí |
| Ver expedientes | No | Sí | Sí |
| Administrar usuarios | No | No | Sí |

El permiso se aplica en el servidor, no solo escondiendo botones: entrar
por la URL directa a una pantalla que no corresponde devuelve 403.

---

## Cómo está organizado

```
Cita_Medica/
├── src/                        Los dos microservicios
│   ├── ClassLibrary1/          Domain — entidades y reglas del negocio
│   ├── Core/                   Casos de uso (CQRS con MediatR), DTOs
│   ├── Persistence/            EF Core y el DbContext de Agenda
│   ├── Infrastructure/         Llamadas al otro microservicio
│   ├── Api/                    Agenda y Atención — controladores
│   ├── Catalogos/              Catálogos y Seguridad — su propia solución
│   │   ├── Catalogos.Domain/
│   │   ├── Catalogos.Core/
│   │   ├── Catalogos.Persistence/
│   │   └── Catalogos.Api/
│   └── GenericPersistence/     Repositorio genérico reutilizable
├── rst/                        Biblioteca REST propia (NugetPackage_Rest)
├── citamedica-web/             Frontend Angular 22
├── sql/                        Scripts de siembra
└── CitaMedica.slnx             La solución
```

### Arquitectura

**Hexagonal, cuatro capas.** `Domain` no conoce a nadie; `Core` define las
interfaces y los casos de uso; `Persistence` e `Infrastructure` las
implementan; `Api` solo expone. Las dependencias apuntan siempre hacia
adentro.

**CQRS con MediatR.** Cada operación es un `Command` o una `Query` con su
handler. Los controladores no tienen lógica: reciben, mandan el mensaje y
devuelven.

**Dos microservicios sobre una base.** *Catálogos y Seguridad* maneja
pacientes, médicos y autenticación. *Agenda y Atención* maneja citas,
consultas, diagnósticos y expedientes. Agenda le pide los nombres a
Catálogos por HTTP, propagando el token del usuario.

Si Catálogos se cae, Agenda sigue respondiendo: donde iría "Ana García"
aparece "Paciente #1". Degrada, no falla.

**Autenticación con JWT.** Catálogos emite el token al iniciar sesión y
Agenda lo valida con la misma clave. Dura 8 horas. Las contraseñas se
guardan como derivado PBKDF2-HMAC-SHA256 con 100 000 iteraciones y sal
propia por usuario.

---

## Pendientes conocidos

Vale la pena decirlos en vez de que alguien los descubra:

- **Código de una evaluación anterior sin quitar.** En `Core` e
  `Infrastructure` quedan `CrearPedidoCommand`, `CobrarPedidoCommand` y una
  pasarela de pagos, con su bloque `Pasarela` en `appsettings.json`. No los
  llama ningún endpoint — son de un ejercicio previo y están identificados
  para borrarse.
- **URLs inventadas** en `Downstream` (`PagosBaseUrl`, `InventarioBaseUrl`,
  `ReportesBaseUrl`): apuntan a servicios que no existen. Solo se usa
  `CatalogosBaseUrl`.
- **La clave JWT está en `appsettings.json`.** Sirve para desarrollo; en
  producción va en variables de entorno o en un gestor de secretos.
- **Los controladores de Catálogos** (Pacientes, Médicos, Especialidades)
  piden sesión iniciada pero todavía no distinguen por rol.
- **`WeatherForecast.cs`** y la carpeta `Migrations/` de
  `Catalogos.Persistence` son restos de las plantillas de Visual Studio.
