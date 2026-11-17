/* =====================================================================
   CitaMedica — Catálogos y Seguridad
   Siembra de Roles, Usuarios y los médicos que faltaban.

   Base de datos: ClinicaDB

   Orden obligado por el esquema:
       Roles -> Usuarios -> Medicos -> Medico_Especialidad

   Por qué Usuarios va antes que Medicos: Medicos.UsuarioID tiene una
   restricción UNIQUE, y en SQL Server una columna UNIQUE que admite nulos
   solo tolera UN nulo en toda la tabla. El médico 1 ya ocupa ese nulo, así
   que todo médico nuevo necesita su propio usuario.

   Contraseñas: PBKDF2-HMAC-SHA256, 100.000 iteraciones, sal de 16 bytes.
   Formato guardado: PBKDF2$iteraciones$sal_base64$hash_base64

   Credenciales de la demo:
       admin       / Admin123!       (Administrador)
       recepcion   / Recepcion123!   (Recepción)
       aportillo   / Medico123!      (Médico)
       jhernandez  / Medico123!      (Médico)
       scruz       / Medico123!      (Médico)
       mescobar    / Medico123!      (Médico)

   Es idempotente: inserta cada registro solo si su Id no existe.
   ===================================================================== */

USE ClinicaDB;
GO

/* ---------- 1. Roles ---------- */
IF OBJECTPROPERTY(OBJECT_ID('dbo.Roles'), 'TableHasIdentity') = 1
    SET IDENTITY_INSERT dbo.Roles ON;

INSERT INTO dbo.Roles (RolID, Nombre, Descripcion, Activo)
SELECT v.RolID, v.Nombre, v.Descripcion, v.Activo
FROM (VALUES
    (1, N'Administrador', N'Acceso total al sistema',                       CAST(1 AS bit)),
    (2, N'Médico',        N'Atiende citas y registra atenciones',           CAST(1 AS bit)),
    (3, N'Recepción',     N'Agenda citas y administra pacientes',           CAST(1 AS bit))
) AS v (RolID, Nombre, Descripcion, Activo)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Roles r WHERE r.RolID = v.RolID);

IF OBJECTPROPERTY(OBJECT_ID('dbo.Roles'), 'TableHasIdentity') = 1
    SET IDENTITY_INSERT dbo.Roles OFF;
GO

/* ---------- 2. Usuarios ---------- */
IF OBJECTPROPERTY(OBJECT_ID('dbo.Usuarios'), 'TableHasIdentity') = 1
    SET IDENTITY_INSERT dbo.Usuarios ON;

INSERT INTO dbo.Usuarios (UsuarioID, RolID, SucursalID, NombreUsuario, NombreCompleto, Email, PasswordHash, Activo, FechaCreacion)
SELECT v.UsuarioID, v.RolID, v.SucursalID, v.NombreUsuario, v.NombreCompleto, v.Email, v.PasswordHash, v.Activo, SYSDATETIME()
FROM (VALUES
    (1, 1, 1, 'admin',      N'Administrador del Sistema',  'admin@clinica.sv',      'PBKDF2$100000$xm1+d3O2jllpPB6RK/0IKA==$DZfIpydF4YwM8yrB+/AsP7kYDNNLQVyzwZ7mHbI8H2U=', CAST(1 AS bit)),
    (2, 3, 1, 'recepcion',  N'Recepción Clínica Escalón',  'recepcion@clinica.sv',  'PBKDF2$100000$C1lkD5kNRlEcyLpbiY6Oaw==$NiMaDj8RdMg4ZQYMnz18Y92LOecN+0ua0MZjIyPf+tE=', CAST(1 AS bit)),
    (3, 2, 1, 'aportillo',  N'Ana Beatriz Portillo Guzmán','aportillo@clinica.sv',  'PBKDF2$100000$OR/tvJ60S3spYv7+bu+D4g==$4TQjyoxzUgu90M0qa9yAO9TkJYNlC893l0VMTBY0TNQ=', CAST(1 AS bit)),
    (4, 2, 1, 'jhernandez', N'José Ricardo Hernández Mejía','jhernandez@clinica.sv','PBKDF2$100000$y0usiqRy5Y/yzJIPssl9Mg==$3pTcWnOmR+CHYQ5IgP4BwdscFD3yCuasXJOd6eSMYmU=', CAST(1 AS bit)),
    (5, 2, 1, 'scruz',      N'Sofía Elena Cruz Alvarado',  'scruz@clinica.sv',      'PBKDF2$100000$vZ+8mrgKFAzhZIC8s0ZSSg==$2tQTu6gFf6qaXGK747ZTqdyUNaBqrkhioJs1o18I2IQ=', CAST(1 AS bit)),
    (6, 2, 1, 'mescobar',   N'Mario Antonio Escobar Linares','mescobar@clinica.sv', 'PBKDF2$100000$5PNUfcMu+/VWqC3RBOLvMg==$VTvTAr4hxKyAvnR177Zy7tcsJ+nDooNuMI6jX80RFhw=', CAST(1 AS bit))
) AS v (UsuarioID, RolID, SucursalID, NombreUsuario, NombreCompleto, Email, PasswordHash, Activo)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Usuarios u WHERE u.UsuarioID = v.UsuarioID);

IF OBJECTPROPERTY(OBJECT_ID('dbo.Usuarios'), 'TableHasIdentity') = 1
    SET IDENTITY_INSERT dbo.Usuarios OFF;
GO

/* ---------- 3. Médicos ----------
   Cada uno con su usuario. El médico 1 se queda sin usuario: es el único
   NULL que la restricción UNIQUE permite. */
IF OBJECTPROPERTY(OBJECT_ID('dbo.Medicos'), 'TableHasIdentity') = 1
    SET IDENTITY_INSERT dbo.Medicos ON;

INSERT INTO dbo.Medicos (MedicoID, UsuarioID, CodigoMedico, Nombres, Apellidos, NumeroLicencia, Telefono, Email, Activo)
SELECT v.MedicoID, v.UsuarioID, v.CodigoMedico, v.Nombres, v.Apellidos, v.NumeroLicencia, v.Telefono, v.Email, v.Activo
FROM (VALUES
    (2, 3, 'MED-002', N'Ana Beatriz',   N'Portillo Guzmán', 'JVPM-10987', '2250-1101', 'aportillo@clinica.sv',  CAST(1 AS bit)),
    (3, 4, 'MED-003', N'José Ricardo',  N'Hernández Mejía', 'JVPM-11455', '2250-1102', 'jhernandez@clinica.sv', CAST(1 AS bit)),
    (4, 5, 'MED-004', N'Sofía Elena',   N'Cruz Alvarado',   'JVPM-11890', '2250-1103', 'scruz@clinica.sv',      CAST(1 AS bit)),
    (5, 6, 'MED-005', N'Mario Antonio', N'Escobar Linares', 'JVPM-12044', '2250-1104', 'mescobar@clinica.sv',   CAST(1 AS bit))
) AS v (MedicoID, UsuarioID, CodigoMedico, Nombres, Apellidos, NumeroLicencia, Telefono, Email, Activo)
WHERE NOT EXISTS (SELECT 1 FROM dbo.Medicos m WHERE m.MedicoID = v.MedicoID);

IF OBJECTPROPERTY(OBJECT_ID('dbo.Medicos'), 'TableHasIdentity') = 1
    SET IDENTITY_INSERT dbo.Medicos OFF;
GO

/* ---------- 4. Medico_Especialidad ---------- */
INSERT INTO dbo.Medico_Especialidad (MedicoID, EspecialidadID, Principal)
SELECT v.MedicoID, v.EspecialidadID, v.Principal
FROM (VALUES
    (1, 1, CAST(1 AS bit)),
    (2, 2, CAST(1 AS bit)),
    (3, 4, CAST(1 AS bit)),
    (3, 1, CAST(0 AS bit)),
    (4, 3, CAST(1 AS bit)),
    (5, 5, CAST(1 AS bit))
) AS v (MedicoID, EspecialidadID, Principal)
WHERE EXISTS (SELECT 1 FROM dbo.Medicos m WHERE m.MedicoID = v.MedicoID)
  AND EXISTS (SELECT 1 FROM dbo.Especialidades e WHERE e.EspecialidadID = v.EspecialidadID)
  AND NOT EXISTS (SELECT 1 FROM dbo.Medico_Especialidad me
                  WHERE me.MedicoID = v.MedicoID AND me.EspecialidadID = v.EspecialidadID);
GO

/* ---------- 5. Verificación ---------- */
SELECT 'Roles' AS Tabla, COUNT(*) AS Filas FROM dbo.Roles
UNION ALL SELECT 'Usuarios',            COUNT(*) FROM dbo.Usuarios
UNION ALL SELECT 'Medicos',             COUNT(*) FROM dbo.Medicos
UNION ALL SELECT 'Medico_Especialidad', COUNT(*) FROM dbo.Medico_Especialidad
UNION ALL SELECT 'Especialidades',      COUNT(*) FROM dbo.Especialidades
UNION ALL SELECT 'Pacientes',           COUNT(*) FROM dbo.Pacientes;

SELECT m.MedicoID, m.CodigoMedico, m.Nombres + ' ' + m.Apellidos AS Medico,
       u.NombreUsuario, e.Nombre AS Especialidad, me.Principal
FROM dbo.Medicos m
LEFT JOIN dbo.Usuarios u ON u.UsuarioID = m.UsuarioID
LEFT JOIN dbo.Medico_Especialidad me ON me.MedicoID = m.MedicoID
LEFT JOIN dbo.Especialidades e ON e.EspecialidadID = me.EspecialidadID
ORDER BY m.MedicoID, me.Principal DESC;
GO
