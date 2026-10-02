using CarRent.Domain.Enums;

namespace CarRent.Application.Models.Cars;

public class CarFilterModel : BaseSpecificationParameters
{
    public BodyType? BodyType { get; set; }
    public TransmissionType? TransmissionType { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
}