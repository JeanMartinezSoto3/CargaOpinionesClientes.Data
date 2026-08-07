using CargaOpinionesClientes.Worker.Models.Transactional;
using Microsoft.EntityFrameworkCore;

namespace CargaOpinionesClientes.Worker.Persistence;

public class TransactionalDbContext : DbContext
{
    public TransactionalDbContext(
        DbContextOptions<TransactionalDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Producto> Productos => Set<Producto>();

    public DbSet<FuenteDato> FuentesDatos => Set<FuenteDato>();

    public DbSet<Opinion> Opiniones => Set<Opinion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>()
            .Property(x => x.IdCliente)
            .ValueGeneratedNever();

        modelBuilder.Entity<Producto>()
            .Property(x => x.IdProducto)
            .ValueGeneratedNever();

        modelBuilder.Entity<FuenteDato>()
            .Property(x => x.IdFuente)
            .ValueGeneratedNever();

        modelBuilder.Entity<Opinion>()
            .Property(x => x.IdOpinion)
            .ValueGeneratedNever();
    }
}