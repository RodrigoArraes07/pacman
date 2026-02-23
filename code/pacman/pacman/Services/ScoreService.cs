using System.Text.Json;
using Windows.Storage;

namespace pacman.Services;

public class ScoreService : IScoreService
{
    private const string FileName = "scores.json";

    private static string GetFilePath() =>
        Path.Combine(ApplicationData.Current.LocalFolder.Path, FileName);

    public async Task<List<ScoreEntry>> GetScoresAsync()
    {
        var path = GetFilePath();

        if (!File.Exists(path))
            return [];

        var json = await File.ReadAllTextAsync(path);
        var scores = JsonSerializer.Deserialize<List<ScoreEntry>>(json) ?? [];

        return [.. scores.OrderByDescending(s => s.Score)];
    }

    public async Task SaveScoreAsync(ScoreEntry entry)
    {
        var scores = await GetScoresAsync();
        scores.Add(entry);

        var json = JsonSerializer.Serialize(scores, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(GetFilePath(), json);
    }

    public async Task<List<ScoreEntry>> GetTopScoresAsync(int count)
    {
        var scores = await GetScoresAsync();
        return [.. scores.Take(count)];
    }
}
