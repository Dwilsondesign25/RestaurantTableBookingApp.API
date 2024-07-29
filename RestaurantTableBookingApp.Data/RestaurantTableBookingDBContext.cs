using Microsoft.EntityFrameworkCore;
using RestaurantTableBookingApp.Core;
using RestaurantTableBookingApp.Core.ViewModels;

namespace RestaurantTableBookingApp.Data
{
    public class RestaurantTableBookingDbContext : DbContext
    {
        public RestaurantTableBookingDbContext(DbContextOptions<RestaurantTableBookingDbContext> options)
            : base(options)
        { }

        public DbSet<Restaurant> Restaurant { get; set; }
        public DbSet<DiningTable> DiningTable { get; set; }
        public DbSet<Reservation> Reservation { get; set; }
        public DbSet<RestaurantBranch> RestaurantBranches { get; set; }
        public DbSet<TimeSlot> TimeSlots { get; set; }
        public DbSet<User> User { get; set; }

        // Define DbSet for DiningTableWithTimeSlotsModel as a keyless entity
        public DbSet<DiningTableWithTimeSlotsModel> DiningTableWithTimeSlots { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure DiningTableWithTimeSlotsModel as a keyless entity
            modelBuilder.Entity<DiningTableWithTimeSlotsModel>(entity =>
            {
                entity.HasNoKey();
                entity.ToView(null); // Optional: specify a view name if needed
            });

            // Additional configuration for other entities if needed
        }
    }
}
