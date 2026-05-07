# Base de Datos - Esquema y Configuración

## Motor de Base de Datos

### SQL Server
- **Versión**: SQL Server 2019+ compatible
- **ORM**: Entity Framework Core 9.0.10
- **Approach**: Code-First con Migrations
- **Connection**: Configurable por ambiente

### Connection Strings

#### Development
```json
{
  "ConnectionStrings": {
    "SQL": "Server=localhost\\SQLEXPRESS;Database=AgroForm;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

#### Production
```json
{
  "ConnectionStrings": {
    "SQL": "Server=tcp:TestAgroForm.mssql.somee.com,1433;Initial Catalog=TestAgroForm;User ID=andresgarcia_SQLLogin_1;Password=pcot6d83il;TrustServerCertificate=True;"
  }
}
```

## Esquema de Base de Datos

### Tablas Principales

#### 1. Licencias (Multi-tenancy)
```sql
CREATE TABLE Licencias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    RazonSocial NVARCHAR(200) NOT NULL,
    NombreContacto NVARCHAR(100) NULL,
    NumeroContacto NVARCHAR(50) NULL,
    TipoLicencia INT NOT NULL DEFAULT 0,
    EsPrueba BIT NOT NULL DEFAULT 0,
    FechaFinPrueba DATETIME NULL,
    Activo BIT NOT NULL DEFAULT 1,
    
    -- Auditoría
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL
);
```

#### 2. Usuarios
```sql
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
    SuperAdmin BIT DEFAULT 0,
    PhoneNumber NVARCHAR(50) NULL,
    
    -- Auditoría
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    
    CONSTRAINT FK_Usuarios_Licencias FOREIGN KEY (IdLicencia) 
        REFERENCES Licencias(Id) ON DELETE CASCADE
);
```

#### 3. Campañas
```sql
CREATE TABLE Campanias (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    Nombre NVARCHAR(200) NOT NULL,
    FechaInicio DATETIME NOT NULL,
    FechaFin DATETIME NULL,
    Activa BIT NOT NULL DEFAULT 1,
    Cerrada BIT NOT NULL DEFAULT 0,
    
    -- Auditoría
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    
    CONSTRAINT FK_Campanias_Licencias FOREIGN KEY (IdLicencia) 
        REFERENCES Licencias(Id) ON DELETE CASCADE
);
```

#### 4. Campos
```sql
CREATE TABLE Campos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    IdCampania INT NULL,
    Nombre NVARCHAR(200) NOT NULL,
    Superficie DECIMAL(18,2) NOT NULL,
    Ubicacion NVARCHAR(500) NULL,
    Descripcion NVARCHAR(MAX) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    
    -- Auditoría
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    
    CONSTRAINT FK_Campos_Licencias FOREIGN KEY (IdLicencia) 
        REFERENCES Licencias(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Campos_Campanias FOREIGN KEY (IdCampania) 
        REFERENCES Campanias(Id) ON DELETE SET NULL
);
```

#### 5. Lotes
```sql
CREATE TABLE Lotes (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    IdCampo INT NOT NULL,
    IdCampania INT NOT NULL,
    Nombre NVARCHAR(200) NOT NULL,
    Superficie DECIMAL(18,2) NOT NULL,
    NumeroLote NVARCHAR(50) NULL,
    Coordenadas NVARCHAR(MAX) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    
    -- Auditoría
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    
    CONSTRAINT FK_Lotes_Licencias FOREIGN KEY (IdLicencia) 
        REFERENCES Licencias(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Lotes_Campos FOREIGN KEY (IdCampo) 
        REFERENCES Campos(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Lotes_Campanias FOREIGN KEY (IdCampania) 
        REFERENCES Campanias(Id) ON DELETE CASCADE
);
```

#### 6. Cultivos
```sql
CREATE TABLE Cultivos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(MAX) NULL,
    CicloDias INT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    
    -- Auditoría
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL
);
```

#### 7. Variedades
```sql
CREATE TABLE Variedades (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdCultivo INT NOT NULL,
    Nombre NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(MAX) NULL,
    PotenciaRinde DECIMAL(18,2) NULL,
    DíasACosecha INT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    
    -- Auditoría
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    
    CONSTRAINT FK_Variedades_Cultivos FOREIGN KEY (IdCultivo) 
        REFERENCES Cultivos(Id) ON DELETE CASCADE
);
```

#### 8. Actividades
```sql
CREATE TABLE Actividades (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    IdCampania INT NOT NULL,
    IdCampo INT NULL,
    IdLote INT NULL,
    IdTipoActividad INT NOT NULL,
    IdCultivo INT NULL,
    IdVariedad INT NULL,
    FechaActividad DATETIME NOT NULL,
    Descripcion NVARCHAR(MAX) NULL,
    Superficie REALizada DECIMAL(18,2) NULL,
    CostoDirecto DECIMAL(18,2) NULL,
    Observaciones NVARCHAR(MAX) NULL,
    Estado INT NOT NULL DEFAULT 1,
    
    -- Auditoría
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    
    CONSTRAINT FK_Actividades_Licencias FOREIGN KEY (IdLicencia) 
        REFERENCES Licencias(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Actividades_Campanias FOREIGN KEY (IdCampania) 
        REFERENCES Campanias(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Actividades_Campos FOREIGN KEY (IdCampo) 
        REFERENCES Campos(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Actividades_Lotes FOREIGN KEY (IdLote) 
        REFERENCES Lotes(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Actividades_Cultivos FOREIGN KEY (IdCultivo) 
        REFERENCES Cultivos(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Actividades_Variedades FOREIGN KEY (IdVariedad) 
        REFERENCES Variedades(Id) ON DELETE SET NULL
);
```

#### 9. Tipos de Actividad
```sql
CREATE TABLE TiposActividad (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(MAX) NULL,
    Categoria NVARCHAR(100) NULL,
    Activo BIT NOT NULL DEFAULT 1,
    
    -- Auditoría
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL
);
```

#### 10. Gastos
```sql
CREATE TABLE Gastos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    IdCampania INT NOT NULL,
    IdCampo INT NULL,
    IdLote INT NULL,
    IdActividad INT NULL,
    IdTipoGasto INT NOT NULL,
    IdMoneda INT NOT NULL,
    FechaGasto DATETIME NOT NULL,
    Descripcion NVARCHAR(500) NOT NULL,
    Importe DECIMAL(18,2) NOT NULL,
    Proveedor NVARCHAR(300) NULL,
    Comprobante NVARCHAR(100) NULL,
    Observaciones NVARCHAR(MAX) NULL,
    
    -- Auditoría
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    
    CONSTRAINT FK_Gastos_Licencias FOREIGN KEY (IdLicencia) 
        REFERENCES Licencias(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Gastos_Campanias FOREIGN KEY (IdCampania) 
        REFERENCES Campanias(Id) ON DELETE CASCADE,
    CONSTRAINT FK_Gastos_Campos FOREIGN KEY (IdCampo) 
        REFERENCES Campos(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Gastos_Lotes FOREIGN KEY (IdLote) 
        REFERENCES Lotes(Id) ON DELETE SET NULL,
    CONSTRAINT FK_Gastos_Actividades FOREIGN KEY (IdActividad) 
        REFERENCES Actividades(Id) ON DELETE SET NULL
);
```

#### 11. Monedas
```sql
CREATE TABLE Monedas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    Simbolo NVARCHAR(10) NOT NULL,
    Cotizacion DECIMAL(18,4) NOT NULL DEFAULT 1,
    EsMonedaBase BIT NOT NULL DEFAULT 0,
    Activa BIT NOT NULL DEFAULT 1,
    
    -- Auditoría
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL
);
```

#### 12. Estados Fenológicos
```sql
CREATE TABLE EstadosFenologicos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdCultivo INT NOT NULL,
    Nombre NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(MAX) NULL,
    Orden INT NOT NULL,
    DiasDesdeSiembra INT NULL,
    Activo BIT NOT NULL DEFAULT 1,
    
    -- Auditoría
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    
    CONSTRAINT FK_EstadosFenologicos_Cultivos FOREIGN KEY (IdCultivo) 
        REFERENCES Cultivos(Id) ON DELETE CASCADE
);
```

#### 13. Registro Clima
```sql
CREATE TABLE RegistroClima (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    IdLicencia INT NOT NULL,
    Fecha DATETIME NOT NULL,
    TemperaturaMaxima DECIMAL(5,2) NULL,
    TemperaturaMinima DECIMAL(5,2) NULL,
    Precipitacion DECIMAL(8,2) NULL,
    HumedadRelativa INT NULL,
    VelocidadViento DECIMAL(5,2) NULL,
    Observaciones NVARCHAR(MAX) NULL,
    
    -- Auditoría
    RegistrationDate DATETIME NULL,
    RegistrationUser NVARCHAR(150) NULL,
    ModificationDate DATETIME NULL,
    ModificationUser NVARCHAR(150) NULL,
    
    CONSTRAINT FK_RegistroClima_Licencias FOREIGN KEY (IdLicencia) 
        REFERENCES Licencias(Id) ON DELETE CASCADE
);
```

## Entity Framework Core Configuration

### AppDbContext
```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSets
    public DbSet<Licencia> Licencias { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Campania> Campanias { get; set; }
    public DbSet<Campo> Campos { get; set; }
    public DbSet<Lote> Lotes { get; set; }
    public DbSet<Cultivo> Cultivos { get; set; }
    public DbSet<Variedad> Variedades { get; set; }
    public DbSet<Actividad> Actividades { get; set; }
    public DbSet<TipoActividad> TiposActividad { get; set; }
    public DbSet<Gasto> Gastos { get; set; }
    public DbSet<Moneda> Monedas { get; set; }
    public DbSet<EstadoFenologico> EstadosFenologicos { get; set; }
    public DbSet<RegistroClima> RegistroClima { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configuración de entidades
        ConfigureLicencias(modelBuilder);
        ConfigureUsuarios(modelBuilder);
        ConfigureCampanias(modelBuilder);
        // ... más configuraciones
        
        // Configuración global de auditoría
        ConfigureAuditableEntities(modelBuilder);
    }

    private void ConfigureAuditableEntities(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(EntityBase).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(EntityBase.RegistrationDate))
                    .ValueGeneratedOnAdd();
                    
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(EntityBase.ModificationDate))
                    .ValueGeneratedOnUpdate();
            }
        }
    }
}
```

### Entity Configurations
```csharp
// Configuración de Licencia
modelBuilder.Entity<Licencia>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.RazonSocial).IsRequired().HasMaxLength(200);
    entity.Property(e => e.NombreContacto).HasMaxLength(100);
    entity.Property(e => e.NumeroContacto).HasMaxLength(50);
    entity.Property(e => e.Activo).HasDefaultValue(true);
    entity.Property(e => e.EsPrueba).HasDefaultValue(false);
    entity.Property(e => e.TipoLicencia).HasDefaultValue(0);
});

// Configuración de Usuario
modelBuilder.Entity<Usuario>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
    entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
    entity.Property(e => e.Activo).HasDefaultValue(true);
    entity.Property(e => e.EmailConfirmed).HasDefaultValue(false);
    entity.Property(e => e.SuperAdmin).HasDefaultValue(false);
    
    entity.HasOne(e => e.Licencia)
          .WithMany()
          .HasForeignKey(e => e.IdLicencia)
          .OnDelete(DeleteBehavior.Cascade);
});

// Configuración de Campo
modelBuilder.Entity<Campo>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
    entity.Property(e => e.Superficie).HasPrecision(18, 2);
    entity.Property(e => e.Ubicacion).HasMaxLength(500);
    entity.Property(e => e.Activo).HasDefaultValue(true);
    
    entity.HasOne(e => e.Licencia)
          .WithMany()
          .HasForeignKey(e => e.IdLicencia)
          .OnDelete(DeleteBehavior.Cascade);
          
    entity.HasOne(e => e.Campania)
          .WithMany()
          .HasForeignKey(e => e.IdCampania)
          .OnDelete(DeleteBehavior.SetNull);
});
```

## Migrations Strategy

### Code-First Approach
```bash
# Crear nueva migration
dotnet ef migrations add AddNuevaEntidad

# Aplicar migration a base de datos
dotnet ef database update

# Generar script SQL
dotnet ef migrations script
```

### Seed Data
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // Seed de datos iniciales
    modelBuilder.Entity<Moneda>().HasData(
        new Moneda { Id = 1, Nombre = "Peso Argentino", Simbolo = "$", Cotizacion = 1, EsMonedaBase = true, Activa = true },
        new Moneda { Id = 2, Nombre = "Dólar Estadounidense", Simbolo = "USD", Cotizacion = 1000, EsMonedaBase = false, Activa = true }
    );
    
    modelBuilder.Entity<TipoActividad>().HasData(
        new TipoActividad { Id = 1, Nombre = "Siembra", Categoria = "Cultivo", Activo = true },
        new TipoActividad { Id = 2, Nombre = "Cosecha", Categoria = "Cultivo", Activo = true },
        new TipoActividad { Id = 3, Nombre = "Fertilización", Categoria = "Nutrición", Activo = true }
    );
}
```

## Índices y Performance

### Índices Recomendados
```sql
-- Índices para búsquedas frecuentes
CREATE INDEX IX_Usuarios_IdLicencia_Email ON Usuarios(IdLicencia, Email);
CREATE INDEX IX_Campanias_IdLicencia_Activa ON Campanias(IdLicencia, Activa);
CREATE INDEX IX_Campos_IdLicencia_IdCampania ON Campos(IdLicencia, IdCampania);
CREATE INDEX IX_Lotes_IdCampo_IdCampania ON Lotes(IdCampo, IdCampania);
CREATE INDEX IX_Actividades_IdCampania_FechaActividad ON Actividades(IdCampania, FechaActividad);
CREATE INDEX IX_Gastos_IdCampania_FechaGasto ON Gastos(IdCampania, FechaGasto);

-- Índices para auditoría
CREATE INDEX IX_Entidades_RegistrationDate ON [TableName](RegistrationDate);
CREATE INDEX IX_Entities_ModificationDate ON [TableName](ModificationDate);
```

### Query Optimization
```csharp
// Configuración de Entity Framework
options.UseSqlServer(connectionString, sqlOptions =>
{
    sqlOptions.CommandTimeout(60);
    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
    sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
});

// Ejemplo de consulta optimizada
var campos = await _context.Campos
    .Include(c => c.Campania)
    .Include(c => c.Lotes.Take(10)) // Limitar includes
    .Where(c => c.IdLicencia == idLicencia && c.Activo)
    .AsNoTracking() // Solo lectura
    .ToListAsync();
```

## Backup y Recovery

### Backup Strategy
```sql
-- Backup completo diario
BACKUP DATABASE AgroForm 
TO DISK = 'C:\Backups\AgroForm_Full_YYYYMMDD.bak'
WITH COMPRESSION, CHECKSUM;

-- Backup differential cada 6 horas
BACKUP DATABASE AgroForm 
TO DISK = 'C:\Backups\AgroForm_Diff_YYYYMMDD_HH.bak'
WITH DIFFERENTIAL, COMPRESSION, CHECKSUM;

-- Backup log cada 15 minutos (si es Full Recovery)
BACKUP LOG AgroForm 
TO DISK = 'C:\Backups\AgroForm_Log_YYYYMMDD_HHMM.trn';
```

### Recovery Model
```sql
-- Recovery Model recomendado: SIMPLE para desarrollo, FULL para producción
ALTER DATABASE AgroForm SET RECOVERY SIMPLE;
-- o
ALTER DATABASE AgroForm SET RECOVERY FULL;
```

## Security

### Data Encryption
```csharp
// Configuración de columnas encriptadas (SQL Server 2019+)
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Usuario>(entity =>
    {
        entity.Property(e => e.PasswordHash)
              .HasConversion(
                  v => Convert.ToBase64String(DataProtection.Protect(Encoding.UTF8.GetBytes(v))),
                  v => Encoding.UTF8.GetString(DataProtection.Unprotect(Convert.FromBase64String(v)))
              );
    });
}
```

### Row Level Security
```sql
-- Implementar Row Level Security para multi-tenancy
CREATE SECURITY POLICY LicenciaPolicy
ADD FILTER PREDICATE dbo.fn_LicenciaFilter(IdLicencia) ON dbo.Campos,
ADD BLOCK PREDICATE dbo.fn_LicenciaFilter(IdLicencia) ON dbo.Campos;

CREATE FUNCTION dbo.fn_LicenciaFilter(@IdLicencia INT)
RETURNS TABLE
WITH SCHEMABINDING
AS
RETURN
SELECT 1 AS fn_result
WHERE @IdLicencia = SESSION_CONTEXT(N'IdLicencia');
```

## Monitoring y Maintenance

### Database Monitoring
```sql
-- Consultas para monitoreo
SELECT 
    database_name,
    log_reuse_wait_desc,
    log_size_mb,
    log_used_mb,
    log_used_percent
FROM sys.dm_db_log_space_usage;

-- Monitoreo de conexiones
SELECT 
    session_id,
    login_name,
    host_name,
    program_name,
    status,
    cpu_time,
    memory_usage
FROM sys.dm_exec_sessions
WHERE is_user_process = 1;
```

### Maintenance Jobs
```sql
-- Rebuild de índices (semanal)
EXEC sp_MSforeachtable 'ALTER INDEX ALL ON ? REBUILD';

-- Update statistics (diario)
EXEC sp_MSforeachtable 'UPDATE STATISTICS ?';

-- Check integrity (semanal)
DBCC CHECKDB('AgroForm') WITH NO_INFOMSGS;
```

## Data Archiving

### Archiving Strategy
```sql
-- Tabla de archivo para actividades históricas
CREATE TABLE Actividades_Archivo (
    -- Misma estructura que Actividades
    ArchivoDate DATETIME DEFAULT GETDATE()
);

-- Procedimiento de archivo
CREATE PROCEDURE sp_ArchivarActividades
    @FechaCorte DATETIME
AS
BEGIN
    INSERT INTO Actividades_Archivo
    SELECT *, GETDATE() as ArchivoDate
    FROM Actividades
    WHERE FechaActividad < @FechaCorte;
    
    DELETE FROM Actividades
    WHERE FechaActividad < @FechaCorte;
END;
```

## Connection Pooling Configuration

### EF Core Configuration
```csharp
services.AddDbContextFactory<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(60);
        sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
    });
    
    // Configuración de connection pooling
    options.EnableServiceProviderCaching();
    options.EnableSensitiveDataLogging(false);
});
```

## Testing Database

### In-Memory Database para Tests
```csharp
services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("TestDb");
});

// O con SQLite
services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("DataSource=:memory:");
});
```
