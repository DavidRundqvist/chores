namespace Chores.Models;

public class Chore
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public TimeSpan Frequency { get; set; }
}
