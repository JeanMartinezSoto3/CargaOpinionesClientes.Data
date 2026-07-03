using CargaOpinionesClientes.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace CargaOpinionesClientes.Data.Context
{
    public class OpinionesDbContext : DbContext
    {
        private readonly string _connectionString;

        public OpinionesDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        public DbSet<Cliente> Clientes { get; set; } = null!;
        public DbSet<Producto> Productos { get; set; } = null!;
        public DbSet<FuenteDato> FuentesDatos { get; set; } = null!;
        public DbSet<Opinion> Opiniones { get; set; } = null!;
        public DbSet<ErrorCarga> ErroresCarga { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(_connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Clientes");
                entity.HasKey(e => e.IdCliente);

                entity.Property(e => e.IdCliente)
                      .ValueGeneratedNever();

                entity.Property(e => e.Nombre)
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(e => e.Email)
                      .HasMaxLength(150)
                      .IsRequired();

                entity.HasIndex(e => e.Email)
                      .IsUnique();
            });

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("Productos");
                entity.HasKey(e => e.IdProducto);

                entity.Property(e => e.IdProducto)
                      .ValueGeneratedNever();

                entity.Property(e => e.Nombre)
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(e => e.Categoria)
                      .HasMaxLength(100)
                      .IsRequired();
            });

            modelBuilder.Entity<FuenteDato>(entity =>
            {
                entity.ToTable("FuentesDatos");
                entity.HasKey(e => e.IdFuente);

                entity.Property(e => e.IdFuente)
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.CodigoFuente)
                      .HasMaxLength(20);

                entity.Property(e => e.NombreFuente)
                      .HasMaxLength(100)
                      .IsRequired();

                entity.Property(e => e.FechaCarga);

                entity.HasIndex(e => e.NombreFuente)
                      .IsUnique();
            });

            modelBuilder.Entity<Opinion>(entity =>
            {
                entity.ToTable("Opiniones");
                entity.HasKey(e => e.IdOpinion);

                entity.Property(e => e.IdOpinion)
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.IdExterno)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.TipoOpinion)
                      .HasMaxLength(30)
                      .IsRequired();

                entity.Property(e => e.Comentario)
                      .HasMaxLength(1000)
                      .IsRequired();

                entity.Property(e => e.Clasificacion)
                      .HasMaxLength(50);

                entity.Property(e => e.FechaRegistro)
                      .HasDefaultValueSql("SYSDATETIME()");

                entity.HasIndex(e => new { e.TipoOpinion, e.IdExterno })
                      .IsUnique();

                entity.HasOne(e => e.Cliente)
                      .WithMany(c => c.Opiniones)
                      .HasForeignKey(e => e.IdCliente)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Producto)
                      .WithMany(p => p.Opiniones)
                      .HasForeignKey(e => e.IdProducto)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.FuenteDato)
                      .WithMany(f => f.Opiniones)
                      .HasForeignKey(e => e.IdFuente)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ErrorCarga>(entity =>
            {
                entity.ToTable("ErroresCarga");
                entity.HasKey(e => e.IdError);

                entity.Property(e => e.IdError)
                      .ValueGeneratedOnAdd();

                entity.Property(e => e.Archivo)
                      .HasMaxLength(150)
                      .IsRequired();

                entity.Property(e => e.Motivo)
                      .HasMaxLength(500)
                      .IsRequired();

                entity.Property(e => e.DatosRegistro);

                entity.Property(e => e.FechaError)
                      .HasDefaultValueSql("SYSDATETIME()");
            });
        }
    }
}