namespace DevManager.Domain.Entities;

public class State : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string UF { get; set; } = string.Empty;
    public ICollection<City> Cities { get; set; } = new List<City>();
}
