namespace WoodGroveGroceriesWebApplication.EntityFramework
{
    using Entities;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class WoodGroveGroceriesDbContext : DbContext
    {
        public WoodGroveGroceriesDbContext(DbContextOptions<WoodGroveGroceriesDbContext> options)
            : base(options)
        {
        }

        public DbSet<CatalogItem> CatalogItems { get; set; }

        public DbSet<Pantry> Pantries { get; set; }

        public DbSet<Trolley> Trollies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CatalogItem>(BuildCatalogItemModel);
            modelBuilder.Entity<Pantry>(BuildPantryModel);
            modelBuilder.Entity<Trolley>(BuildTrolleyModel);
        }

        private static void BuildCatalogItemModel(EntityTypeBuilder<CatalogItem> entityTypeBuilder)
        {
            entityTypeBuilder.ToTable("Catalog");

            entityTypeBuilder.Property(catalogItem => catalogItem.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            entityTypeBuilder.Property(catalogItem => catalogItem.OwnerId)
                .IsRequired();

            entityTypeBuilder.Property(catalogItem => catalogItem.ProductId)
                .IsRequired();

            entityTypeBuilder.Property(catalogItem => catalogItem.ProductName)
                .IsRequired();

            entityTypeBuilder.Property(catalogItem => catalogItem.ProductPictureUrl)
                .IsRequired();
        }

        private static void BuildPantryModel(EntityTypeBuilder<Pantry> entityTypeBuilder)
        {
            entityTypeBuilder.Property(pantryItem => pantryItem.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            var navigation = entityTypeBuilder.Metadata.FindNavigation(nameof(Pantry.Items));
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        }

        private static void BuildTrolleyModel(EntityTypeBuilder<Trolley> entityTypeBuilder)
        {
            entityTypeBuilder.Property(trollyItem => trollyItem.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            var navigation = entityTypeBuilder.Metadata.FindNavigation(nameof(Trolley.Items));
            navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}