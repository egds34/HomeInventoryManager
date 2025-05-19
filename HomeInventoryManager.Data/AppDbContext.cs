
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using HomeInventoryManager.Data.ValueConverters;

namespace HomeInventoryManager.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        //I realized I could have used the model builder after changing literally everything. So it is staying this way.
        public DbSet<Item> ITEMS { get; set; }
        public DbSet<Category> CATEGORIES { get; set; }
        public DbSet<Receipt> RECEIPTS { get; set; }
        public DbSet<Location> LOCATIONS { get; set; }
        public DbSet<Photo> PHOTOS { get; set; }
        public DbSet<ItemTag> ITEM_TAGS { get; set; }
        public DbSet<Tag> TAGS { get; set; }
        public DbSet<User> USERSET { get; set; } // user table

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //We arent using this. But its throwing errors if not specified.
            modelBuilder.Entity<IdentityUserLogin<string>>()
                .HasKey(login => new { login.LoginProvider, login.ProviderKey });

            modelBuilder.Entity<User>().HasKey(u => u.user_id);
            modelBuilder.Entity<Category>().HasKey(c => c.id);
            modelBuilder.Entity<Item>().HasKey(i => i.id);
            modelBuilder.Entity<Location>().HasKey(l => l.id);
            modelBuilder.Entity<Photo>().HasKey(p => p.id);
            modelBuilder.Entity<Receipt>().HasKey(r => r.id);
            modelBuilder.Entity<Tag>().HasKey(t => t.id);
            modelBuilder.Entity<ItemTag>().HasKey(it => new { it.item_id, it.tag_id }); // Composite key
            base.OnModelCreating(modelBuilder);
        }
    }
}
