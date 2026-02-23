namespace pacman.Services;

public interface IScoreService
{
    Task<List<ScoreEntry>> GetScoresAsync();
    Task SaveScoreAsync(ScoreEntry entry);
    Task<List<ScoreEntry>> GetTopScoresAsync(int count);
}
