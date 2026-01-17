namespace Chores.Models;

public class Chore
{
    public string Name { get; set; } = string.Empty;
    public TimeSpan Frequency { get; set; }
}
