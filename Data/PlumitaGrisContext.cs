using Microsoft.EntityFrameworkCore;
using PlumitaGrisAPI.Models;

namespace PlumitaGrisAPI.Data
{
    public class PlumitaGrisContext : DbContext
    {
        public PlumitaGrisContext(DbContextOptions<PlumitaGrisContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Inventario> Inventarios { get; set; }
        public DbSet<Carrito> Carritos { get; set; }
        public DbSet<DetalleCarrito> DetallesCarrito { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }

        // Catálogos
        public DbSet<Rol> Roles { get; set; }
        public DbSet<EstadoPedido> EstadosPedido { get; set; }
        public DbSet<ModalidadEntrega> ModalidadesEntrega { get; set; }
        public DbSet<EstadoPago> EstadosPago { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>()
                .HasOne(c => c.Usuario)
                .WithOne()
                .HasForeignKey<Cliente>(c => c.IdUsuario);

            modelBuilder.Entity<Administrador>()
                .HasOne(a => a.Usuario)
                .WithOne()
                .HasForeignKey<Administrador>(a => a.IdUsuario);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany()
                .HasForeignKey(p => p.IdCategoria);

            modelBuilder.Entity<Inventario>()
                .HasOne(i => i.Producto)
                .WithOne()
                .HasForeignKey<Inventario>(i => i.IdProducto);

            modelBuilder.Entity<Carrito>()
                .HasOne(c => c.Cliente)
                .WithOne()
                .HasForeignKey<Carrito>(c => c.IdCliente);

            modelBuilder.Entity<DetalleCarrito>()
                .HasOne(dc => dc.Carrito)
                .WithMany()
                .HasForeignKey(dc => dc.IdCarrito);

            modelBuilder.Entity<DetalleCarrito>()
                .HasOne(dc => dc.Producto)
                .WithMany()
                .HasForeignKey(dc => dc.IdProducto);

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany()
                .HasForeignKey(p => p.IdCliente);

            modelBuilder.Entity<DetallePedido>()
                .HasOne(dp => dp.Pedido)
                .WithMany(p => p.Detalles)
                .HasForeignKey(dp => dp.IdPedido);

            modelBuilder.Entity<DetallePedido>()
                .HasOne(dp => dp.Producto)
                .WithMany()
                .HasForeignKey(dp => dp.IdProducto);

            modelBuilder.Entity<Pago>()
                .HasOne(pa => pa.Pedido)
                .WithOne()
                .HasForeignKey<Pago>(pa => pa.IdPedido);

            // ---- Relaciones con catálogos ----
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany()
                .HasForeignKey(u => u.IdRol);

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.EstadoPedido)
                .WithMany()
                .HasForeignKey(p => p.IdEstadoPedido);

            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.ModalidadEntrega)
                .WithMany()
                .HasForeignKey(p => p.IdModalidadEntrega);

            modelBuilder.Entity<Pago>()
                .HasOne(pa => pa.EstadoPago)
                .WithMany()
                .HasForeignKey(pa => pa.IdEstadoPago);

            modelBuilder.Entity<Producto>().ToTable(tb => tb.UseSqlOutputClause(false));
            modelBuilder.Entity<Pedido>().ToTable(tb => tb.UseSqlOutputClause(false));
            modelBuilder.Entity<Usuario>().ToTable(tb => tb.UseSqlOutputClause(false));

            base.OnModelCreating(modelBuilder);
        }
    }
}