namespace DevManager.Domain.Entities;

public class Developer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Seniority { get; set; } = string.Empty;
    public Guid CityId { get; set; }
    public City? City { get; set; }
    public string? Observations { get; set; }
    public ICollection<DeveloperProgrammingLanguage> DeveloperProgrammingLanguages { get; set; } = new List<DeveloperProgrammingLanguage>();
    public ICollection<ProgrammingLanguage> ProgrammingLanguages { get; set; } = new List<ProgrammingLanguage>();
}
