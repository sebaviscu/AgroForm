using AgroForm.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AgroForm.Data.DBContext
{
    public class AppDbContext : DbContext
    {
        public readonly ILogger<AppDbContext> _logger;

        public AppDbContext(DbContextOptions<AppDbContext> options, ILogger<AppDbContext> logger)
            : base(options)
        {
            _logger = logger;
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Campo> Campos { get; set; }
        public DbSet<Lote> Lotes { get; set; }
        public DbSet<Campania> Campanias { get; set; }
        public DbSet<RegistroClima> RegistrosClima { get; set; }
        public DbSet<TipoActividad> TiposActividad { get; set; }
        public DbSet<Actividad> Actividades { get; set; }
        public DbSet<Insumo> Insumos { get; set; }
        public DbSet<HistoricoPrecioInsumo> HistoricosPrecioInsumo { get; set; }

        public DbSet<Moneda> Monedas { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===============================
            // Configuraciones generales
            // ===============================
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

            // Campo
            modelBuilder.Entity<Campo>(entity =>
            {
                entity.ToTable("Campos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Ubicacion).HasMaxLength(250);
                entity.Property(e => e.SuperficieHectareas).HasColumnType("decimal(10,2)");

                // Índice para multi-tenant
                entity.HasIndex(e => e.IdLicencia);
            });

            // Campania
            modelBuilder.Entity<Campania>(entity =>
            {
                entity.ToTable("Campanias");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.HasIndex(e => e.IdLicencia);
            });

            // Lote
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

            // RegistroClima
            modelBuilder.Entity<RegistroClima>(entity =>
            {
                entity.ToTable("RegistrosClima");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Milimetros).HasColumnType("decimal(10,2)");
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne(r => r.Lote)
                    .WithMany(l => l.RegistrosClima)
                    .HasForeignKey(r => r.IdLote)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Actividad
            modelBuilder.Entity<Actividad>(entity =>
            {
                entity.ToTable("Actividades");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne(a => a.Lote)
                    .WithMany(l => l.Actividades)
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

                entity.HasOne(a => a.Insumo)
                    .WithMany()
                    .HasForeignKey(a => a.IdInsumo)
                    .IsRequired(false)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // HistoricoPrecioInsumo
            modelBuilder.Entity<HistoricoPrecioInsumo>(entity =>
            {
                entity.ToTable("HistoricosPrecioInsumo");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Precio).HasColumnType("decimal(18,2)");
                entity.HasIndex(e => e.IdLicencia);

                entity.HasOne(h => h.Insumo)
                    .WithMany(i => i.HistoricoPrecios)
                    .HasForeignKey(h => h.IdInsumo)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(h => h.Moneda)
                    .WithMany(m => m.HistoricoPrecios)
                    .HasForeignKey(h => h.IdMoneda)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(300);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.PasswordSalt).IsRequired();

                // Índice único por email y licencia
                entity.HasIndex(e => new { e.Email, e.IdLicencia }).IsUnique();
                entity.HasIndex(e => e.IdLicencia);
            });

            // Insumo
            modelBuilder.Entity<Insumo>(entity =>
            {
                entity.ToTable("Insumos");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Nombre).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Descripcion).HasMaxLength(500);
                entity.Property(e => e.UnidadMedida).HasMaxLength(50);

                entity.HasOne(i => i.Marca)
                    .WithMany(m => m.Insumos)
                    .HasForeignKey(i => i.IdMarca)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Proveedor)
                    .WithMany(p => p.Insumos)
                    .HasForeignKey(i => i.IdProveedor)
                    .OnDelete(DeleteBehavior.Restrict);


                entity.HasOne(i => i.TipoInsumo)
                    .WithMany(t => t.Insumos)
                    .HasForeignKey(i => i.IdTipoInsumo)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(i => i.Moneda)
                   .WithMany()
                   .HasForeignKey(i => i.IdMoneda)
                   .OnDelete(DeleteBehavior.Restrict);

            });

            modelBuilder.Entity<TipoInsumo>(entity =>
            {
                entity.ToTable("TiposInsumo");
                entity.HasKey(e => e.Id);

            });

            modelBuilder.Entity<TipoActividad>(entity =>
            {
                entity.ToTable("TiposActividad");
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.TipoInsumo)
                  .WithMany(e => e.TiposActividad)
                  .HasForeignKey(e => e.IdTipoInsumo)
                  .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}