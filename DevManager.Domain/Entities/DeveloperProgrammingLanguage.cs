namespace DevManager.Domain.Entities;

public class DeveloperProgrammingLanguage
{
    public Guid DeveloperId { get; set; }
    public Developer Developer { get; set; } = null!;
    public Guid ProgrammingLanguageId { get; set; }
    public ProgrammingLanguage ProgrammingLanguage { get; set; } = null!;
}
