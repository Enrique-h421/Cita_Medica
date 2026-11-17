/* ============================================================================
   04_EstadosCita.sql   —   CitaMedica / ClinicaDB
   ----------------------------------------------------------------------------
   Siembra los estados que le faltan a la tabla Estados_Cita.

   Hoy la tabla solo tiene "Confirmada", así que todas las citas se ven iguales
   y el ciclo de la cita no se puede contar: no hay a dónde moverla cuando se
   atiende, se cancela o el paciente no llega.

   Este script agrega los que faltan y NO toca los que ya existen. Se puede
   correr varias veces sin duplicar nada.

   El código ("CONF", "ATEN", ...) es lo que la aplicación usa para encontrar
   el estado, no el Id numérico: así no importa en qué orden se sembraron.
   Están en Domain/AgendaYAtencion/CodigosEstadoCita.cs.
   ============================================================================ */

USE ClinicaDB;
GO

/* ---------------------------------------------------------------------------
   PASO 1 — Esquema actual de la tabla.
   Esto no cambia nada, solo muestra las columnas. Si el PASO 2 falla porque la
   tabla tiene una columna obligatoria que este script no conoce, mandame lo
   que salga aquí y lo ajusto.
   --------------------------------------------------------------------------- */
SELECT
    c.column_id                                          AS Orden,
    c.name                                               AS Columna,
    t.name                                               AS Tipo,
    c.max_length                                         AS Largo,
    c.is_nullable                                        AS AceptaNulos,
    COLUMNPROPERTY(c.object_id, c.name, 'IsIdentity')    AS EsIdentity,
    dc.definition                                        AS ValorPorDefecto
FROM sys.columns c
JOIN sys.types t
    ON t.user_type_id = c.user_type_id
LEFT JOIN sys.default_constraints dc
    ON dc.parent_object_id = c.object_id
   AND dc.parent_column_id = c.column_id
WHERE c.object_id = OBJECT_ID('dbo.Estados_Cita')
ORDER BY c.column_id;
GO

/* ---------------------------------------------------------------------------
   PASO 2 — Insertar los estados que falten.
   --------------------------------------------------------------------------- */
DECLARE @estados TABLE (Codigo varchar(30), Nombre nvarchar(100));

INSERT INTO @estados (Codigo, Nombre) VALUES
    ('PEND', N'Pendiente'),
    ('CONF', N'Confirmada'),
    ('ATEN', N'Atendida'),
    ('CANC', N'Cancelada'),
    ('NOAS', N'No asistió');

IF COLUMNPROPERTY(OBJECT_ID('dbo.Estados_Cita'), 'EstadoCitaID', 'IsIdentity') = 1
BEGIN
    -- El Id lo pone SQL Server.
    INSERT INTO dbo.Estados_Cita (Codigo, Nombre)
    SELECT e.Codigo, e.Nombre
    FROM @estados e
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.Estados_Cita x WHERE x.Codigo = e.Codigo
    );
END
ELSE
BEGIN
    -- La tabla no es identity: hay que calcular el Id a mano, siguiendo
    -- después del último que ya existe.
    DECLARE @ultimo int = ISNULL((SELECT MAX(EstadoCitaID) FROM dbo.Estados_Cita), 0);

    INSERT INTO dbo.Estados_Cita (EstadoCitaID, Codigo, Nombre)
    SELECT
        @ultimo + ROW_NUMBER() OVER (ORDER BY e.Codigo),
        e.Codigo,
        e.Nombre
    FROM @estados e
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.Estados_Cita x WHERE x.Codigo = e.Codigo
    );
END
GO

/* ---------------------------------------------------------------------------
   PASO 3 — Verificación.
   Deben aparecer los 5 estados, y la cuenta de citas por estado.
   --------------------------------------------------------------------------- */
SELECT
    e.EstadoCitaID,
    e.Codigo,
    e.Nombre,
    (SELECT COUNT(*) FROM dbo.Citas c WHERE c.EstadoCitaID = e.EstadoCitaID) AS Citas
FROM dbo.Estados_Cita e
ORDER BY e.EstadoCitaID;
GO
