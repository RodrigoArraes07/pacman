namespace pacman.Presentation;

public partial class PacmanMenuViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IAudioService _audio;

    public PacmanMenuViewModel(INavigator navigator, IAudioService audio)
    {
        _navigator = navigator;
        _audio = audio;
        StartGameCommand = new AsyncRelayCommand(StartGame);
    }

    public ICommand StartGameCommand { get; }

    [ObservableProperty]
    private bool isSoundEnabled = true;

    private async Task StartGame()
    {
        _audio.PlayUiClick();
        await _navigator.NavigateViewModelAsync<GamePageViewModel>(this);
    }

    [RelayCommand]
    private async Task ShowHighScores()
    {
        _audio.PlayUiClick();
        await _navigator.NavigateViewModelAsync<HighScoresViewModel>(this);
    }

    [RelayCommand]
    private void ToggleSound()
    {
        _audio.IsMuted = !_audio.IsMuted;
        IsSoundEnabled = !_audio.IsMuted;
        if (IsSoundEnabled)
            _audio.PlayUiClick();
    }

    [RelayCommand]
    private void Exit()
    {
        _audio.PlayUiClick();
        Environment.Exit(0);
    }
}
