
USE master;
GO

ALTER DATABASE AgroForm SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

DROP DATABASE AgroForm;
GO

CREATE DATABASE AgroForm;


USE AgroForm;
GO

CREATE TABLE Licencias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RazonSocial NVARCHAR(200) NOT NULL,
    NombreContacto NVARCHAR(100) NULL,
    NumeroContacto NVARCHAR(50) NULL,
	TipoLicencia INT NOT NULL DEFAULT 0,
    EsPrueba BIT NOT NULL DEFAULT 0,
    FechaFinPrueba DATETIME NULL,
	Activo BIT NOT NULL DEFAULT 1,
	
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL
);

CREATE TABLE PagoLicencias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    TipoPagoLicencia INT NOT NULL DEFAULT 0,
    Precio DECIMAL(18,2) NOT NULL,
    Fecha DATETIME NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL
    
    CONSTRAINT FK_PagoLicencias_Licencias FOREIGN KEY (IdLicencia) 
        REFERENCES Licencias(Id) ON DELETE CASCADE
);

CREATE TABLE Usuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    Rol INT NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    PasswordSalt VARBINARY(MAX) NOT NULL,
    EmailConfirmed BIT DEFAULT 0,
    SuperAdmin BIT DEFAULT 0,
    PhoneNumber NVARCHAR(50) NULL,
	IdMonedaReferencia INT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE
);

CREATE TABLE Campos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Ubicacion NVARCHAR(250) NULL,
    SuperficieHectareas DECIMAL(10,2) NULL,
    Latitud DECIMAL(18,8) NOT NULL,
    Longitud DECIMAL(18,8) NOT NULL,
	CoordenadasPoligono NVARCHAR(MAX) NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE
);

CREATE TABLE Campanias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    EstadosCampania INT NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    FechaInicio DATETIME NOT NULL,
    FechaFin DATETIME NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE
);

CREATE TABLE Cultivos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    Orden INT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    Color NVARCHAR(7) NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE
);

CREATE TABLE EstadosFenologicos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdCultivo INT NOT NULL,
    Codigo NVARCHAR(50) NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    Orden INT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdCultivo) REFERENCES Cultivos(Id) ON DELETE CASCADE
);

-- ============================================
-- TABLA: LicenciasCultivos (Visibility side table)
-- ============================================
CREATE TABLE LicenciasCultivos (
    Id INT IDENTITY(1,1) NOT NULL,
    IdLicencia INT NOT NULL,
    IdCultivo INT NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    Orden INT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    CONSTRAINT PK_LicenciasCultivos PRIMARY KEY (Id),
    CONSTRAINT UQ_LicenciasCultivos_IdLicencia_IdCultivo UNIQUE (IdLicencia, IdCultivo),
    CONSTRAINT FK_LicenciasCultivos_Licencias
        FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE,
    CONSTRAINT FK_LicenciasCultivos_Cultivos
        FOREIGN KEY (IdCultivo) REFERENCES Cultivos(Id) ON DELETE NO ACTION
);

CREATE TABLE Catalogos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Tipo INT NOT NULL, -- Según enum TipoCatalogo
    Nombre NVARCHAR(150) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    IdLicencia INT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE
);

-- ============================================
-- TABLA: LicenciasCatalogos (Visibility side table)
-- ============================================
CREATE TABLE LicenciasCatalogos (
    Id INT IDENTITY(1,1) NOT NULL,
    IdLicencia INT NOT NULL,
    IdCatalogo INT NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    Orden INT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    CONSTRAINT PK_LicenciasCatalogos PRIMARY KEY (Id),
    CONSTRAINT UQ_LicenciasCatalogos_IdLicencia_IdCatalogo UNIQUE (IdLicencia, IdCatalogo),
    CONSTRAINT FK_LicenciasCatalogos_Licencias
        FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE,
    CONSTRAINT FK_LicenciasCatalogos_Catalogos
        FOREIGN KEY (IdCatalogo) REFERENCES Catalogos(Id) ON DELETE NO ACTION
);

CREATE TABLE Lotes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    SuperficieHectareas DECIMAL(10,2) NULL,
    IdCampo INT NOT NULL,
    IdCampania INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdCampo) REFERENCES Campos(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE
);

-- ============================================
-- TABLA: CicloCultivos
-- ============================================
CREATE TABLE CicloCultivos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    IdLote INT NOT NULL,
    IdCultivo INT NOT NULL,
    IdCampania INT NOT NULL,
    Epoca INT NULL,
    FechaInicio DATETIME NULL,
    FechaFin DATETIME NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id),
    FOREIGN KEY (IdCultivo) REFERENCES Cultivos(Id),
    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id)
);


CREATE TABLE Monedas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Codigo NVARCHAR(10) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Simbolo NVARCHAR(10) NULL,
    TipoCambioReferencia DECIMAL(18,4) NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL
);

-- UnidadesMedida
CREATE TABLE UnidadesMedida (
    Id INT IDENTITY(1,1) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Sigla NVARCHAR(20) NOT NULL,
    Categoria INT NOT NULL,
    DimensionBase INT NOT NULL,
    FactorConversion DECIMAL(18,6) NOT NULL DEFAULT 1.0,
    Orden INT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    CONSTRAINT PK_UnidadesMedida PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT CHK_UnidadesMedida_FactorConversion CHECK (FactorConversion > 0)
);

CREATE TABLE Siembras (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdCampania INT NOT NULL,
    IdLicencia INT NOT NULL,
    Fecha DATETIME NOT NULL,
    Observacion NVARCHAR(MAX) NULL,
    IdLote INT NOT NULL,
    IdTipoActividad INT NOT NULL,
    IdUsuario INT NULL,
    SuperficieHa DECIMAL(10,2) NULL,
    Superficie DECIMAL(10,2) NULL,
    IdUnidadSuperficie INT NULL,
    DensidadSemillaKgHa DECIMAL(10,2) NULL,
    Densidad DECIMAL(10,2) NULL,
    IdUnidadDensidad INT NULL,
    Costo DECIMAL(18,2) NULL,
	CostoARS DECIMAL(18,2) NULL,
    CostoUSD DECIMAL(18,2) NULL,
    IdCultivo INT NOT NULL,
    IdMetodoSiembra INT NULL,
	IdMoneda INT NOT NULL,
    Epoca INT NULL,
    IdCicloCultivo INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    
    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id),
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
    FOREIGN KEY (IdCultivo) REFERENCES Cultivos(Id),
    FOREIGN KEY (IdMetodoSiembra) REFERENCES Catalogos(Id),
 FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdCicloCultivo) REFERENCES CicloCultivos(Id),
    FOREIGN KEY (IdUnidadSuperficie) REFERENCES UnidadesMedida(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdUnidadDensidad) REFERENCES UnidadesMedida(Id) ON DELETE NO ACTION
);

CREATE TABLE Riegos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdCampania INT NOT NULL,
    IdLicencia INT NOT NULL,
    Fecha DATETIME NOT NULL,
    Observacion NVARCHAR(MAX) NULL,
    IdLote INT NOT NULL,
    IdTipoActividad INT NOT NULL,
    IdUsuario INT NULL,
    HorasRiego DECIMAL(10,2) NULL,
    VolumenAguaM3 DECIMAL(10,2) NULL,
    VolumenAgua DECIMAL(10,2) NULL,
    IdUnidadVolumenAgua INT NULL,
    Costo DECIMAL(18,2) NULL,
	CostoARS DECIMAL(18,2) NULL,
    CostoUSD DECIMAL(18,2) NULL,
    IdMetodoRiego INT NULL,
    IdFuenteAgua INT NULL,
	IdMoneda INT NOT NULL,
    IdCicloCultivo INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    
    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id),
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
    FOREIGN KEY (IdMetodoRiego) REFERENCES Catalogos(Id),
    FOREIGN KEY (IdFuenteAgua) REFERENCES Catalogos(Id),
 FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdCicloCultivo) REFERENCES CicloCultivos(Id),
    FOREIGN KEY (IdUnidadVolumenAgua) REFERENCES UnidadesMedida(Id) ON DELETE NO ACTION
);

CREATE TABLE Fertilizaciones (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdCampania INT NOT NULL,
    IdLicencia INT NOT NULL,
    Fecha DATETIME NOT NULL,
    Observacion NVARCHAR(MAX) NULL,
    IdLote INT NOT NULL,
    IdTipoActividad INT NOT NULL,
    IdUsuario INT NULL,
    CantidadKgHa DECIMAL(10,2) NULL,
    Cantidad DECIMAL(10,2) NULL,
    IdUnidadCantidad INT NULL,
    IdNutriente INT NULL,
    IdTipoFertilizante INT NULL,
    DosisKgHa DECIMAL(10,2) NULL,
    Dosis DECIMAL(10,2) NULL,
    IdUnidadDosis INT NULL,
    Costo DECIMAL(18,2) NULL,
	CostoARS DECIMAL(18,2) NULL,
    CostoUSD DECIMAL(18,2) NULL,
    IdMetodoAplicacion INT NULL,
	IdMoneda INT NOT NULL,
    IdCicloCultivo INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,

    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id),
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
    FOREIGN KEY (IdNutriente) REFERENCES Catalogos(Id),
    FOREIGN KEY (IdTipoFertilizante) REFERENCES Catalogos(Id),
    FOREIGN KEY (IdMetodoAplicacion) REFERENCES Catalogos(Id),
    FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdCicloCultivo) REFERENCES CicloCultivos(Id),
    FOREIGN KEY (IdUnidadCantidad) REFERENCES UnidadesMedida(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdUnidadDosis) REFERENCES UnidadesMedida(Id) ON DELETE NO ACTION
);

CREATE TABLE Pulverizaciones (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdCampania INT NOT NULL,
    IdLicencia INT NOT NULL,
    Fecha DATETIME NOT NULL,
    Observacion NVARCHAR(MAX) NULL,
    IdLote INT NOT NULL,
    IdTipoActividad INT NOT NULL,
    IdUsuario INT NULL,
    VolumenLitrosHa DECIMAL(10,2) NULL,
    Volumen DECIMAL(10,2) NULL,
    IdUnidadVolumen INT NULL,
    Dosis DECIMAL(10,2) NULL,
    IdUnidadDosis INT NULL,
    CondicionesClimaticas NVARCHAR(200) NULL,
    Costo DECIMAL(18,2) NULL,
	CostoARS DECIMAL(18,2) NULL,
    CostoUSD DECIMAL(18,2) NULL,
    IdProductoAgroquimico INT NULL,
	IdMoneda INT NOT NULL,
    IdCicloCultivo INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,

    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id),
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
    FOREIGN KEY (IdProductoAgroquimico) REFERENCES Catalogos(Id),
 FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdCicloCultivo) REFERENCES CicloCultivos(Id),
    FOREIGN KEY (IdUnidadVolumen) REFERENCES UnidadesMedida(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdUnidadDosis) REFERENCES UnidadesMedida(Id) ON DELETE NO ACTION
);

CREATE TABLE Monitoreos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdCampania INT NOT NULL,
    IdLicencia INT NOT NULL,
    Fecha DATETIME NOT NULL,
    Observacion NVARCHAR(MAX) NULL,
    Costo DECIMAL(18,2) NULL,
	CostoARS DECIMAL(18,2) NULL,
    CostoUSD DECIMAL(18,2) NULL,
	IdMonitoreo INT NOT NULL,
    IdLote INT NOT NULL,
    IdTipoActividad INT NOT NULL,
    IdUsuario INT NULL,
    IdEstadoFenologico INT NULL,
    IdTipoMonitoreo INT NOT NULL,
	IdMoneda INT NOT NULL,
    IdCicloCultivo INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,

    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id),
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
    FOREIGN KEY (IdEstadoFenologico) REFERENCES EstadosFenologicos(Id),
    FOREIGN KEY (IdTipoMonitoreo) REFERENCES Catalogos(Id),
	FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdCicloCultivo) REFERENCES CicloCultivos(Id)
);

CREATE TABLE AnalisisSuelos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdCampania INT NOT NULL,
    IdLicencia INT NOT NULL,
    Fecha DATETIME NOT NULL,
    Observacion NVARCHAR(MAX) NULL,
    IdLote INT NOT NULL,
    IdTipoActividad INT NOT NULL,
    IdUsuario INT NULL,
    ProfundidadCm DECIMAL(10,2) NULL,
    PH DECIMAL(5,2) NULL,
    MateriaOrganica DECIMAL(10,2) NULL,
    Nitrogeno DECIMAL(10,2) NULL,
    Fosforo DECIMAL(10,2) NULL,
    Potasio DECIMAL(10,2) NULL,
    ConductividadElectrica DECIMAL(10,2) NULL,
    CIC DECIMAL(10,2) NULL,
    Textura NVARCHAR(50) NULL,
    Costo DECIMAL(18,2) NULL,
	CostoARS DECIMAL(18,2) NULL,
    CostoUSD DECIMAL(18,2) NULL,
    IdLaboratorio INT NULL,
	IdMoneda INT NOT NULL,
    IdCicloCultivo INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,

    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id),
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
    FOREIGN KEY (IdLaboratorio) REFERENCES Catalogos(Id),
	FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdCicloCultivo) REFERENCES CicloCultivos(Id)
);

CREATE TABLE Cosechas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdCampania INT NOT NULL,
    IdLicencia INT NOT NULL,
    Fecha DATETIME NOT NULL,
    Observacion NVARCHAR(MAX) NULL,
    IdLote INT NOT NULL,
    IdTipoActividad INT NOT NULL,
    IdUsuario INT NULL,
    RendimientoTonHa DECIMAL(10,2) NULL,
    Rendimiento DECIMAL(10,2) NULL,
    IdUnidadRendimiento INT NULL,
    HumedadGrano DECIMAL(5,2) NULL,
    SuperficieCosechadaHa DECIMAL(10,2) NULL,
    SuperficieCosechada DECIMAL(10,2) NULL,
    IdUnidadSuperficieCosechada INT NULL,
    Costo DECIMAL(18,2) NULL,
	CostoARS DECIMAL(18,2) NULL,
    CostoUSD DECIMAL(18,2) NULL,
    IdCultivo INT NOT NULL,
	IdMoneda INT NOT NULL,
    IdCicloCultivo INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,

    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id),
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
    FOREIGN KEY (IdCultivo) REFERENCES Cultivos(Id),
	FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdCicloCultivo) REFERENCES CicloCultivos(Id)
);

CREATE TABLE OtrasLabores (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdCampania INT NOT NULL,
    IdLicencia INT NOT NULL,
    Fecha DATETIME NOT NULL,
    Observacion NVARCHAR(MAX) NULL,
    Costo DECIMAL(18,2) NULL,
	CostoARS DECIMAL(18,2) NULL,
    CostoUSD DECIMAL(18,2) NULL,
    IdLote INT NOT NULL,
    IdTipoActividad INT NOT NULL,
    IdUsuario INT NULL,
	IdMoneda INT NOT NULL,
    IdCicloCultivo INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,

    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id),
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
 FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION,
   FOREIGN KEY (IdCicloCultivo) REFERENCES CicloCultivos(Id)
);

-- ============================================
-- TABLA: Acopios
-- ============================================
CREATE TABLE Acopios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdCampania INT NOT NULL,
    IdLicencia INT NOT NULL,
    Fecha DATETIME NOT NULL,
    Observacion NVARCHAR(MAX) NULL,
    IdLote INT NOT NULL,
    IdTipoActividad INT NOT NULL,
    IdUsuario INT NULL,
    TipoAcopio INT NOT NULL DEFAULT 1,
    Codigo NVARCHAR(50) NOT NULL DEFAULT '',
    FechaIngreso DATETIME2 NULL,
    IdCultivo INT NOT NULL,
    CantidadActualTn DECIMAL(10,2) NULL,
    CapacidadTotalTn DECIMAL(10,2) NULL,
    HumedadGrano DECIMAL(5,2) NULL,
    Estado NVARCHAR(50) NOT NULL DEFAULT '',
    Ubicacion NVARCHAR(150) NOT NULL DEFAULT '',
    -- Silo Bolsa specific
    Longitud DECIMAL(10,2) NULL,
    Diametro DECIMAL(10,2) NULL,
    FechaEmbolsado DATETIME2 NULL,
    -- Silo specific
    TipoSilo NVARCHAR(50) NULL,
    Aireacion BIT NULL,
    TemperaturaGrano DECIMAL(5,2) NULL,
    -- Planta Externa specific
    Empresa NVARCHAR(150) NULL,
    NumeroContrato NVARCHAR(50) NULL,
    TarifaAlmacenaje DECIMAL(18,2) NULL,
    Costo DECIMAL(18,2) NULL,
    CostoARS DECIMAL(18,2) NULL,
    CostoUSD DECIMAL(18,2) NULL,
    IdMoneda INT NOT NULL,
    IdCicloCultivo INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,

    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id),
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
    FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdCicloCultivo) REFERENCES CicloCultivos(Id),
    FOREIGN KEY (IdCultivo) REFERENCES Cultivos(Id)
);

CREATE TABLE RegistrosClima (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    IdCampania INT NOT NULL,
    Fecha DATETIME NOT NULL,
    Milimetros DECIMAL(10,2) NOT NULL,
    IdCampo INT NOT NULL,
    TipoClima INT NOT NULL,
    Observaciones NVARCHAR(MAX) NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
	
    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id),
    FOREIGN KEY (IdCampo) REFERENCES Campos(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE CASCADE
);


CREATE TABLE TiposActividad (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Icono NVARCHAR(50) NOT NULL,
    ColorIcono NVARCHAR(20),
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL
);

CREATE TABLE [dbo].[ReporteCierreCampania](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [IdLicencia] [int] NOT NULL,
    [FechaCreacion] [datetime2](7) NOT NULL,
    [FechaModificacion] [datetime2](7) NULL,
    [UsuarioCreacion] [nvarchar](256) NULL,
    [UsuarioModificacion] [nvarchar](256) NULL,
    
    [IdCampania] [int] NOT NULL,
    
    [NombreCampania] [nvarchar](500) NOT NULL,
    [FechaInicio] [datetime2](7) NOT NULL,
    [FechaFin] [datetime2](7) NOT NULL,
    
    [SuperficieTotalHa] [decimal](18,2) NOT NULL,
    [ToneladasProducidas] [decimal](18,2) NOT NULL,
    [CostoPorHaArs] [decimal](18,2) NOT NULL,
    [CostoPorToneladaArs] [decimal](18,2) NOT NULL,
    [CostoPorHaUsd] [decimal](18,2) NOT NULL,
    [CostoPorToneladaUsd] [decimal](18,2) NOT NULL,
    [RendimientoPromedioHa] [decimal](18,2) NOT NULL,
    
    [AnalisisSueloArs] [decimal](18,2) NOT NULL,
    [CostoSiembrasArs] [decimal](18,2) NOT NULL,
    [CostoRiegosArs] [decimal](18,2) NOT NULL,
    [CostoPulverizacionesArs] [decimal](18,2) NOT NULL,
    [CostoCosechasArs] [decimal](18,2) NOT NULL,
    [CostoMonitoreosArs] [decimal](18,2) NOT NULL,
    [CostoFertilizantesArs] [decimal](18,2) NOT NULL,
    [CostoOtrasLaboresArs] [decimal](18,2) NOT NULL,
    [CostoAcopiosArs] [decimal](18,2) NOT NULL,
    
    [AnalisisSueloUsd] [decimal](18,2) NOT NULL,
    [CostoSiembrasUsd] [decimal](18,2) NOT NULL,
    [CostoRiegosUsd] [decimal](18,2) NOT NULL,
    [CostoPulverizacionesUsd] [decimal](18,2) NOT NULL,
    [CostoCosechasUsd] [decimal](18,2) NOT NULL,
    [CostoMonitoreosUsd] [decimal](18,2) NOT NULL,
    [CostoFertilizantesUsd] [decimal](18,2) NOT NULL,
    [CostoOtrasLaboresUsd] [decimal](18,2) NOT NULL,
    [CostoAcopiosUsd] [decimal](18,2) NOT NULL,
    
    GastosTotalesArs decimal(18,2) NOT NULL,
	GastosTotalesUsd decimal(18,2) NOT NULL,
	GastosPorCategoriaJson nvarchar(max) NOT NULL,
	 
    [LluviaAcumuladaTotal] [decimal](18,2) NOT NULL,
    [LluviasPorMesJson] [nvarchar](max) NULL,
    [EventosExtremosJson] [nvarchar](max) NULL,
    
    [ResumenPorCultivoJson] [nvarchar](max) NULL,
    [ResumenPorCampoJson] [nvarchar](max) NULL,
    [ResumenPorLoteJson] [nvarchar](max) NULL,
    
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    
    -- Constraints
    CONSTRAINT [PK_ReporteCierreCampania] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ReporteCierreCampania_Campania_IdCampania] FOREIGN KEY ([IdCampania])
        REFERENCES [dbo].[Campanias] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ReporteCierreCampania_Licencia_IdLicencia] FOREIGN KEY ([IdLicencia])
        REFERENCES [dbo].[Licencias] ([Id]) ON DELETE CASCADE
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];

CREATE TABLE Gastos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    TipoGasto INT NOT NULL DEFAULT 0,
    Observacion NVARCHAR(1000) NULL,
    Fecha DATE NOT NULL,
    Costo DECIMAL(18,2) NULL,
    CostoARS DECIMAL(18,2) NULL,
    CostoUSD DECIMAL(18,2) NULL,
    IdMoneda INT NOT NULL,
    IdCampania INT NOT NULL,
    IdLicencia INT NOT NULL,
    
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(100) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(100) NULL,
    
    -- Claves foráneas
    CONSTRAINT FK_Gastos_Monedas FOREIGN KEY (IdMoneda)
        REFERENCES Monedas(Id),
    CONSTRAINT FK_Gastos_Campanias FOREIGN KEY (IdCampania)
        REFERENCES Campanias(Id) ON DELETE NO ACTION,
    CONSTRAINT FK_Gastos_Licencias FOREIGN KEY (IdLicencia)
        REFERENCES Licencias(Id) ON DELETE CASCADE
);

GO


-- CamposLaborUnidad
CREATE TABLE CamposLaborUnidad (
    Id INT IDENTITY(1,1) NOT NULL,
    IdTipoActividad INT NOT NULL,
    NombreCampo NVARCHAR(100) NOT NULL,
    NombrePropiedad NVARCHAR(100) NOT NULL,
    Etiqueta NVARCHAR(200) NOT NULL DEFAULT '',
    Requerido BIT NOT NULL DEFAULT 0,
    Orden INT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    CONSTRAINT PK_CamposLaborUnidad PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_CamposLaborUnidad_TiposActividad FOREIGN KEY (IdTipoActividad)
        REFERENCES TiposActividad(Id),
    CONSTRAINT UQ_CamposLaborUnidad_IdTipoActividad_NombrePropiedad
        UNIQUE (IdTipoActividad, NombrePropiedad)
);
GO

-- CamposLaborUnidadPermitida
CREATE TABLE CamposLaborUnidadPermitida (
    Id INT IDENTITY(1,1) NOT NULL,
    IdCampoLaborUnidad INT NOT NULL,
    IdUnidadMedida INT NOT NULL,
    EsPredeterminado BIT NOT NULL DEFAULT 0,
    Orden INT NOT NULL DEFAULT 0,
    CONSTRAINT PK_CamposLaborUnidadPermitida PRIMARY KEY CLUSTERED (Id ASC),
    CONSTRAINT FK_CamposLaborUnidadPermitida_CampoLaborUnidad
        FOREIGN KEY (IdCampoLaborUnidad) REFERENCES CamposLaborUnidad(Id),
    CONSTRAINT FK_CamposLaborUnidadPermitida_UnidadesMedida
        FOREIGN KEY (IdUnidadMedida) REFERENCES UnidadesMedida(Id),
    CONSTRAINT UQ_CamposLaborUnidadPermitida_CampoUnidad
        UNIQUE (IdCampoLaborUnidad, IdUnidadMedida)
);
GO

CREATE UNIQUE INDEX IX_CamposLaborUnidadPermitida_Default
    ON CamposLaborUnidadPermitida (IdCampoLaborUnidad, EsPredeterminado)
    WHERE EsPredeterminado = 1;
GO


CREATE TABLE [dbo].[IndicesSatelitales] (
        [Id]              INT            IDENTITY(1,1) NOT NULL,
        [IdLote]          INT            NOT NULL,
        [IdLicencia]      INT            NULL,
        [FechaCaptura]    DATE           NOT NULL,
        [FechaConsulta]   DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        [Fuente]          VARCHAR(50)    NOT NULL DEFAULT 'Sentinel-2 (Copernicus)',
        [ResolucionMts]   INT            NOT NULL DEFAULT 10,
        [CloudCover]      DECIMAL(5,2)   NULL,
        [NDVI]            DECIMAL(5,3)   NULL,
        [NDWI]            DECIMAL(5,3)   NULL,
        [EVI]             DECIMAL(5,3)   NULL,
        [NDRE]            DECIMAL(5,3)   NULL,
        [SAVI]            DECIMAL(5,3)   NULL,
        [GNDVI]           DECIMAL(5,3)   NULL,
        [EsValido]        BIT            NOT NULL DEFAULT 1,
        [IdCampania]      INT            NULL,

        CONSTRAINT [PK_IndicesSatelitales] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_IndicesSatelitales_Lotes] FOREIGN KEY ([IdLote])
            REFERENCES [dbo].[Lotes] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_IndicesSatelitales_Licencias] FOREIGN KEY ([IdLicencia])
            REFERENCES [dbo].[Licencias] ([Id]),
        CONSTRAINT [FK_IndicesSatelitales_Campanias] FOREIGN KEY ([IdCampania])
            REFERENCES [dbo].[Campanias] ([Id])
    );
	
GO

CREATE TABLE [dbo].[LotesGeometria] (
        [Id]                       INT            IDENTITY(1,1) NOT NULL,
        [IdLote]                   INT            NOT NULL,
        [GeometriaOriginal]        NVARCHAR(MAX)  NOT NULL,
        [GeometriaSimplificada]    NVARCHAR(MAX)  NOT NULL,
        [ToleranciaSimplificacion] DECIMAL(10,6)  NULL,
        [AreaHa]                   DECIMAL(10,4)  NULL,
        [CentroLat]                DECIMAL(10,7)  NULL,
        [CentroLng]                DECIMAL(10,7)  NULL,
        [BoundsJson]               NVARCHAR(500)  NULL,
        [FechaCalculo]             DATETIME2      NOT NULL DEFAULT GETUTCDATE(),

        CONSTRAINT [PK_LotesGeometria] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_LotesGeometria_Lotes] FOREIGN KEY ([IdLote])
            REFERENCES [dbo].[Lotes] ([Id]) ON DELETE CASCADE
    );
	
GO

CREATE TABLE [dbo].[LogsConsultasSatelitales] (
        [Id]              BIGINT         IDENTITY(1,1) NOT NULL,
        [IdLote]          INT            NULL,
        [FechaConsulta]   DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        [TipoConsulta]    VARCHAR(50)    NOT NULL,
        [IndiceSolicitado] VARCHAR(50)   NOT NULL,
        [FechaDesde]      DATE           NULL,
        [FechaHasta]      DATE           NULL,
        [Parametros]      NVARCHAR(MAX)  NULL,
        [DuracionMs]      INT            NULL,
        [Exitoso]         BIT            NOT NULL DEFAULT 1,
        [ErrorMessage]    NVARCHAR(500)  NULL,
        [CostoEstimado]   DECIMAL(10,8)  NULL,  -- Siempre NULL en Copernicus (sin costo unitario)
        [IdLicencia]      INT            NULL,

        CONSTRAINT [PK_LogsConsultasSatelitales] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
	
	
	
	
