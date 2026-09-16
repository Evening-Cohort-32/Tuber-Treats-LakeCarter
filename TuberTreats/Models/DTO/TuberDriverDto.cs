namespace TuberTreats.Models.DTO;

public class TuberDriverDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<TuberOrderDto> TuberDeliveries { get; set; }
}