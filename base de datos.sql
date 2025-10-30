CREATE DATABASE AgroForm;

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
    Rol int not NULL,
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
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE NO ACTION
);

CREATE TABLE Campanias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
	IdLicencia INT NOT NULL,
	EstadosCamapaña INT NOT NULL
    Nombre NVARCHAR(150) NOT NULL,
    FechaInicio DATETIME NOT NULL,
    FechaFin DATETIME NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE NO ACTION
);

CREATE TABLE Lotes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
	IdLicencia INT NOT NULL,
    Nombre NVARCHAR(150) NOT NULL,
    SuperficieHectareas DECIMAL(10,2) NULL,
    CampoId INT NOT NULL,
    CampaniaId INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (CampoId) REFERENCES Campos(Id) ON DELETE NO ACTION,
    FOREIGN KEY (CampaniaId) REFERENCES Campanias(Id) ON DELETE NO ACTION,
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

CREATE TABLE RegistrosClima (
    Id INT IDENTITY(1,1) PRIMARY KEY,
	IdLicencia INT NOT NULL,
    Fecha DATETIME NOT NULL,
    Milimetros DECIMAL(10,2) NOT NULL,
    LoteId INT NOT NULL,
	TipoClima INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (LoteId) REFERENCES Lotes(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE NO ACTION
);

CREATE TABLE TiposActividad (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Icono NVARCHAR(150) NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL
);

CREATE TABLE Actividades (
    Id INT IDENTITY(1,1) PRIMARY KEY,
	IdLicencia INT NOT NULL,
    Descripcion NVARCHAR(500) NOT NULL,
    Fecha DATETIME NOT NULL,
	Observacion NVARCHAR(max) NULL,
    LoteId INT NOT NULL,
    TipoActividadId INT NOT NULL,
    UsuarioId INT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (LoteId) REFERENCES Lotes(Id) ON DELETE CASCADE,
    FOREIGN KEY (TipoActividadId) REFERENCES TiposActividad(Id) ON DELETE NO ACTION,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE NO ACTION,
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
	MarcaId INT NULL,
    ProveedorId INT NULL,
    TipoInsumoId INT NULL,
    StockActual DECIMAL(18,2) NULL,
    StockMinimo DECIMAL(18,2) NULL,
    Estado BIT NOT NULL DEFAULT 1,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
	FOREIGN KEY (MarcaId) REFERENCES Marcas(Id) ON DELETE NO ACTION,
    FOREIGN KEY (ProveedorId) REFERENCES Proveedores(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE NO ACTION,
    FOREIGN KEY (TipoInsumoId) REFERENCES TiposInsumo(Id) ON DELETE NO ACTION
);

CREATE TABLE MovimientosInsumo (
    Id INT IDENTITY(1,1) PRIMARY KEY,
	IdLicencia INT NOT NULL,
	ActividadId INT NOT NULL UNIQUE, 
    InsumoId INT NOT NULL,
    MonedaId INT NOT NULL,
    UsuarioId INT NOT NULL,
    Cantidad DECIMAL(18,2) NOT NULL,
    CostoUnitario DECIMAL(18,2) NOT NULL,
    EsEntrada BIT NOT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (ActividadId) REFERENCES Actividades(Id) ON DELETE CASCADE,
    FOREIGN KEY (InsumoId) REFERENCES Insumos(Id) ON DELETE NO ACTION,
    FOREIGN KEY (MonedaId) REFERENCES Monedas(Id) ON DELETE NO ACTION,
    FOREIGN KEY (UsuarioId) REFERENCES Usuarios(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE NO ACTION
);

CREATE TABLE HistoricosPrecioInsumo (
    Id INT IDENTITY(1,1) PRIMARY KEY,
	IdLicencia INT NOT NULL,
    InsumoId INT NOT NULL,
    MonedaId INT NOT NULL,
    Precio DECIMAL(18,2) NOT NULL,
    FechaDesde DATETIME NOT NULL,
    FechaHasta DATETIME NULL,
    Observaciones NVARCHAR(500) NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (InsumoId) REFERENCES Insumos(Id) ON DELETE CASCADE,
    FOREIGN KEY (MonedaId) REFERENCES Monedas(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE NO ACTION
);

