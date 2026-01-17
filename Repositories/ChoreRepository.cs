using System.Text.Json;
using Chores.Models;

namespace Chores.Repositories;

public class ChoreRepository
{
    private readonly string _filePath;

    public ChoreRepository(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<Chore[]> LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = await File.ReadAllTextAsync(_filePath);
        var chores = JsonSerializer.Deserialize<Chore[]>(json) ?? [];
        return chores;
    }

    public async Task SaveAsync(Chore[] chores)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(chores, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_filePath, json);
    }
}
