using AgroForm.Model;
using AgroForm.Model.Actividades;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AgroForm.Data.DBContext
{
    public partial class AppDbContext : DbContext
    {
        public readonly ILogger<AppDbContext> _logger;

        public AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext> logger)
            : base(options)
        {
            _logger = logger;
        }

        public DbSet<Licencia> Licencias { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Campo> Campos { get; set; }
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<Campania> Campanias { get; set; }
        public DbSet<RegistroClima> RegistrosClima { get; set; }
        public DbSet<TipoActividad> TiposActividad { get; set; }
        public DbSet<Moneda> Monedas { get; set; }

        public DbSet<Cultivo> Cultivos { get; set; }
        public DbSet<Variedad> Variedades { get; set; }
        public DbSet<EstadoFenologico> EstadosFenologicos { get; set; }
        public DbSet<Catalogo> Catalogos { get; set; }

        public DbSet<Siembra> Siembras { get; set; }
        public DbSet<Riego> Riegos { get; set; }
        public DbSet<Fertilizacion> Fertilizaciones { get; set; }
        public DbSet<Pulverizacion> Pulverizaciones { get; set; }
        public DbSet<Monitoreo> Monitoreos { get; set; }
        public DbSet<AnalisisSuelo> AnalisisSuelos { get; set; }
        public DbSet<Cosecha> Cosechas { get; set; }
        public DbSet<OtraLabor> OtrasLabores { get; set; }
        public DbSet<ReporteCierreCampania> ReportesCierreCampania { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===============================
            // Configuraciones generales
            // ===============================
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);


            modelBuilder.Entity<Campo>(entity =>
            {
                entity.ToTable("Campos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Ubicacion).HasMaxLength(250);
                entity.Property(e => e.SuperficieHectareas).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Latitud).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Longitud).HasColumnType("decimal(18,2)");

                entity.HasIndex(e => e.IdLicencia);
            });

            modelBuilder.Entity<Campania>(entity =>
            {
                entity.ToTable("Campanias");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.HasIndex(e => e.IdLicencia);
            });

            modelBuilder.Entity<Lote>(entity =>
            {
                entity.ToTable("Lotes");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.SuperficieHectareas).HasColumnType("decimal(10,2)");
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne(l => l.Campo)
                    .WithMany(c => c.Lotes)
                    .HasForeignKey(l => l.IdCampo)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(l => l.Campania)
                    .WithMany(c => c.Lotes)
                    .HasForeignKey(l => l.IdCampania)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<RegistroClima>(entity =>
            {
                entity.ToTable("RegistrosClima");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Milimetros).HasColumnType("decimal(10,2)");
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne(r => r.Campo)
                    .WithMany(l => l.RegistrosClima)
                    .HasForeignKey(r => r.IdCampo)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Cultivo>(entity =>
            {
                entity.ToTable("Cultivos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
            });

            modelBuilder.Entity<Variedad>(entity =>
            {
                entity.ToTable("Variedades");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Descripcion).HasMaxLength(500);

                entity.HasOne(v => v.Cultivo)
                    .WithMany(c => c.Variedades)
                    .HasForeignKey(v => v.IdCultivo)
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

            // Catalogo
            modelBuilder.Entity<Catalogo>(entity =>
            {
                entity.ToTable("Catalogos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
            });

            modelBuilder.Entity<Siembra>(entity =>
            {
                entity.ToTable("Siembras");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

                // Propiedades base de Actividad

                // Propiedades específicas
                entity.Property(e => e.SuperficieHa).HasColumnType("decimal(10,2)");
                entity.Property(e => e.DensidadSemillaKgHa).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Costo).HasColumnType("decimal(18,2)");

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

                entity.HasOne(s => s.Variedad)
                    .WithMany()
                    .HasForeignKey(s => s.IdVariedad)
                    .IsRequired(false)
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
            });

            // Riego
            modelBuilder.Entity<Riego>(entity =>
            {
                entity.ToTable("Riegos");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

                entity.Property(e => e.HorasRiego).HasColumnType("decimal(10,2)");
                entity.Property(e => e.VolumenAguaM3).HasColumnType("decimal(10,2)");

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
            });

            // Fertilizacion
            modelBuilder.Entity<Fertilizacion>(entity =>
            {
                entity.ToTable("Fertilizaciones");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

                entity.Property(e => e.CantidadKgHa).HasColumnType("decimal(10,2)");
                entity.Property(e => e.DosisKgHa).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Costo).HasColumnType("decimal(18,2)");

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
            });

            // Pulverizacion
            modelBuilder.Entity<Pulverizacion>(entity =>
            {
                entity.ToTable("Pulverizaciones");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

                entity.Property(e => e.VolumenLitrosHa).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Dosis).HasColumnType("decimal(10,2)");
                entity.Property(e => e.CondicionesClimaticas).HasMaxLength(200);

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
            });

            // Monitoreo
            modelBuilder.Entity<Monitoreo>(entity =>
            {
                entity.ToTable("Monitoreos");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

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
            });

            modelBuilder.Entity<AnalisisSuelo>(entity =>
            {
                entity.ToTable("AnalisisSuelos");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

                entity.Property(e => e.ProfundidadCm).HasColumnType("decimal(10,2)");
                entity.Property(e => e.PH).HasColumnType("decimal(5,2)");
                entity.Property(e => e.MateriaOrganica).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Nitrogeno).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Fosforo).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Potasio).HasColumnType("decimal(10,2)");
                entity.Property(e => e.ConductividadElectrica).HasColumnType("decimal(10,2)");
                entity.Property(e => e.CIC).HasColumnType("decimal(10,2)");
                entity.Property(e => e.Textura).HasMaxLength(50);

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
            });


            // Cosecha
            modelBuilder.Entity<Cosecha>(entity =>
            {
                entity.ToTable("Cosechas");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

                entity.Property(e => e.RendimientoTonHa).HasColumnType("decimal(10,2)");
                entity.Property(e => e.HumedadGrano).HasColumnType("decimal(5,2)");
                entity.Property(e => e.SuperficieCosechadaHa).HasColumnType("decimal(10,2)");

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
            });

            modelBuilder.Entity<OtraLabor>(entity =>
            {
                entity.ToTable("OtrasLabores");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

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
        }
    }
}