/* =====================================================================
   queries.sql - Consultas de la sección de base de datos
   Se ejecutan sobre el modelo creado por schema.sql.
   ===================================================================== */

/* 1. Cantidad de muestras por estado, solo las creadas en el mes en curso.
      Se filtra por rango [primer día del mes, primer día del mes siguiente) en lugar de
      MONTH()/YEAR() sobre la columna, para que el filtro pueda usar IX_Samples_CreatedAt. */
DECLARE @MonthStart DATETIME2 = DATEFROMPARTS(YEAR(SYSDATETIME()), MONTH(SYSDATETIME()), 1);
DECLARE @NextMonthStart DATETIME2 = DATEADD(MONTH, 1, @MonthStart);

SELECT
    s.Status,
    COUNT(*) AS SampleCount
FROM Samples s
WHERE s.CreatedAt >= @MonthStart
  AND s.CreatedAt <  @NextMonthStart
GROUP BY s.Status
ORDER BY SampleCount DESC;
GO

/* 2. Todos los exámenes del catálogo con la cantidad de veces que han sido solicitados,
      incluidos los que nunca se han solicitado (LEFT JOIN + COUNT de la columna del lado
      derecho, que da 0), ordenados de mayor a menor cantidad. */
SELECT
    t.Code,
    t.Name,
    COUNT(st.SampleId) AS TimesRequested
FROM Tests t
LEFT JOIN SampleTests st ON st.TestId = t.Id
GROUP BY t.Id, t.Code, t.Name
ORDER BY TimesRequested DESC, t.Code;
GO

/* 3. Pacientes con dos o más muestras rechazadas: número de documento, nombre
      y cantidad de rechazos. */
SELECT
    p.DocumentNumber,
    p.FullName,
    COUNT(*) AS RejectedCount
FROM Patients p
INNER JOIN Samples s ON s.PatientId = p.Id
WHERE s.Status = 'Rechazada'
GROUP BY p.Id, p.DocumentNumber, p.FullName
HAVING COUNT(*) >= 2
ORDER BY RejectedCount DESC, p.FullName;
GO
