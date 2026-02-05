namespace pacman.Presentation;

public partial  class PacmanMenuViewModel : ObservableObject
{
    
    private INavigator _navigator;
    
    public PacmanMenuViewModel(INavigator navigator)
    {
        _navigator = navigator;
        StartGameCommand = new AsyncRelayCommand(StartGame);
    }
    
    public ICommand StartGameCommand { get; }
    
    [ObservableProperty]
    private bool isSoundEnabled = true;
    
    private async Task StartGame()
    {
        await _navigator.NavigateViewModelAsync<GamePageViewModel>(this);
    }

    [RelayCommand]
    private void ShowHighScores()
    {
        // Implementar lógica para mostrar pontuações
        // Exemplo: NavigationService.Navigate(typeof(HighScoresPage));
    }

    [RelayCommand]
    private void ToggleSound()
    {
        IsSoundEnabled = !IsSoundEnabled;
        // Implementar lógica para ativar/desativar som
    }

    [RelayCommand]
    private void Exit()
    {
        // Implementar lógica para sair
        // Exemplo: NavigationService.GoBack();
    }
}
