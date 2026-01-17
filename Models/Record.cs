namespace Chores.Models;

public class Record
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ChoreId { get; set; }
    public DateTime PerformedAt { get; set; }
}
