using System.Collections.ObjectModel;
using Microsoft.UI.Xaml;

namespace pacman.Presentation;

public class RankedScoreEntry
{
    public int Rank { get; }
    public string PlayerName { get; }
    public int Score { get; }

    public string RankLabel => $"#{Rank}";
    public string ScoreLabel => Score.ToString("N0");

    public RankedScoreEntry(int rank, ScoreEntry entry)
    {
        Rank = rank;
        PlayerName = entry.PlayerName;
        Score = entry.Score;
    }
}

public partial class HighScoresViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IAudioService _audio;
    private readonly IScoreService _scoreService;

    public ObservableCollection<RankedScoreEntry> TopThree { get; } = new();
    public ObservableCollection<RankedScoreEntry> OtherScores { get; } = new();

    public Visibility EmptyVisibility =>
        TopThree.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

    public HighScoresViewModel(INavigator navigator, IAudioService audio, IScoreService scoreService)
    {
        _navigator = navigator;
        _audio = audio;
        _scoreService = scoreService;
    }

    public async Task LoadScoresAsync()
    {
        var scores = await _scoreService.GetTopScoresAsync(10);

        TopThree.Clear();
        OtherScores.Clear();

        int rank = 1;
        foreach (var score in scores)
        {
            var entry = new RankedScoreEntry(rank, score);
            if (rank <= 3)
                TopThree.Add(entry);
            else
                OtherScores.Add(entry);
            rank++;
        }

        OnPropertyChanged(nameof(EmptyVisibility));
    }

    [RelayCommand]
    private async Task GoBack()
    {
        _audio.PlayUiClick();
        await _navigator.NavigateBackAsync(this);
    }
}
