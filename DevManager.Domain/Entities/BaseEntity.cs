using DevManager.Domain.Interfaces;

namespace DevManager.Domain.Entities;

public abstract class BaseEntity : ISoftDelete
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted => DeletedAt.HasValue;
}
