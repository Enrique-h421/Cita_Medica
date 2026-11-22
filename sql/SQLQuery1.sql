USE ClinicaDB;

SELECT t.name AS Tabla,
       c.column_id AS Orden,
       c.name AS Columna,
       ty.name AS Tipo,
       c.max_length AS Largo,
       c.is_nullable AS AceptaNulos,
       COLUMNPROPERTY(c.object_id, c.name, 'IsIdentity') AS EsIdentity
FROM sys.columns c
JOIN sys.tables t ON t.object_id = c.object_id
JOIN sys.types ty ON ty.user_type_id = c.user_type_id
WHERE t.name IN ('Diagnosticos', 'Atencion_Diagnostico')
ORDER BY t.name, c.column_id;

SELECT COUNT(*) AS Diagnosticos FROM dbo.Diagnosticos;
SELECT COUNT(*) AS AtencionDiagnostico FROM dbo.Atencion_Diagnostico;