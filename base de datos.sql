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
	EstadosCamapaña INT NOT NULL,
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

CREATE TABLE RegistrosClima (
    Id INT IDENTITY(1,1) PRIMARY KEY,
	IdLicencia INT NOT NULL,
    Fecha DATETIME NOT NULL,
    Milimetros DECIMAL(10,2) NOT NULL,
    IdLote INT NOT NULL,
	TipoClima INT NOT NULL,
	Observaciones NVARCHAR(max) NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE CASCADE,
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
    CONSTRAINT FK_TipoInsumo FOREIGN KEY (IdTipoInsumo)
        REFERENCES TiposInsumo(Id)
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

CREATE TABLE Actividades (
    Id INT IDENTITY(1,1) PRIMARY KEY,
	IdLicencia INT NOT NULL,
    Fecha DATETIME NOT NULL,
	Observacion NVARCHAR(max) NULL,
	Cantidad DECIMAL(18,2) NULL,
    IdLote INT NOT NULL,
    IdTipoActividad INT NOT NULL,
    IdUsuario INT NOT NULL,
	IdInsumo INT NULL,
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    FOREIGN KEY (IdLote) REFERENCES Lotes(Id) ON DELETE CASCADE,
    FOREIGN KEY (IdTipoActividad) REFERENCES TiposActividad(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdUsuario) REFERENCES Usuarios(Id) ON DELETE NO ACTION,
    FOREIGN KEY (IdLicencia) REFERENCES Licencias(Id) ON DELETE NO ACTION,
	FOREIGN KEY (IdInsumo) REFERENCES Insumos(Id) ON DELETE NO ACTION
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








-- Primero, cambiar a otra base de datos
USE master;
GO

-- Cerrar todas las conexiones a la base de datos AgroForm
ALTER DATABASE AgroForm SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- Ahora eliminar la base de datos
DROP DATABASE AgroForm;
GO