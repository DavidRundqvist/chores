using System.Text.Json;
using Chores.Models;

namespace Chores.Repositories;

public class RecordRepository
{
    private readonly string _filePath;

    public RecordRepository(string filePath)
    {
        _filePath = filePath;
    }

    public async Task<Record[]> LoadAsync()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json = await File.ReadAllTextAsync(_filePath);
        var records = JsonSerializer.Deserialize<Record[]>(json) ?? [];
        return records;
    }

    public async Task SaveAsync(Record[] records)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_filePath, json);
    }
}
