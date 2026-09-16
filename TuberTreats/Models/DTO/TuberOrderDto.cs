namespace TuberTreats.Models.DTO;

public class TuberOrderDto
{
    public int Id { get; set; }
    public DateTime OrderPlacedOnDate { get; set; }
    public int CustomerId { get; set; }
    public int? TuberDriverId { get; set; }
    public DateTime? DeliveredOnDate { get; set; }
    public List<ToppingDto> Toppings { get; set; }
    public CustomerDto Customer { get; set; }
    public TuberDriverDto Driver { get; set; }
}