using Microsoft.EntityFrameworkCore;

namespace TodoApi.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // DbSet para los TestUsers
        public DbSet<TestUser> TestUsers { get; set; } = null!;

        // DbSet para los Posts
        public DbSet<Post> Posts { get; set; } = null!;

        // Configuración adicional (opcional)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuraciones específicas para cada entidad pueden ir aquí, por ejemplo:
            // Configuración de índice único para el nombre de usuario
            modelBuilder.Entity<TestUser>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Puedes definir relaciones, índices, y otras configuraciones aquí si es necesario
        }
    }
}

