using CargaOpinionesClientes.Worker.Models.Warehouse;
using Microsoft.EntityFrameworkCore;

namespace CargaOpinionesClientes.Worker.Persistence;

public class DataWarehouseDbContext : DbContext
{
    public DataWarehouseDbContext(
        DbContextOptions<DataWarehouseDbContext> options)
        : base(options)
    {
    }

    public DbSet<DimCliente> DimClientes => Set<DimCliente>();

    public DbSet<DimProducto> DimProductos => Set<DimProducto>();

    public DbSet<DimFuente> DimFuentes => Set<DimFuente>();

    public DbSet<DimFecha> DimFechas => Set<DimFecha>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<DimCliente>()
            .Property(x => x.IdCliente)
            .ValueGeneratedNever();

        modelBuilder.Entity<DimProducto>()
            .Property(x => x.IdProducto)
            .ValueGeneratedNever();

        modelBuilder.Entity<DimFecha>()
            .Property(x => x.IdFecha)
            .ValueGeneratedNever();

        modelBuilder.Entity<DimFuente>()
            .Property(x => x.IdFuente)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<DimFuente>()
            .HasIndex(x => x.CodigoFuente);
    }
}