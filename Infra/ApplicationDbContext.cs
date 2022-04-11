using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext {

        public DbSet<User> Users { get; set; }
        public DbSet<FireSaleItem> FireSaleItems { get; set; }
        public DbSet<Purchase> Purchases { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql("Host=127.0.0.1;Database=flashsales;Username=development;Password=development");

}
