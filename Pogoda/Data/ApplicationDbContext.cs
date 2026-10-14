using Microsoft.EntityFrameworkCore;

namespace Pogoda.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<SavedLocation> SavedLocations => Set<SavedLocation>();
        public DbSet<WeatherChart> WeatherCharts { get; set; }
        public DbSet<UserSettings> UserSettings => Set<UserSettings>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");

                entity.HasKey(user => user.Id);

                entity.Property(user => user.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(user => user.Username)
                    .HasColumnName("username")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(user => user.Email)
                    .HasColumnName("email")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.HasIndex(user => user.Email)
                    .IsUnique();

                entity.Property(user => user.PasswordHash)
                    .HasColumnName("password_hash")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(user => user.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp")
                    .IsRequired();
            });

            modelBuilder.Entity<SavedLocation>(entity =>
            {
                entity.ToTable("SavedLocations");

                entity.HasKey(location => location.Id);

                entity.Property(location => location.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(location => location.UserId)
                    .IsRequired();

                entity.Property(location => location.Name)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(location => location.Address)
                    .HasMaxLength(255);

                entity.Property(location => location.Latitude)
                    .HasPrecision(9, 6)
                    .IsRequired();

                entity.Property(location => location.Longitude)
                    .HasPrecision(9, 6)
                    .IsRequired();

                entity.Property(location => location.CreatedAt)
                    .HasColumnType("datetime")
                    .IsRequired();

                entity.HasIndex(location => location.UserId);

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(location => location.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<WeatherChart>(entity =>
            {
                entity.ToTable("WeatherCharts");
                entity.HasKey(chart => chart.Id);
                entity.Property(chart => chart.Id).ValueGeneratedOnAdd();
                entity.Property(chart => chart.UserId).IsRequired();
                entity.Property(chart => chart.Name).HasMaxLength(100).IsRequired();
                entity.Property(chart => chart.LocationName).HasMaxLength(100).IsRequired();
                entity.Property(chart => chart.Latitude).HasPrecision(9, 6).IsRequired();
                entity.Property(chart => chart.Longitude).HasPrecision(9, 6).IsRequired();
                entity.Property(chart => chart.StartDate).HasColumnType("date").IsRequired();
                entity.Property(chart => chart.EndDate).HasColumnType("date").IsRequired();
                entity.Property(chart => chart.CreatedAt).HasColumnType("datetime").IsRequired();
                entity.HasIndex(chart => chart.UserId);
                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(chart => chart.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserSettings>(entity =>
            {
                entity.ToTable("UserSettings");
                entity.HasKey(settings => settings.Id);

                entity.Property(settings => settings.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(settings => settings.UserId)
                    .IsRequired();

                entity.Property(settings => settings.TemperatureUnit)
                    .HasMaxLength(2)
                    .IsRequired();

                entity.Property(settings => settings.WindSpeedUnit)
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(settings => settings.Theme)
                    .HasMaxLength(10)
                    .IsRequired();

                entity.Property(settings => settings.UpdatedAt)
                    .HasColumnType("datetime")
                    .IsRequired();

                entity.HasIndex(settings => settings.UserId)
                    .IsUnique();

                entity.HasOne<User>()
                    .WithMany()
                    .HasForeignKey(settings => settings.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}