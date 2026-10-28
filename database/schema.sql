/* =====================================================================
   schema.sql - Recepción de muestras
   Crea tablas, restricciones, índices y datos iniciales del catálogo.
   Se ejecuta sobre una base de datos vacía (p. ej. CREATE DATABASE Muestras).
   ===================================================================== */

-- 1. Pacientes
CREATE TABLE Patients (
    Id             INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Patients PRIMARY KEY,
    DocumentNumber VARCHAR(12)   NOT NULL,
    FullName       NVARCHAR(150) NOT NULL,
    CreatedAt      DATETIME2     NOT NULL CONSTRAINT DF_Patients_CreatedAt DEFAULT SYSDATETIME(),
    -- No pueden existir dos pacientes con el mismo documento
    CONSTRAINT UQ_Patients_DocumentNumber UNIQUE (DocumentNumber)
);
GO

-- 2. Catálogo de exámenes
CREATE TABLE Tests (
    Id   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Tests PRIMARY KEY,
    Code VARCHAR(20)   NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    -- No pueden existir dos exámenes con el mismo código
    CONSTRAINT UQ_Tests_Code UNIQUE (Code)
);
GO

-- 3. Muestras
CREATE TABLE Samples (
    Id              INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Samples PRIMARY KEY,
    PatientId       INT           NOT NULL,
    SampleType      VARCHAR(20)   NOT NULL,
    CollectedAt     DATETIME2     NOT NULL,
    Status          VARCHAR(20)   NOT NULL CONSTRAINT DF_Samples_Status DEFAULT 'Registrada',
    RejectionReason NVARCHAR(500) NULL,
    CreatedAt       DATETIME2     NOT NULL CONSTRAINT DF_Samples_CreatedAt DEFAULT SYSDATETIME(),
    CONSTRAINT FK_Samples_Patients FOREIGN KEY (PatientId) REFERENCES Patients(Id),
    CONSTRAINT CHK_Samples_SampleType CHECK (SampleType IN ('Sangre', 'Orina', 'Heces', 'Hisopado')),
    CONSTRAINT CHK_Samples_Status CHECK (Status IN ('Registrada', 'Recibida', 'Procesada', 'Rechazada')),
    -- Una muestra rechazada siempre debe tener motivo
    CONSTRAINT CHK_Samples_RejectionReason CHECK (Status <> 'Rechazada' OR RejectionReason IS NOT NULL)
);
GO

-- 4. Exámenes asociados a cada muestra (la PK impide repetir un examen en la misma muestra)
CREATE TABLE SampleTests (
    SampleId INT NOT NULL,
    TestId   INT NOT NULL,
    CONSTRAINT PK_SampleTests PRIMARY KEY (SampleId, TestId),
    CONSTRAINT FK_SampleTests_Samples FOREIGN KEY (SampleId) REFERENCES Samples(Id),
    CONSTRAINT FK_SampleTests_Tests FOREIGN KEY (TestId) REFERENCES Tests(Id)
);
GO

/* Índices no agrupados
   - IX_Samples_CreatedAt: el listado ordena por CreatedAt DESC y pagina con OFFSET/FETCH;
     también apoya el filtro por mes en curso de queries.sql.
   - IX_Samples_PatientId: join con Patients y conteo de rechazos por paciente.
   - IX_SampleTests_TestId: conteo de solicitudes por examen (la PK empieza por SampleId). */
CREATE NONCLUSTERED INDEX IX_Samples_CreatedAt ON Samples (CreatedAt DESC, Id DESC) INCLUDE (Status);
CREATE NONCLUSTERED INDEX IX_Samples_PatientId ON Samples (PatientId) INCLUDE (Status);
CREATE NONCLUSTERED INDEX IX_SampleTests_TestId ON SampleTests (TestId);
GO

-- Datos iniciales del catálogo
INSERT INTO Tests (Code, Name) VALUES
('HEM01', N'Hemograma completo'),
('GLU02', N'Glucosa en ayunas'),
('URO03', N'Uroanálisis completo'),
('COP04', N'Coprológico general'),
('PCR05', N'Hisopado PCR COVID-19');
GO
