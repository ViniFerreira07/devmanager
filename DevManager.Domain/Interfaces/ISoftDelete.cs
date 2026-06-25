namespace DevManager.Domain.Interfaces;

public interface ISoftDelete
{
    DateTime? DeletedAt { get; set; }
}
