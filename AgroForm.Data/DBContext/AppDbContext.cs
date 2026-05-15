using AgroForm.Model;
using AgroForm.Model.Actividades;
using AgroForm.Model.Satelital;
using AgroForm.Model.Unidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;

namespace AgroForm.Data.DBContext
{
    public partial class AppDbContext : DbContext
    {
        public readonly ILogger<AppDbContext> _logger;
        private readonly IUserContext _userContext;

        public AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext> logger, IUserContext userContext)
            : base(options)
        {
            _logger = logger;
            _userContext = userContext;
        }

        public DbSet<Licencia> Licencias { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<PagoLicencia> PagoLicencias { get; set; }
        public DbSet<Campo> Campos { get; set; }
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<Campania> Campanias { get; set; }
        public DbSet<RegistroClima> RegistrosClima { get; set; }
        public DbSet<TipoActividad> TiposActividad { get; set; }
        public DbSet<Moneda> Monedas { get; set; }

        public DbSet<Cultivo> Cultivos { get; set; }
        public DbSet<EstadoFenologico> EstadosFenologicos { get; set; }
        public DbSet<Catalogo> Catalogos { get; set; }
        public DbSet<LicenciasCultivos> LicenciasCultivos { get; set; }
        public DbSet<LicenciasCatalogos> LicenciasCatalogos { get; set; }

        public DbSet<Siembra> Siembras { get; set; }
        public DbSet<Riego> Riegos { get; set; }
        public DbSet<Fertilizacion> Fertilizaciones { get; set; }
        public DbSet<Pulverizacion> Pulverizaciones { get; set; }
        public DbSet<Monitoreo> Monitoreos { get; set; }
        public DbSet<AnalisisSuelo> AnalisisSuelos { get; set; }
        public DbSet<Cosecha> Cosechas { get; set; }
        public DbSet<OtraLabor> OtrasLabores { get; set; }
        public DbSet<Acopio> Acopios { get; set; }
        public DbSet<CicloCultivo> CicloCultivos { get; set; }
        public DbSet<ReporteCierreCampania> ReportesCierreCampania { get; set; }
        public DbSet<Gasto> Gastos { get; set; }

        // Satelital
        public DbSet<IndiceSatelital> IndicesSatelitales { get; set; }
        public DbSet<LoteGeometria> LotesGeometria { get; set; }
        public DbSet<LogConsultaSatelital> LogsConsultasSatelitales { get; set; }

        public DbSet<UnidadMedida> UnidadesMedida { get; set; }
        public DbSet<CampoLaborUnidad> CamposLaborUnidad { get; set; }
        public DbSet<CampoLaborUnidadPermitida> CamposLaborUnidadPermitida { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===============================
            // Configuraciones generales
            // ===============================
            // Nota: Las configuraciones están definidas directamente en OnModelCreating
            // ya que no se utilizan clases IEntityTypeConfiguration separadas

                        

            modelBuilder.Entity<Campo>(entity =>
            {
                entity.ToTable("Campos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Ubicacion).HasMaxLength(250);
                entity.Property(e => e.SuperficieHectareas).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Latitud).HasColumnType("decimal(18,8)");
                entity.Property(e => e.Longitud).HasColumnType("decimal(18,8)");

                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Campania>(entity =>
            {
                entity.ToTable("Campanias");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Lote>(entity =>
            {
                entity.ToTable("Lotes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.SuperficieHectareas).HasColumnType("decimal(10,2)");
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);


                entity.HasOne(l => l.Campo)
                    .WithMany(c => c.Lotes)
                    .HasForeignKey(l => l.IdCampo)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.Campania)
                    .WithMany(c => c.Lotes)
                    .HasForeignKey(l => l.IdCampania)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<RegistroClima>(entity =>
            {
                entity.ToTable("RegistrosClima");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Milimetros).HasColumnType("decimal(10,2)");
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);


                entity.HasOne(r => r.Campo)
                    .WithMany(l => l.RegistrosClima)
                    .HasForeignKey(r => r.IdCampo)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(l => l.Campania)
                    .WithMany(c => c.RegistrosClima)
                    .HasForeignKey(l => l.IdCampania)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Cultivo>(entity =>
            {
                entity.ToTable("Cultivos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.Color).HasMaxLength(20);
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<EstadoFenologico>(entity =>
            {
                entity.ToTable("EstadosFenologicos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Codigo).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Descripcion).HasMaxLength(500);

                entity.HasOne(ef => ef.Cultivo)
                    .WithMany(c => c.EstadosFenologicos)
                    .HasForeignKey(ef => ef.IdCultivo)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // LicenciasCultivos - Visibility side table
            modelBuilder.Entity<LicenciasCultivos>(entity =>
            {
                entity.ToTable("LicenciasCultivos");
                entity.HasKey(e => e.Id);

                // A license can only have one visibility entry per crop
                entity.HasIndex(e => new { e.IdLicencia, e.IdCultivo }).IsUnique();
                entity.HasIndex(e => e.IdLicencia);
                entity.HasIndex(e => e.IdCultivo);

                entity.HasOne(lc => lc.Licencia)
                    .WithMany()
                    .HasForeignKey(lc => lc.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(lc => lc.Cultivo)
                    .WithMany(c => c.LicenciasCultivos)
                    .HasForeignKey(lc => lc.IdCultivo)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Catalogo
            modelBuilder.Entity<Catalogo>(entity =>
            {
                entity.ToTable("Catalogos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);


                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // LicenciasCatalogos - Visibility side table
            modelBuilder.Entity<LicenciasCatalogos>(entity =>
            {
                entity.ToTable("LicenciasCatalogos");
                entity.HasKey(e => e.Id);

                // A license can only have one visibility entry per catalog item
                entity.HasIndex(e => new { e.IdLicencia, e.IdCatalogo }).IsUnique();
                entity.HasIndex(e => e.IdLicencia);
                entity.HasIndex(e => e.IdCatalogo);

                entity.HasOne(lc => lc.Licencia)
                    .WithMany()
                    .HasForeignKey(lc => lc.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(lc => lc.Catalogo)
                    .WithMany(c => c.LicenciasCatalogos)
                    .HasForeignKey(lc => lc.IdCatalogo)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // CicloCultivo
            modelBuilder.Entity<CicloCultivo>(entity =>
            {
                entity.ToTable("CicloCultivos");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);


                entity.Property(e => e.Epoca)
                    .IsRequired(false);

                entity.HasOne(cc => cc.Lote)
                    .WithMany(l => l.CicloCultivos)
                    .HasForeignKey(cc => cc.IdLote)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cc => cc.Cultivo)
                    .WithMany()
                    .HasForeignKey(cc => cc.IdCultivo)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(cc => cc.Campania)
                    .WithMany()
                    .HasForeignKey(cc => cc.IdCampania)
                    .OnDelete(DeleteBehavior.Restrict);

            });

            modelBuilder.Entity<Siembra>(entity =>
            {
                entity.ToTable("Siembras");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

                // Propiedades base de Actividad

                // Propiedades específicas
                entity.Property(e => e.Superficie).HasColumnType("decimal(10,2)");
                entity.Property(e => e.SuperficieHa).HasColumnType("decimal(18,4)");
                entity.Property(e => e.Densidad).HasColumnType("decimal(10,2)");
                entity.Property(e => e.DensidadSemillaKgHa).HasColumnType("decimal(18,4)");

                // FK Unidades
                entity.HasOne(e => e.UnidadSuperficie)
                    .WithMany()
                    .HasForeignKey(e => e.IdUnidadSuperficie)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.IdUnidadSuperficie);

                entity.HasOne(e => e.UnidadDensidad)
                    .WithMany()
                    .HasForeignKey(e => e.IdUnidadDensidad)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.IdUnidadDensidad);

                entity.Property(e => e.Costo).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoARS).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoUSD).HasColumnType("decimal(18,4)");

                // Relaciones base de Actividad
                entity.HasOne(a => a.Lote)
                    .WithMany(l => l.Siembras)
                    .HasForeignKey(a => a.IdLote)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.TipoActividad)
                    .WithMany()
                    .HasForeignKey(a => a.IdTipoActividad)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Usuario)
                    .WithMany()
                    .HasForeignKey(a => a.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relaciones específicas
                entity.HasOne(s => s.Cultivo)
                    .WithMany()
                    .HasForeignKey(s => s.IdCultivo)
                    .OnDelete(DeleteBehavior.Restrict);


                entity.HasOne(s => s.MetodoSiembra)
                    .WithMany()
                    .HasForeignKey(s => s.IdMetodoSiembra)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Campania)
                    .WithMany()
                    .HasForeignKey(a => a.IdCampania)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(h => h.Moneda)
                    .WithMany()
                    .HasForeignKey(h => h.IdMoneda)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.CicloCultivo)
                    .WithMany(cc => cc.Siembras)
                    .HasForeignKey(a => a.IdCicloCultivo)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Riego
            modelBuilder.Entity<Riego>(entity =>
            {
                entity.ToTable("Riegos");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);


                entity.Property(e => e.HorasRiego).HasColumnType("decimal(10,2)");
                entity.Property(e => e.VolumenAgua).HasColumnType("decimal(10,2)");
                entity.Property(e => e.VolumenAguaM3).HasColumnType("decimal(18,4)");

                // FK Unidad
                entity.HasOne(e => e.UnidadVolumenAgua)
                    .WithMany()
                    .HasForeignKey(e => e.IdUnidadVolumenAgua)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.IdUnidadVolumenAgua);

                entity.Property(e => e.Costo).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoARS).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoUSD).HasColumnType("decimal(18,4)");

                // Relaciones base
                entity.HasOne(a => a.Lote)
                    .WithMany(l => l.Riegos)
                    .HasForeignKey(a => a.IdLote)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.TipoActividad)
                    .WithMany()
                    .HasForeignKey(a => a.IdTipoActividad)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Usuario)
                    .WithMany()
                    .HasForeignKey(a => a.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.MetodoRiego)
                    .WithMany()
                    .HasForeignKey(r => r.IdMetodoRiego)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(r => r.FuenteAgua)
                    .WithMany()
                    .HasForeignKey(r => r.IdFuenteAgua)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Campania)
                    .WithMany()
                    .HasForeignKey(a => a.IdCampania)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(h => h.Moneda)
                    .WithMany()
                    .HasForeignKey(h => h.IdMoneda)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.CicloCultivo)
                    .WithMany(cc => cc.Riegos)
                    .HasForeignKey(a => a.IdCicloCultivo)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Fertilizacion
            modelBuilder.Entity<Fertilizacion>(entity =>
            {
                entity.ToTable("Fertilizaciones");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);


                entity.Property(e => e.Cantidad).HasColumnType("decimal(10,2)");
                entity.Property(e => e.CantidadKgHa).HasColumnType("decimal(18,4)");
                entity.Property(e => e.Dosis).HasColumnType("decimal(10,2)");
                entity.Property(e => e.DosisKgHa).HasColumnType("decimal(18,4)");

                // FK Unidades
                entity.HasOne(e => e.UnidadCantidad)
                    .WithMany()
                    .HasForeignKey(e => e.IdUnidadCantidad)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.IdUnidadCantidad);

                entity.HasOne(e => e.UnidadDosis)
                    .WithMany()
                    .HasForeignKey(e => e.IdUnidadDosis)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.IdUnidadDosis);

                entity.Property(e => e.Costo).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoARS).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoUSD).HasColumnType("decimal(18,4)");

                // Relaciones base
                entity.HasOne(a => a.Lote)
                    .WithMany(l => l.Fertilizaciones)
                    .HasForeignKey(a => a.IdLote)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.Nutriente)
                    .WithMany()
                    .HasForeignKey(f => f.IdNutriente)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.TipoFertilizante)
                    .WithMany()
                    .HasForeignKey(f => f.IdTipoFertilizante)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(f => f.MetodoAplicacion)
                    .WithMany()
                    .HasForeignKey(f => f.IdMetodoAplicacion)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.TipoActividad)
                    .WithMany()
                    .HasForeignKey(a => a.IdTipoActividad)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Usuario)
                    .WithMany()
                    .HasForeignKey(a => a.IdUsuario)

                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(a => a.Campania)
                    .WithMany()
                    .HasForeignKey(a => a.IdCampania)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(h => h.Moneda)
                    .WithMany()
                    .HasForeignKey(h => h.IdMoneda)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.CicloCultivo)
                    .WithMany(cc => cc.Fertilizaciones)
                    .HasForeignKey(a => a.IdCicloCultivo)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Pulverizacion
            modelBuilder.Entity<Pulverizacion>(entity =>
            {
                entity.ToTable("Pulverizaciones");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);


                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);


                entity.Property(e => e.Volumen).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Dosis).HasColumnType("decimal(10,2)");

                // FK Unidades
                entity.HasOne(e => e.UnidadVolumen)
                    .WithMany()
                    .HasForeignKey(e => e.IdUnidadVolumen)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.IdUnidadVolumen);

                entity.HasOne(e => e.UnidadDosis)
                    .WithMany()
                    .HasForeignKey(e => e.IdUnidadDosis)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.IdUnidadDosis);
                entity.Property(e => e.CondicionesClimaticas).HasMaxLength(200);

                entity.Property(e => e.Costo).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoARS).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoUSD).HasColumnType("decimal(18,4)");

                entity.HasOne(p => p.ProductoAgroquimico)
                    .WithMany()
                    .HasForeignKey(p => p.IdProductoAgroquimico)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Campania)
                    .WithMany()
                    .HasForeignKey(a => a.IdCampania)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Lote)
                    .WithMany(l => l.Pulverizaciones)
                    .HasForeignKey(a => a.IdLote)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.TipoActividad)
                    .WithMany()
                    .HasForeignKey(a => a.IdTipoActividad)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Usuario)
                    .WithMany()
                    .HasForeignKey(a => a.IdUsuario);

                entity.HasOne(h => h.Moneda)
                    .WithMany()
                    .HasForeignKey(h => h.IdMoneda)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.CicloCultivo)
                    .WithMany(cc => cc.Pulverizaciones)
                    .HasForeignKey(a => a.IdCicloCultivo)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Monitoreo
            modelBuilder.Entity<Monitoreo>(entity =>
            {
                entity.ToTable("Monitoreos");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);


                entity.Property(e => e.Costo).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoARS).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoUSD).HasColumnType("decimal(18,4)");

                entity.HasOne(m => m.EstadoFenologico)
                    .WithMany()
                    .HasForeignKey(m => m.IdEstadoFenologico)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(m => m.TipoMonitoreo)
                    .WithMany()
                    .HasForeignKey(m => m.IdTipoMonitoreo)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Campania)
                    .WithMany()
                    .HasForeignKey(a => a.IdCampania)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Lote)
                    .WithMany(l => l.Monitoreos)
                    .HasForeignKey(a => a.IdLote)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.TipoActividad)
                    .WithMany()
                    .HasForeignKey(a => a.IdTipoActividad)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Usuario)
                    .WithMany()
                    .HasForeignKey(a => a.IdUsuario);

                entity.HasOne(h => h.Moneda)
                    .WithMany()
                    .HasForeignKey(h => h.IdMoneda)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.CicloCultivo)
                    .WithMany(cc => cc.Monitoreos)
                    .HasForeignKey(a => a.IdCicloCultivo)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<AnalisisSuelo>(entity =>
            {
                entity.ToTable("AnalisisSuelos");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);


                entity.Property(e => e.ProfundidadCm).HasColumnType("decimal(10,2)");
                entity.Property(e => e.PH).HasColumnType("decimal(5,2)");
                entity.Property(e => e.MateriaOrganica).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Nitrogeno).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Fosforo).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Potasio).HasColumnType("decimal(10,2)");
                entity.Property(e => e.ConductividadElectrica).HasColumnType("decimal(10,2)");
                entity.Property(e => e.CIC).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Textura).HasMaxLength(50);

                entity.Property(e => e.Costo).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoARS).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoUSD).HasColumnType("decimal(18,4)");

                entity.HasOne(a => a.Lote)
                    .WithMany(l => l.AnalisisSuelos)
                    .HasForeignKey(a => a.IdLote)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.TipoActividad)
                    .WithMany()
                    .HasForeignKey(a => a.IdTipoActividad)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Usuario)
                    .WithMany()
                    .HasForeignKey(a => a.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Campania)
                    .WithMany()
                    .HasForeignKey(a => a.IdCampania)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Laboratorio)
                    .WithMany()
                    .HasForeignKey(a => a.IdLaboratorio)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(h => h.Moneda)
                    .WithMany()
                    .HasForeignKey(h => h.IdMoneda)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.CicloCultivo)
                    .WithMany(cc => cc.AnalisisSuelos)
                    .HasForeignKey(a => a.IdCicloCultivo)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // Cosecha
            modelBuilder.Entity<Cosecha>(entity =>
            {
                entity.ToTable("Cosechas");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);


                entity.Property(e => e.Rendimiento).HasColumnType("decimal(10,2)");
                entity.Property(e => e.RendimientoTonHa).HasColumnType("decimal(18,4)");
                entity.Property(e => e.HumedadGrano).HasColumnType("decimal(5,2)");
                entity.Property(e => e.SuperficieCosechada).HasColumnType("decimal(10,2)");
                entity.Property(e => e.SuperficieCosechadaHa).HasColumnType("decimal(18,4)");

                // FK Unidades
                entity.HasOne(e => e.UnidadRendimiento)
                    .WithMany()
                    .HasForeignKey(e => e.IdUnidadRendimiento)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.IdUnidadRendimiento);

                entity.HasOne(e => e.UnidadSuperficieCosechada)
                    .WithMany()
                    .HasForeignKey(e => e.IdUnidadSuperficieCosechada)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.IdUnidadSuperficieCosechada);

                entity.Property(e => e.Costo).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoARS).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoUSD).HasColumnType("decimal(18,4)");

                entity.HasOne(c => c.Cultivo)
                    .WithMany()
                    .HasForeignKey(c => c.IdCultivo)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Campania)
                    .WithMany()
                    .HasForeignKey(a => a.IdCampania)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Lote)
                    .WithMany(l => l.Cosechas)
                    .HasForeignKey(a => a.IdLote)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.TipoActividad)
                    .WithMany()
                    .HasForeignKey(a => a.IdTipoActividad)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Usuario)
                    .WithMany()
                    .HasForeignKey(a => a.IdUsuario);

                entity.HasOne(h => h.Moneda)
                    .WithMany()
                    .HasForeignKey(h => h.IdMoneda)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.CicloCultivo)
                    .WithMany(cc => cc.Cosechas)
                    .HasForeignKey(a => a.IdCicloCultivo)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<OtraLabor>(entity =>
            {
                entity.ToTable("OtrasLabores");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);


                entity.Property(e => e.Costo).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoARS).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoUSD).HasColumnType("decimal(18,4)");

                entity.HasOne(a => a.Lote)
                    .WithMany(l => l.OtrasLabores)
                    .HasForeignKey(a => a.IdLote)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.TipoActividad)
                    .WithMany()
                    .HasForeignKey(a => a.IdTipoActividad)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Usuario)
                    .WithMany()
                    .HasForeignKey(a => a.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Campania)
                    .WithMany()
                    .HasForeignKey(a => a.IdCampania)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(h => h.Moneda)
                    .WithMany()
                    .HasForeignKey(h => h.IdMoneda)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.CicloCultivo)
                    .WithMany(cc => cc.OtrasLabores)
                    .HasForeignKey(a => a.IdCicloCultivo)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Acopio>(entity =>
            {
                entity.ToTable("Acopios");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

                entity.Property(e => e.TipoAcopio).IsRequired();
                entity.Property(e => e.Codigo).HasMaxLength(50);
                entity.Property(e => e.CantidadActualTn).HasColumnType("decimal(10,2)");
                entity.Property(e => e.CapacidadTotalTn).HasColumnType("decimal(10,2)");
                entity.Property(e => e.HumedadGrano).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Estado).HasMaxLength(50);
                entity.Property(e => e.Ubicacion).HasMaxLength(200);
                entity.Property(e => e.Longitud).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Diametro).HasColumnType("decimal(10,2)");
                entity.Property(e => e.TipoSilo).HasMaxLength(50);
                entity.Property(e => e.TemperaturaGrano).HasColumnType("decimal(5,2)");
                entity.Property(e => e.Empresa).HasMaxLength(150);
                entity.Property(e => e.NumeroContrato).HasMaxLength(50);
                entity.Property(e => e.TarifaAlmacenaje).HasColumnType("decimal(18,4)");
                entity.Property(e => e.Costo).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoARS).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoUSD).HasColumnType("decimal(18,4)");

                entity.HasOne(a => a.Cultivo)
                    .WithMany()
                    .HasForeignKey(a => a.IdCultivo)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Lote)
                    .WithMany(l => l.Acopios)
                    .HasForeignKey(a => a.IdLote)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.TipoActividad)
                    .WithMany()
                    .HasForeignKey(a => a.IdTipoActividad)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Usuario)
                    .WithMany()
                    .HasForeignKey(a => a.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Campania)
                    .WithMany()
                    .HasForeignKey(a => a.IdCampania)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(h => h.Moneda)
                    .WithMany()
                    .HasForeignKey(h => h.IdMoneda)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.CicloCultivo)
                    .WithMany(cc => cc.Acopios)
                    .HasForeignKey(a => a.IdCicloCultivo)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(300);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.PasswordSalt).IsRequired();

                entity.HasIndex(e => new { e.Email, e.IdLicencia }).IsUnique();
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TipoActividad>(entity =>
            {
                entity.ToTable("TiposActividad");
                entity.HasKey(e => e.Id);

            });

            modelBuilder.Entity<Licencia>(entity =>
            {
                entity.ToTable("Licencias");
                entity.HasKey(e => e.Id);

            });

            modelBuilder.Entity<Moneda>(entity =>
            {
                entity.ToTable("Monedas");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Codigo).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.TipoCambioReferencia).HasColumnType("decimal(18,6)");
            });

            modelBuilder.Entity<PagoLicencia>(entity =>
            {
                entity.ToTable("PagoLicencias");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Precio).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Fecha).HasColumnType("datetime2");
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne(p => p.Licencia)
                    .WithMany(l => l.PagoLicencias)
                    .HasForeignKey(p => p.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ReporteCierreCampania>(entity =>
            {
                entity.ToTable("ReporteCierreCampania");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.AnalisisSueloArs).HasColumnType("decimal(18,4)");
                entity.Property(e => e.AnalisisSueloUsd).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoCosechasArs).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoCosechasUsd).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoFertilizantesArs).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoFertilizantesUsd).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoMonitoreosArs).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoMonitoreosUsd).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoOtrasLaboresArs).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoOtrasLaboresUsd).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoAcopiosArs).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoAcopiosUsd).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoPorHaArs).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoPorToneladaArs).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoPulverizacionesArs).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoPulverizacionesUsd).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoRiegosArs).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoRiegosUsd).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoSiembrasArs).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoSiembrasUsd).HasColumnType("decimal(18,4)");
                entity.Property(e => e.LluviaAcumuladaTotal).HasColumnType("decimal(18,2)");
                entity.Property(e => e.RendimientoPromedioHa).HasColumnType("decimal(18,2)");
                entity.Property(e => e.SuperficieTotalHa).HasColumnType("decimal(18,2)");
                entity.Property(e => e.ToneladasProducidas).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CostoPorHaUsd).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoPorToneladaUsd).HasColumnType("decimal(18,4)");
                entity.Property(e => e.GastosTotalesArs).HasColumnType("decimal(18,4)");
                entity.Property(e => e.GastosTotalesUsd).HasColumnType("decimal(18,4)");

                entity.HasOne(rc => rc.Campania)
                    .WithOne(c => c.ReporteCierreCampania)
                    .HasForeignKey<ReporteCierreCampania>(rc => rc.IdCampania)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(rc => rc.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===============================
            // Configuración de Unidades de Medida
            // ===============================
            modelBuilder.Entity<UnidadMedida>(entity =>
            {
                entity.ToTable("UnidadesMedida");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Sigla).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Categoria)
                    .HasConversion<int>()
                    .HasColumnType("int");
                entity.Property(e => e.DimensionBase)
                    .HasConversion<int>()
                    .HasColumnType("int");
                entity.Property(e => e.FactorConversion)
                    .HasColumnType("decimal(18,6)")
                    .IsRequired();
                entity.Property(e => e.Orden).HasColumnType("int");
                entity.Property(e => e.Activo).HasDefaultValue(true);

                entity.HasIndex(e => e.Categoria);
                entity.HasIndex(e => e.DimensionBase);
                entity.HasIndex(e => e.Sigla).IsUnique().HasFilter("[Activo] = 1");
            });

            modelBuilder.Entity<CampoLaborUnidad>(entity =>
            {
                entity.ToTable("CamposLaborUnidad");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.NombreCampo).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NombrePropiedad).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Etiqueta).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Requerido).HasDefaultValue(false);

                entity.HasOne(e => e.TipoActividad)
                    .WithMany()
                    .HasForeignKey(e => e.IdTipoActividad)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.IdTipoActividad);
            });

            modelBuilder.Entity<CampoLaborUnidadPermitida>(entity =>
            {
                entity.ToTable("CamposLaborUnidadPermitida");
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.CampoLaborUnidad)
                    .WithMany(c => c.UnidadesPermitidas)
                    .HasForeignKey(e => e.IdCampoLaborUnidad)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.UnidadMedida)
                    .WithMany()
                    .HasForeignKey(e => e.IdUnidadMedida)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.IdCampoLaborUnidad);
                entity.HasIndex(e => e.IdUnidadMedida);
            });

            modelBuilder.Entity<Gasto>(entity =>
            {
                entity.ToTable("Gastos");

                // Llave primaria
                entity.HasKey(e => e.Id);

               
                entity.Property(e => e.Costo)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.CostoARS)
                    .HasColumnType("decimal(18,2)");

                entity.Property(e => e.CostoUSD)
                    .HasColumnType("decimal(18,2)");

                // Relaciones
                entity.HasOne(e => e.Moneda)
                    .WithMany()
                    .HasForeignKey(e => e.IdMoneda)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Campania)
                    .WithMany(_=>_.Gastos)
                    .HasForeignKey(e => e.IdCampania)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne<Licencia>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLicencia)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===============================
            // Configuración de Índices Satelitales
            // ===============================
            modelBuilder.Entity<IndiceSatelital>(entity =>
            {
                entity.ToTable("IndicesSatelitales");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FechaCaptura).HasColumnType("date");
                entity.Property(e => e.Fuente).HasMaxLength(50).HasDefaultValue("Sentinel-2");
                entity.Property(e => e.ResolucionMts).HasDefaultValue(10);
                entity.Property(e => e.CloudCover).HasColumnType("decimal(5,2)");
                entity.Property(e => e.NDVI).HasColumnType("decimal(5,3)");
                entity.Property(e => e.NDWI).HasColumnType("decimal(5,3)");
                entity.Property(e => e.EVI).HasColumnType("decimal(5,3)");
                entity.Property(e => e.NDRE).HasColumnType("decimal(5,3)");
                entity.Property(e => e.SAVI).HasColumnType("decimal(5,3)");
                entity.Property(e => e.GNDVI).HasColumnType("decimal(5,3)");
                entity.Property(e => e.EsValido).HasDefaultValue(true);

                entity.HasIndex(e => new { e.IdLote, e.FechaCaptura });
                entity.HasIndex(e => e.IdLicencia);
                entity.HasIndex(e => e.FechaCaptura);

                entity.HasOne<Lote>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLote)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LoteGeometria>(entity =>
            {
                entity.ToTable("LotesGeometria");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.GeometriaOriginal).HasColumnType("nvarchar(max)");
                entity.Property(e => e.GeometriaSimplificada).HasColumnType("nvarchar(max)");
                entity.Property(e => e.ToleranciaSimplificacion).HasColumnType("decimal(10,6)");
                entity.Property(e => e.AreaHa).HasColumnType("decimal(10,4)");
                entity.Property(e => e.CentroLat).HasColumnType("decimal(10,7)");
                entity.Property(e => e.CentroLng).HasColumnType("decimal(10,7)");
                entity.Property(e => e.BoundsJson).HasMaxLength(500);

                entity.HasIndex(e => e.IdLote);

                entity.HasOne<Lote>()
                    .WithMany()
                    .HasForeignKey(e => e.IdLote)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<LogConsultaSatelital>(entity =>
            {
                entity.ToTable("LogsConsultasSatelitales");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TipoConsulta).HasMaxLength(50).IsRequired();
                entity.Property(e => e.IndiceSolicitado).HasMaxLength(50).IsRequired();
                entity.Property(e => e.ErrorMessage).HasMaxLength(500);
                entity.Property(e => e.CostoEstimado).HasColumnType("decimal(10,8)");

                entity.HasIndex(e => e.FechaConsulta);
                entity.HasIndex(e => e.IdLote);
                entity.HasIndex(e => e.TipoConsulta);
            });

            // LaborDTO - used by SqlQueryRaw<LaborDTO> in ActividadService
            modelBuilder.Entity<LaborDTO>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.Costo).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoARS).HasColumnType("decimal(18,4)");
                entity.Property(e => e.CostoUSD).HasColumnType("decimal(18,4)");
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<EntityBase>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        // Solo establecer valores por defecto si no fueron explícitamente asignados
                        if (entry.Entity.RegistrationDate == null)
                            entry.Entity.RegistrationDate = TimeHelper.GetArgentinaTime();
                        if (string.IsNullOrEmpty(entry.Entity.RegistrationUser))
                            entry.Entity.RegistrationUser = _userContext.UserName;
                        break;
                    case EntityState.Modified:
                        entry.Entity.ModificationDate = TimeHelper.GetArgentinaTime();
                        entry.Entity.ModificationUser = _userContext.UserName;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
