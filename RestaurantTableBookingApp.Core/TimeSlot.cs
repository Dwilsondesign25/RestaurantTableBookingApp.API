using System.ComponentModel.DataAnnotations;



namespace RestaurantTableBookingApp.Core;

public partial class TimeSlot
{
    [Key]
    public int Id { get; set; }

    public int DiningTableId { get; set; }

    public DateTime ReservationDay { get; set; }

    public string MealType { get; set; } = null!;

    public string TableStatus { get; set; } = null!;


    public virtual DiningTable DiningTable { get; set; } = null!;

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
