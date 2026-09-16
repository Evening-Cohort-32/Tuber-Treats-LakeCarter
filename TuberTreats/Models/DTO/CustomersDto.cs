namespace TuberTreats.Models.DTO;

public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public List<TuberOrderDto> TuberOrders { get; set; }
}