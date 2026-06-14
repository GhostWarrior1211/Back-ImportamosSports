using ImportamosSports.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace ImportamosSports.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<EstadoPedido> EstadosPedido => Set<EstadoPedido>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<DetallePedido> DetallesPedido => Set<DetallePedido>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<Cupon> Cupones => Set<Cupon>();
    public DbSet<AsesorVenta> AsesoresVenta => Set<AsesorVenta>();
    public DbSet<ProductoTalla> ProductoTallas => Set<ProductoTalla>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.ToTable("Rol");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuario");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombres).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Apellidos).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Correo).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Clave).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Telefono).HasMaxLength(20);

            entity.HasOne(e => e.Rol)
                  .WithMany(r => r.Usuarios)
                  .HasForeignKey(e => e.RolId);
        });

        modelBuilder.Entity<Marca>(entity =>
        {
            entity.ToTable("Marca");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("Producto");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(150).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Precio).HasColumnType("decimal(10,2)");
            entity.Property(e => e.ImagenUrl).HasMaxLength(500);

            entity.HasOne(e => e.Marca)
                  .WithMany(m => m.Productos)
                  .HasForeignKey(e => e.MarcaId);
        });

        modelBuilder.Entity<Rol>().HasData(
            new Rol { Id = 1, Nombre = "Admin" },
            new Rol { Id = 2, Nombre = "Trabajador" },
            new Rol { Id = 3, Nombre = "Cliente" }
        );

        modelBuilder.Entity<Marca>().HasData(
            new Marca { Id = 1, Nombre = "Nike" },
            new Marca { Id = 2, Nombre = "Adidas" },
            new Marca { Id = 3, Nombre = "Puma" },
            new Marca { Id = 4, Nombre = "Asics" },
            new Marca { Id = 5, Nombre = "New Balance" }
        );
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario
            {
                Id = 1,
                Nombres = "Admin",
                Apellidos = "Sistema",
                Correo = "admin@importamossports.com",
                Clave = "123456",
                Telefono = "999999999",
                Activo = true,
                RolId = 1
            }
        );
        modelBuilder.Entity<EstadoPedido>(entity =>
        {
            entity.ToTable("EstadoPedido");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.Property(e => e.TipoComprobante)
              .HasMaxLength(20)
              .HasDefaultValue("Boleta");

            entity.Property(e => e.RucFactura)
                  .HasMaxLength(11);

            entity.Property(e => e.RazonSocialFactura)
                  .HasMaxLength(200);

            entity.Property(e => e.DireccionFiscalFactura)
                  .HasMaxLength(300);

            entity.Property(e => e.SerieComprobante)
                  .HasMaxLength(10);
            entity.ToTable("Pedido");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Subtotal).HasColumnType("decimal(10,2)");
            entity.Property(e => e.DescuentoAplicado).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Igv).HasColumnType("decimal(10,2)");
            entity.Property(e => e.Total).HasColumnType("decimal(10,2)");
            entity.Property(e => e.MetodoPago).HasMaxLength(50).HasDefaultValue("Tarjeta");

            entity.Property(e => e.EstadoPago)
                  .HasMaxLength(50)
                  .HasDefaultValue("Pendiente");

            entity.Property(e => e.ObservacionPago)
                  .HasMaxLength(500);

            entity.Property(e => e.NumeroOperacion)
                  .HasMaxLength(100);

            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId);

            entity.HasOne(e => e.EstadoPedido)
                  .WithMany(e => e.Pedidos)
                  .HasForeignKey(e => e.EstadoPedidoId);

            entity.HasOne(e => e.Cupon)
                  .WithMany()
                  .HasForeignKey(e => e.CuponId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AsesorVenta)
                  .WithMany()
                  .HasForeignKey(e => e.AsesorVentaId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<DetallePedido>(entity =>
        {
            entity.ToTable("DetallePedido");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.PrecioUnitario)
                  .HasColumnType("decimal(10,2)");

            entity.Property(e => e.SubTotal)
                  .HasColumnType("decimal(10,2)");

            entity.Property(e => e.TallaUs)
                  .HasColumnType("decimal(4,1)");

            entity.HasOne(e => e.Pedido)
                  .WithMany(p => p.Detalles)
                  .HasForeignKey(e => e.PedidoId);

            entity.HasOne(e => e.Producto)
                  .WithMany()
                  .HasForeignKey(e => e.ProductoId);
        });

        modelBuilder.Entity<EstadoPedido>().HasData(
            new EstadoPedido { Id = 1, Nombre = "Pendiente" },
            new EstadoPedido { Id = 2, Nombre = "Pagado" },
            new EstadoPedido { Id = 3, Nombre = "En preparación" },
            new EstadoPedido { Id = 4, Nombre = "En camino" },
            new EstadoPedido { Id = 5, Nombre = "Entregado" },
            new EstadoPedido { Id = 6, Nombre = "Cancelado" }
        );
        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.ToTable("Notificacion");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Medio).HasMaxLength(50).IsRequired();
            entity.Property(e => e.Mensaje).HasMaxLength(500).IsRequired();

            entity.HasOne(e => e.Pedido)
                  .WithMany()
                  .HasForeignKey(e => e.PedidoId);
        });
        modelBuilder.Entity<Cupon>(entity =>
        {
            entity.ToTable("Cupon");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Codigo)
                  .HasMaxLength(50)
                  .IsRequired();

            entity.Property(e => e.TipoDescuento)
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.Valor)
                  .HasColumnType("decimal(10,2)");

            entity.Property(e => e.CompraMinima)
                  .HasColumnType("decimal(10,2)");
        });
        modelBuilder.Entity<AsesorVenta>(entity =>
        {
            entity.ToTable("AsesorVenta");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Nombres)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(e => e.Apellidos)
                  .HasMaxLength(100)
                  .IsRequired();

            entity.Property(e => e.Telefono)
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.FotoUrl)
                  .HasMaxLength(300);

            entity.Property(e => e.Activo)
                  .HasDefaultValue(true);
        });
        modelBuilder.Entity<ProductoTalla>(entity =>
        {
            entity.ToTable("ProductoTalla");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.TallaUs)
                  .HasColumnType("decimal(4,1)");

            entity.HasOne(e => e.Producto)
                  .WithMany(p => p.Tallas)
                  .HasForeignKey(e => e.ProductoId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}