using Microsoft.EntityFrameworkCore;

namespace skladMVC.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Item> Items { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;

        public DbSet<Catalog> Catalogs { get; set; } = null!;

        public DbSet<Material> Materials { get; set; } = null!;

        public DbSet<User> Users { get; set; } = null!;

        public DbSet<Job> Jobs { get; set; } = null!;

        public DbSet<CartItem> CartItems { get; set; } = null!;

        public DbSet<Status> Statuses { get; set; } = null!;
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
            Database.EnsureCreated();   // создаем базу данных при первом обращении
        }
    }
}