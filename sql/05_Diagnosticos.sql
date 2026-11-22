
USE ClinicaDB;
GO

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
WHERE c.object_id = OBJECT_ID('dbo.Diagnosticos')
ORDER BY c.column_id;
GO

 
DECLARE @diagnosticos TABLE (
    CodigoCIE10 varchar(20),
    Nombre      nvarchar(300),
    Descripcion nvarchar(1000)
);

INSERT INTO @diagnosticos (CodigoCIE10, Nombre, Descripcion) VALUES
    -- Respiratorio
    ('J00',   N'Rinofaringitis aguda (resfriado común)',            N'Cuadro viral de vías respiratorias altas, autolimitado.'),
    ('J02.9', N'Faringitis aguda, no especificada',                 N'Dolor de garganta sin agente identificado.'),
    ('J03.9', N'Amigdalitis aguda, no especificada',                NULL),
    ('J06.9', N'Infección aguda de vías respiratorias superiores',  N'Diagnóstico de uso frecuente cuando el sitio exacto no se precisa.'),
    ('J11.1', N'Influenza con manifestaciones respiratorias',       N'Virus no identificado por laboratorio.'),
    ('J18.9', N'Neumonía, no especificada',                         N'Requiere valoración y probable radiografía de tórax.'),
    ('J20.9', N'Bronquitis aguda, no especificada',                 NULL),
    ('J30.4', N'Rinitis alérgica, no especificada',                 NULL),
    ('J45.9', N'Asma, no especificada',                             N'Enfermedad crónica; suele registrarse como diagnóstico secundario.'),

    -- Infeccioso y digestivo
    ('A09',   N'Diarrea y gastroenteritis de presunto origen infeccioso', NULL),
    ('A90',   N'Dengue clásico',                                    N'Vigilancia epidemiológica: notificación obligatoria.'),
    ('B01.9', N'Varicela sin complicaciones',                       NULL),
    ('B34.9', N'Infección viral, no especificada',                  NULL),
    ('K21.9', N'Enfermedad por reflujo gastroesofágico sin esofagitis', NULL),
    ('K29.7', N'Gastritis, no especificada',                        NULL),
    ('K30',   N'Dispepsia',                                         NULL),

    -- Crónicas y metabólicas
    ('E03.9', N'Hipotiroidismo, no especificado',                   NULL),
    ('E11.9', N'Diabetes mellitus tipo 2 sin complicaciones',       N'Crónica; se controla, no se cura.'),
    ('E66.9', N'Obesidad, no especificada',                         NULL),
    ('E78.5', N'Hiperlipidemia, no especificada',                   NULL),
    ('I10',   N'Hipertensión esencial (primaria)',                  N'Crónica; el diagnóstico más frecuente en consulta de control.'),
    ('D50.9', N'Anemia por deficiencia de hierro, no especificada', NULL),

    -- Urinario
    ('N39.0', N'Infección de vías urinarias, sitio no especificado', NULL),

    -- Musculoesquelético y neurológico
    ('M54.5', N'Lumbalgia',                                         N'Dolor lumbar bajo, causa más común de incapacidad corta.'),
    ('M25.5', N'Dolor en articulación',                             NULL),
    ('G43.9', N'Migraña, no especificada',                          NULL),

    -- Piel, ojo y oído
    ('L23.9', N'Dermatitis alérgica de contacto, causa no especificada', NULL),
    ('L30.9', N'Dermatitis, no especificada',                       NULL),
    ('H10.9', N'Conjuntivitis, no especificada',                    NULL),
    ('H66.9', N'Otitis media, no especificada',                     NULL),

    -- Síntomas y signos (cuando aún no hay diagnóstico cerrado)
    ('R50.9', N'Fiebre, no especificada',                           N'Se usa cuando todavía no se identifica la causa.'),
    ('R51',   N'Cefalea',                                           NULL),
    ('R10.4', N'Otros dolores abdominales y los no especificados',  NULL),
    ('T78.4', N'Alergia, no especificada',                          NULL),

    -- Salud mental
    ('F41.9', N'Trastorno de ansiedad, no especificado',            NULL),
    ('F32.9', N'Episodio depresivo, no especificado',               NULL),

    -- Consulta sin enfermedad
    ('Z00.0', N'Examen médico general',                             N'Chequeo de rutina, paciente sano.'),
    ('Z76.0', N'Consulta para repetición de receta',                N'Paciente crónico que viene solo por su receta.');

IF COLUMNPROPERTY(OBJECT_ID('dbo.Diagnosticos'), 'DiagnosticoID', 'IsIdentity') = 1
BEGIN
   
    INSERT INTO dbo.Diagnosticos (CodigoCIE10, Nombre, Descripcion, Activo)
    SELECT d.CodigoCIE10, d.Nombre, d.Descripcion, 1
    FROM @diagnosticos d
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.Diagnosticos x WHERE x.CodigoCIE10 = d.CodigoCIE10
    );
END
ELSE
BEGIN
 
    DECLARE @ultimo int = ISNULL((SELECT MAX(DiagnosticoID) FROM dbo.Diagnosticos), 0);

    INSERT INTO dbo.Diagnosticos (DiagnosticoID, CodigoCIE10, Nombre, Descripcion, Activo)
    SELECT
        @ultimo + ROW_NUMBER() OVER (ORDER BY d.CodigoCIE10),
        d.CodigoCIE10,
        d.Nombre,
        d.Descripcion,
        1
    FROM @diagnosticos d
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.Diagnosticos x WHERE x.CodigoCIE10 = d.CodigoCIE10
    );
END
GO

SELECT COUNT(*) AS TotalDiagnosticos FROM dbo.Diagnosticos;
GO

SELECT
    d.DiagnosticoID,
    d.CodigoCIE10,
    d.Nombre,
    d.Activo,
    (SELECT COUNT(*)
     FROM dbo.Atencion_Diagnostico ad
     WHERE ad.DiagnosticoID = d.DiagnosticoID) AS VecesUsado
FROM dbo.Diagnosticos d
ORDER BY d.CodigoCIE10;
GO
