/* =====================================================================
   CitaMedica — Catálogos y Seguridad
   Siembra de Especialidades, Médicos, su relación y Pacientes.

   Base de datos: ClinicaDB  (ya no CatalogosDB)

   Las tablas YA EXISTEN en el esquema del ingeniero. Este script solo
   inserta datos y es idempotente: inserta cada registro únicamente si
   su Id no existe, así que no pisa lo que ya había (Ana García sigue
   siendo el paciente 1, que es a quien apuntan las citas actuales).
   ===================================================================== */

USE ClinicaDB;
GO

/* ---------- 1. Especialidades ---------- */
IF OBJECTPROPERTY(OBJECT_ID('dbo.Especialidades'), 'TableHasIdentity') = 1
    SET IDENTITY_INSERT dbo.Especialidades ON;

INSERT INTO dbo.Especialidades (EspecialidadID, Codigo, Nombre, Descripcion, Activo)
SELECT v.EspecialidadID, v.Codigo, v.Nombre, v.Descripcion, v.Activo
FROM (VALUES
    (1, 'ESP-MGE', N'Medicina General', N'Consulta general y control de rutina', CAST(1 AS bit)),
    (2, 'ESP-PED', N'Pediatría',        N'Atención de niños y adolescentes',     CAST(1 AS bit)),
    (3, 'ESP-GIN', N'Ginecología',      N'Salud de la mujer',                    CAST(1 AS bit)),
    (4, 'ESP-CAR', N'Cardiología',      N'Corazón y sistema circulatorio',       CAST(1 AS bit)),
    (5, 'ESP-DER', N'Dermatología',     N'Piel, cabello y uñas',                 CAST(1 AS bit))
) AS v (EspecialidadID, Codigo, Nombre, Descripcion, Activo)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Especialidades e WHERE e.EspecialidadID = v.EspecialidadID);

IF OBJECTPROPERTY(OBJECT_ID('dbo.Especialidades'), 'TableHasIdentity') = 1
    SET IDENTITY_INSERT dbo.Especialidades OFF;
GO

/* ---------- 2. Médicos ----------
   El médico 1 ya existe (es a quien apuntan las citas): no se toca. */
IF OBJECTPROPERTY(OBJECT_ID('dbo.Medicos'), 'TableHasIdentity') = 1
    SET IDENTITY_INSERT dbo.Medicos ON;

INSERT INTO dbo.Medicos (MedicoID, CodigoMedico, Nombres, Apellidos, NumeroLicencia, Telefono, Email, Activo)
SELECT v.MedicoID, v.CodigoMedico, v.Nombres, v.Apellidos, v.NumeroLicencia, v.Telefono, v.Email, v.Activo
FROM (VALUES
    (2, 'MED-002', N'Ana Beatriz',  N'Portillo Guzmán',  'JVPM-10987', '2250-1101', 'aportillo@clinica.sv', CAST(1 AS bit)),
    (3, 'MED-003', N'José Ricardo', N'Hernández Mejía',  'JVPM-11455', '2250-1102', 'jhernandez@clinica.sv', CAST(1 AS bit)),
    (4, 'MED-004', N'Sofía Elena',  N'Cruz Alvarado',    'JVPM-11890', '2250-1103', 'scruz@clinica.sv',     CAST(1 AS bit)),
    (5, 'MED-005', N'Mario Antonio',N'Escobar Linares',  'JVPM-12044', '2250-1104', 'mescobar@clinica.sv',  CAST(1 AS bit))
) AS v (MedicoID, CodigoMedico, Nombres, Apellidos, NumeroLicencia, Telefono, Email, Activo)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Medicos m WHERE m.MedicoID = v.MedicoID);

IF OBJECTPROPERTY(OBJECT_ID('dbo.Medicos'), 'TableHasIdentity') = 1
    SET IDENTITY_INSERT dbo.Medicos OFF;
GO

/* ---------- 3. Medico_Especialidad ----------
   Un médico puede tener varias; una va marcada como principal. */
INSERT INTO dbo.Medico_Especialidad (MedicoID, EspecialidadID, Principal)
SELECT v.MedicoID, v.EspecialidadID, v.Principal
FROM (VALUES
    (1, 1, CAST(1 AS bit)),                      -- el médico que ya existía: Medicina General
    (2, 2, CAST(1 AS bit)),                      -- Pediatría
    (3, 4, CAST(1 AS bit)),                      -- Cardiología
    (3, 1, CAST(0 AS bit)),                      -- y además Medicina General
    (4, 3, CAST(1 AS bit)),                      -- Ginecología
    (5, 5, CAST(1 AS bit))                       -- Dermatología
) AS v (MedicoID, EspecialidadID, Principal)
WHERE EXISTS (SELECT 1 FROM dbo.Medicos m WHERE m.MedicoID = v.MedicoID)
  AND EXISTS (SELECT 1 FROM dbo.Especialidades e WHERE e.EspecialidadID = v.EspecialidadID)
  AND NOT EXISTS (SELECT 1 FROM dbo.Medico_Especialidad me
                  WHERE me.MedicoID = v.MedicoID AND me.EspecialidadID = v.EspecialidadID);
GO

/* ---------- 4. Pacientes ----------
   El paciente 1 (Ana García) ya existe y no se toca. */
IF OBJECTPROPERTY(OBJECT_ID('dbo.Pacientes'), 'TableHasIdentity') = 1
    SET IDENTITY_INSERT dbo.Pacientes ON;

INSERT INTO dbo.Pacientes
    (PacienteID, CodigoPaciente, TipoDocumento, NumeroDocumento, Nombres, Apellidos,
     FechaNacimiento, Sexo, EstadoCivil, Telefono, Email, Direccion, Ciudad, Pais,
     Ocupacion, TipoSangre, Activo, FechaRegistro)
SELECT v.PacienteID, v.CodigoPaciente, v.TipoDocumento, v.NumeroDocumento, v.Nombres, v.Apellidos,
       v.FechaNacimiento, v.Sexo, v.EstadoCivil, v.Telefono, v.Email, v.Direccion, v.Ciudad, v.Pais,
       v.Ocupacion, v.TipoSangre, v.Activo, SYSDATETIME()
FROM (VALUES
    (2, 'P002', 'DUI', '03218765-4', N'Juan Carlos',    N'Martínez Lemus',   CAST('1985-11-02' AS date), 'M', 'Casado',  '7712-9087', 'jcmartinez@correo.com', N'Soyapango, San Salvador',   N'San Salvador', N'El Salvador', N'Contador',     'O+',  CAST(1 AS bit)),
    (3, 'P003', 'DUI', '05543210-9', N'Rosa Elena',     N'Flores Campos',    CAST('1978-07-25' AS date), 'F', 'Casada',  '7033-4512', 'reflores@correo.com',   N'Santa Tecla, La Libertad',  N'Santa Tecla',  N'El Salvador', N'Docente',      'A+',  CAST(1 AS bit)),
    (4, 'P004', 'DUI', '02987654-1', N'Luis Alberto',   N'Ramírez Peña',     CAST('2001-01-30' AS date), 'M', 'Soltero', '7890-1123', 'laramirez@correo.com',  N'Mejicanos, San Salvador',   N'Mejicanos',    N'El Salvador', N'Estudiante',   'B+',  CAST(1 AS bit)),
    (5, 'P005', 'DUI', '06112233-5', N'Carmen Lidia',   N'Sánchez Mendoza',  CAST('1966-09-08' AS date), 'F', 'Viuda',   '7456-7788', 'clsanchez@correo.com',  N'San Miguel',                N'San Miguel',   N'El Salvador', N'Comerciante',  'O-',  CAST(1 AS bit)),
    (6, 'P006', 'DUI', '07334455-6', N'Óscar Mauricio', N'Rivera Call',      CAST('1995-05-19' AS date), 'M', 'Soltero', '6023-9900', 'omrivera@correo.com',   N'Santa Ana',                 N'Santa Ana',    N'El Salvador', N'Mecánico',     'AB+', CAST(1 AS bit))
) AS v (PacienteID, CodigoPaciente, TipoDocumento, NumeroDocumento, Nombres, Apellidos,
        FechaNacimiento, Sexo, EstadoCivil, Telefono, Email, Direccion, Ciudad, Pais,
        Ocupacion, TipoSangre, Activo)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Pacientes p WHERE p.PacienteID = v.PacienteID);

IF OBJECTPROPERTY(OBJECT_ID('dbo.Pacientes'), 'TableHasIdentity') = 1
    SET IDENTITY_INSERT dbo.Pacientes OFF;
GO

/* ---------- 5. Verificación ---------- */
SELECT 'Especialidades' AS Tabla, COUNT(*) AS Filas FROM dbo.Especialidades
UNION ALL SELECT 'Medicos',             COUNT(*) FROM dbo.Medicos
UNION ALL SELECT 'Medico_Especialidad', COUNT(*) FROM dbo.Medico_Especialidad
UNION ALL SELECT 'Pacientes',           COUNT(*) FROM dbo.Pacientes;

SELECT m.MedicoID, m.CodigoMedico, m.Nombres + ' ' + m.Apellidos AS Medico,
       e.Nombre AS Especialidad, me.Principal
FROM dbo.Medicos m
LEFT JOIN dbo.Medico_Especialidad me ON me.MedicoID = m.MedicoID
LEFT JOIN dbo.Especialidades e ON e.EspecialidadID = me.EspecialidadID
ORDER BY m.MedicoID, me.Principal DESC;
GO
