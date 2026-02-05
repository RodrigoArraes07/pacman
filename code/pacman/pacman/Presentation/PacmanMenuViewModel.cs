namespace pacman.Presentation;

public partial  class PacmanMenuViewModel : ObservableObject
{
    [ObservableProperty]
    private bool isSoundEnabled = true;

    [RelayCommand]
    private void StartGame()
    {
        // Implementar lógica para iniciar o jogo
        // Exemplo: NavigationService.Navigate(typeof(GamePage));
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
