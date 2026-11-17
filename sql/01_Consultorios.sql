/* =====================================================================
   CitaMedica — Agenda y atención
   Siembra de Sucursal, Áreas y Consultorios sobre el esquema entregado.

   Base de datos: ClinicaDB

   Las tablas YA EXISTEN en el esquema del ingeniero. Este script solo
   inserta datos, en el orden que exigen las llaves foráneas:
       Sucursales  ->  Areas  ->  Consultorios

   Sucursales y Areas no están en el alcance de 20 tablas elegido, pero
   Consultorios.SucursalID es NOT NULL, así que son dependencia estructural.

   Es idempotente: se puede correr varias veces sin duplicar nada.
   ===================================================================== */

USE ClinicaDB;
GO

/* ---------- 1. Sucursal ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Sucursales WHERE SucursalID = 1)
BEGIN
    IF OBJECTPROPERTY(OBJECT_ID('dbo.Sucursales'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT dbo.Sucursales ON;

    INSERT INTO dbo.Sucursales (SucursalID, Codigo, Nombre, Direccion, Telefono, Email, Activo, FechaCreacion)
    VALUES (1, 'SUC-ESC', N'Clínica Escalón',
            N'Paseo General Escalón, San Salvador', '2250-1000',
            'contacto@clinicaescalon.sv', 1, SYSDATETIME());

    IF OBJECTPROPERTY(OBJECT_ID('dbo.Sucursales'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT dbo.Sucursales OFF;

    PRINT 'Sucursal insertada.';
END
ELSE
    PRINT 'La sucursal 1 ya existía.';
GO

/* ---------- 2. Áreas ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Areas WHERE AreaID IN (1, 2, 3))
BEGIN
    IF OBJECTPROPERTY(OBJECT_ID('dbo.Areas'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT dbo.Areas ON;

    INSERT INTO dbo.Areas (AreaID, SucursalID, Codigo, Nombre, Activo)
    VALUES
        (1, 1, 'ARE-CEX', N'Consulta Externa', 1),
        (2, 1, 'ARE-PED', N'Pediatría',        1),
        (3, 1, 'ARE-GIN', N'Ginecología',      1);

    IF OBJECTPROPERTY(OBJECT_ID('dbo.Areas'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT dbo.Areas OFF;

    PRINT 'Áreas insertadas.';
END
ELSE
    PRINT 'Las áreas ya existían.';
GO

/* ---------- 3. Consultorios ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.Consultorios)
BEGIN
    IF OBJECTPROPERTY(OBJECT_ID('dbo.Consultorios'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT dbo.Consultorios ON;

    INSERT INTO dbo.Consultorios (ConsultorioID, SucursalID, AreaID, Codigo, Nombre, Piso, Activo)
    VALUES
        (1, 1, 1, 'C-101', N'Consultorio 1',       '1', 1),
        (2, 1, 1, 'C-102', N'Consultorio 2',       '1', 1),
        (3, 1, 2, 'C-201', N'Consultorio 3',       '2', 1),
        (4, 1, 3, 'C-202', N'Consultorio 4',       '2', 1),
        (5, 1, 1, 'C-PB1', N'Sala de curaciones',  'PB', 1),
        (6, 1, 1, 'C-301', N'Consultorio 6',       '3', 0);

    IF OBJECTPROPERTY(OBJECT_ID('dbo.Consultorios'), 'TableHasIdentity') = 1
        SET IDENTITY_INSERT dbo.Consultorios OFF;

    PRINT 'Consultorios insertados.';
END
ELSE
    PRINT 'La tabla Consultorios ya tenía datos.';
GO

/* ---------- 4. Repartir las citas que ya existen ----------
   Solo las activas (1 a 4), para que la demo no se vea con la columna vacía. */
UPDATE dbo.Citas
SET ConsultorioID = ((CitaID % 4) + 1)
WHERE ConsultorioID IS NULL;

PRINT 'Citas sin consultorio actualizadas.';
GO

/* ---------- 5. Verificación ---------- */
SELECT c.ConsultorioID, c.Codigo, c.Nombre, c.Piso,
       s.Nombre AS Sucursal, a.Nombre AS Area, c.Activo
FROM dbo.Consultorios c
JOIN dbo.Sucursales s ON s.SucursalID = c.SucursalID
LEFT JOIN dbo.Areas a ON a.AreaID = c.AreaID
ORDER BY c.ConsultorioID;

SELECT ci.CitaID, ci.FechaHoraInicio, co.Codigo + ' — ' + co.Nombre AS Consultorio
FROM dbo.Citas ci
LEFT JOIN dbo.Consultorios co ON co.ConsultorioID = ci.ConsultorioID
ORDER BY ci.CitaID;
GO
