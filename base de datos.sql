CREATE DATABASE AgroForm;


USE AgroForm;
GO

CREATE TABLE Licencias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RazonSocial NVARCHAR(200) NOT NULL,
    NombreContacto NVARCHAR(100) NULL,
    NumeroContacto NVARCHAR(50) NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL
);

CREATE TABLE Usuarios (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    Rol INT NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    PasswordSalt VARBINARY(MAX) NOT NULL,
    EmailConfirmed BIT DEFAULT 0,
    PhoneNumber NVARCHAR(50) NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE NO ACTION
);

CREATE TABLE Campos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Ubicacion NVARCHAR(250) NULL,
    SuperficieHectareas DECIMAL(10,2) NULL,
    Latitud DECIMAL(18,2) NOT NULL,
    Longitud DECIMAL(18,2) NOT NULL,
	CoordenadasPoligono NVARCHAR(MAX) NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE NO ACTION
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
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE NO ACTION
);

CREATE TABLE Cultivos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(150) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    Orden INT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL
);

CREATE TABLE EstadosFenologicos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdCultivo INT NOT NULL,
    Codigo NVARCHAR(50) NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdCultivo) REFERENCES Cultivos(Id) ON DELETE CASCADE
);

CREATE TABLE Variedades (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdCultivo INT NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    Tipo INT NOT NULL, -- 0=Variedad, 1=Subproducto, 2=Descarte
    Activo BIT NOT NULL DEFAULT 1,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdCultivo) REFERENCES Cultivos(Id) ON DELETE CASCADE
);

CREATE TABLE Catalogos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Tipo INT NOT NULL, -- Según enum TipoCatalogo
    Nombre NVARCHAR(150) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL
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
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE NO ACTION
);


CREATE TABLE Marcas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    Codigo NVARCHAR(50) NOT NULL UNIQUE,
    Descripcion NVARCHAR(150) NOT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(100) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(100) NULL,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE NO ACTION
);

CREATE TABLE Proveedores (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    RazonSocial NVARCHAR(200) NOT NULL,
    NombreFantasia NVARCHAR(150) NULL,
    CUIT NVARCHAR(20) NULL,
    Direccion NVARCHAR(250) NULL,
    Localidad NVARCHAR(150) NULL,
    Provincia NVARCHAR(150) NULL,
    Telefono NVARCHAR(50) NULL,
    Email NVARCHAR(150) NULL,
    NombreContacto NVARCHAR(150) NULL,
    Observaciones NVARCHAR(500) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(100) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(100) NULL,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE NO ACTION
);

CREATE TABLE Monedas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Codigo NVARCHAR(10) NOT NULL UNIQUE,
    Nombre NVARCHAR(100) NOT NULL,
    Simbolo NVARCHAR(10) NULL,
    TipoCambioReferencia DECIMAL(18,4) NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL
);

CREATE TABLE TiposInsumo (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL
);

CREATE TABLE Insumos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    Descripcion NVARCHAR(500) NULL,
    UnidadMedida NVARCHAR(50) NULL,
    IdMarca INT NULL,
    IdProveedor INT NULL,
    IdTipoInsumo INT NULL,
    StockActual DECIMAL(18,2) NULL,
    StockMinimo DECIMAL(18,2) NULL,
    PrecioActual DECIMAL(18,2) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    IdMoneda INT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdMarca) REFERENCES Marcas(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdProveedor) REFERENCES Proveedores(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdTipoInsumo) REFERENCES TiposInsumo(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION
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
    DensidadSemillaKgHa DECIMAL(10,2) NULL,
    Costo DECIMAL(18,2) NULL,
	CostoARS DECIMAL(18,2) NULL,
    CostoUSD DECIMAL(18,2) NULL,
    IdCultivo INT NOT NULL,
    IdVariedad INT NULL,
    IdMetodoSiembra INT NOT NULL,
	IdMoneda INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    
    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id),
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id),
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
    FOREIGN KEY (IdCultivo) REFERENCES Cultivos(Id),
    FOREIGN KEY (IdVariedad) REFERENCES Variedades(Id),
    FOREIGN KEY (IdMetodoSiembra) REFERENCES Catalogos(Id),
	FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION
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
    Costo DECIMAL(18,2) NULL,
	CostoARS DECIMAL(18,2) NULL,
    CostoUSD DECIMAL(18,2) NULL,
    IdMetodoRiego INT NULL,
    IdFuenteAgua INT NULL,
	IdMoneda INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    
    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id),
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id),
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
    FOREIGN KEY (IdMetodoRiego) REFERENCES Catalogos(Id),
    FOREIGN KEY (IdFuenteAgua) REFERENCES Catalogos(Id),
	FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION
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
    IdNutriente INT NULL,
    IdTipoFertilizante INT NULL,
    DosisKgHa DECIMAL(10,2) NULL,
    Costo DECIMAL(18,2) NULL,
	CostoARS DECIMAL(18,2) NULL,
    CostoUSD DECIMAL(18,2) NULL,
    IdMetodoAplicacion INT NOT NULL,
	IdMoneda INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,

    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id),
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id),
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
    FOREIGN KEY (IdNutriente) REFERENCES Catalogos(Id),
    FOREIGN KEY (IdTipoFertilizante) REFERENCES Catalogos(Id),
    FOREIGN KEY (IdMetodoAplicacion) REFERENCES Catalogos(Id),
	FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION
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
    Dosis DECIMAL(10,2) NULL,
    CondicionesClimaticas NVARCHAR(200) NULL,
    Costo DECIMAL(18,2) NULL,
	CostoARS DECIMAL(18,2) NULL,
    CostoUSD DECIMAL(18,2) NULL,
    IdProductoAgroquimico INT NULL,
	IdMoneda INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,

    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id),
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id),
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
    FOREIGN KEY (IdProductoAgroquimico) REFERENCES Catalogos(Id),
	FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION
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
    IdLote INT NOT NULL,
    IdTipoActividad INT NOT NULL,
    IdUsuario INT NULL,
    IdInsumo INT NULL,
    IdEstadoFenologico INT NULL,
    IdTipoMonitoreo INT NOT NULL,
	IdMoneda INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,

    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id),
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id),
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
    FOREIGN KEY (IdInsumo) REFERENCES Insumos(Id),
    FOREIGN KEY (IdEstadoFenologico) REFERENCES EstadosFenologicos(Id),
    FOREIGN KEY (IdTipoMonitoreo) REFERENCES Catalogos(Id),
	FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION
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
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,

    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id),
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id),
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
    FOREIGN KEY (IdLaboratorio) REFERENCES Catalogos(Id),
	FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION
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
    HumedadGrano DECIMAL(5,2) NULL,
    SuperficieCosechadaHa DECIMAL(10,2) NULL,
    Costo DECIMAL(18,2) NULL,
	CostoARS DECIMAL(18,2) NULL,
    CostoUSD DECIMAL(18,2) NULL,
    IdCultivo INT NOT NULL,
	IdMoneda INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,

    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id),
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id),
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
    FOREIGN KEY (IdCultivo) REFERENCES Cultivos(Id),
	FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION
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
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,

    FOREIGN KEY (IdCampania) REFERENCES Campanias(Id),
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id),
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id),
	FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION
);


CREATE TABLE RegistrosClima (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    Fecha DATETIME NOT NULL,
    Milimetros DECIMAL(10,2) NOT NULL,
    IdCampo INT NOT NULL,
    TipoClima INT NOT NULL,
    Observaciones NVARCHAR(MAX) NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdCampo) REFERENCES Campos(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE NO ACTION
);


CREATE TABLE TiposActividad (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Icono NVARCHAR(50) NOT NULL,
    ColorIcono NVARCHAR(20),
    IdTipoInsumo INT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdTipoInsumo) REFERENCES TiposInsumo(Id)
);


CREATE TABLE HistoricosPrecioInsumo (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    IdInsumo INT NOT NULL,
    IdMoneda INT NOT NULL,
    Precio DECIMAL(18,2) NOT NULL,
    FechaDesde DATETIME NOT NULL,
    FechaHasta DATETIME NULL,
    Observaciones NVARCHAR(500) NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdInsumo) REFERENCES Insumos(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdMoneda) REFERENCES Monedas(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE NO ACTION
);

CREATE TABLE [dbo].[ReporteCierreCampania](
    [Id] [int] IDENTITY(1,1) NOT NULL,
    [IdLicencia] [int] NOT NULL,
    [FechaCreacion] [datetime2](7) NOT NULL,
    [FechaModificacion] [datetime2](7) NULL,
    [UsuarioCreacion] [nvarchar](256) NULL,
    [UsuarioModificacion] [nvarchar](256) NULL,
    [Activo] [bit] NOT NULL,
    
    [IdCampania] [int] NOT NULL,
    
    [NombreCampania] [nvarchar](500) NOT NULL,
    [FechaInicio] [datetime2](7) NOT NULL,
    [FechaFin] [datetime2](7) NOT NULL,
    
    [SuperficieTotalHa] [decimal](18,2) NOT NULL,
    [ToneladasProducidas] [decimal](18,2) NOT NULL,
    [CostoPorHa] [decimal](18,2) NOT NULL,
    [CostoPorTonelada] [decimal](18,2) NOT NULL,
    [RendimientoPromedioHa] [decimal](18,2) NOT NULL,
    
    [AnalisisSueloArs] [decimal](18,2) NOT NULL,
    [CostoSiembrasArs] [decimal](18,2) NOT NULL,
    [CostoRiegosArs] [decimal](18,2) NOT NULL,
    [CostoPulverizacionesArs] [decimal](18,2) NOT NULL,
    [CostoCosechasArs] [decimal](18,2) NOT NULL,
    [CostoMonitoreosArs] [decimal](18,2) NOT NULL,
    [CostoFertilizantesArs] [decimal](18,2) NOT NULL,
    [CostoOtrasLaboresArs] [decimal](18,2) NOT NULL,
    
    [AnalisisSueloUsd] [decimal](18,2) NOT NULL,
    [CostoSiembrasUsd] [decimal](18,2) NOT NULL,
    [CostoRiegosUsd] [decimal](18,2) NOT NULL,
    [CostoPulverizacionesUsd] [decimal](18,2) NOT NULL,
    [CostoCosechasUsd] [decimal](18,2) NOT NULL,
    [CostoMonitoreosUsd] [decimal](18,2) NOT NULL,
    [CostoFertilizantesUsd] [decimal](18,2) NOT NULL,
    [CostoOtrasLaboresUsd] [decimal](18,2) NOT NULL,
    
    [LluviaAcumuladaTotal] [decimal](18,2) NOT NULL,
    [LluviasPorMesJson] [nvarchar](max) NULL,
    [EventosExtremosJson] [nvarchar](max) NULL,
    
    [ResumenPorCultivoJson] [nvarchar](max) NULL,
    [ResumenPorCampoJson] [nvarchar](max) NULL,
    [ResumenPorLoteJson] [nvarchar](max) NULL,
    
    [EsDefinitivo] [bit] NOT NULL,
	
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    
    -- Constraints
    CONSTRAINT [PK_ReporteCierreCampania] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_ReporteCierreCampania_Campania_IdCampania] FOREIGN KEY ([IdCampania]) 
        REFERENCES [dbo].[Campania] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ReporteCierreCampania_Licencia_IdLicencia] FOREIGN KEY ([IdLicencia]) 
        REFERENCES [dbo].[Licencia] ([Id])
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY];







-- Primero, cambiar a otra base de datos
USE master;
GO

-- Cerrar todas las conexiones a la base de datos AgroForm
ALTER DATABASE AgroForm SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- Ahora eliminar la base de datos
DROP DATABASE AgroForm;
GO
