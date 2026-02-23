namespace pacman.Models;

public record ScoreEntry
{
    public string PlayerName { get; init; } = string.Empty;
    public int Score { get; init; }
    public DateTime Date { get; init; }
}