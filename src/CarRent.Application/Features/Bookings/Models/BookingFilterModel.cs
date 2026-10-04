namespace CarRent.Application.Models.Bookings;

public class BookingFilterModel : BaseSpecificationParameters
{
    public Guid? CarId { get; set; }
    public Guid? UserId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}