namespace pacman.Presentation;

public sealed partial class HighScoresPage : Page
{
    private HighScoresViewModel? ViewModel => DataContext as HighScoresViewModel;

    public HighScoresPage()
    {
        this.InitializeComponent();

        this.DataContextChanged += async (s, e) =>
        {
            if (ViewModel == null) return;
            await ViewModel.LoadScoresAsync();
        };
    }
}
