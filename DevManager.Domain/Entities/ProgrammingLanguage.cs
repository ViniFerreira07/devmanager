using DevManager.Domain.Enums;

namespace DevManager.Domain.Entities;

public class ProgrammingLanguage : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ProgrammingLanguageType Type { get; set; }
    public ICollection<DeveloperProgrammingLanguage> DeveloperProgrammingLanguages { get; set; } = new List<DeveloperProgrammingLanguage>();
    public ICollection<Developer> Developers { get; set; } = new List<Developer>();
}
