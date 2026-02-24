using Windows.System;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;

namespace pacman.Presentation;

public sealed partial class GamePage : Page
{
    public GamePageViewModel ViewModel => (DataContext as GamePageViewModel)!;
    private readonly DispatcherTimer _timer;

    private const int CellSize = 16;

    private readonly Dictionary<string, Ellipse> _dotEllipses = new();
    private Image? _pacmanImage;
    private BitmapImage? _pacmanBitmap;

    private BitmapImage? _deadPacmanBitmap;

    private readonly Dictionary<GhostPersonality, Image>       _ghostImages  = new();
    private readonly Dictionary<GhostPersonality, BitmapImage> _ghostBitmaps = new();
    private BitmapImage? _frightenedBitmap;
    private BitmapImage? _frightenedBitmap2;
    private bool _blinkToggle;

    private static readonly Dictionary<GhostPersonality, string> GhostSpritePaths = new()
    {
        { GhostPersonality.Blinky, "ms-appx:///Assets/Sprites/ghosts/red_ghost.png"    },
        { GhostPersonality.Pinky,  "ms-appx:///Assets/Sprites/ghosts/pink_ghost.png"   },
        { GhostPersonality.Inky,   "ms-appx:///Assets/Sprites/ghosts/blue_ghost.png"   },
        { GhostPersonality.Clyde,  "ms-appx:///Assets/Sprites/ghosts/yellow_ghost.png" },
    };

    public GamePage()
    {
        this.InitializeComponent();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _timer.Tick += GameTimer_Tick;

        this.DataContextChanged += (s, e) =>
        {
            if (ViewModel == null) return;

            DispatcherQueue.TryEnqueue(() =>
            {
                if (ViewModel?.Maze == null) return;

                GameCanvas.Children.Clear();
                _dotEllipses.Clear();
                _ghostImages.Clear();
                _pacmanImage = null;

                DrawMaze();
                DrawPacman();
                DrawGhosts();
                ViewModel.Audio.PlayGameStart();
                _timer.Start();

                GameCanvas.Focus(FocusState.Programmatic);
            });
        };
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        _timer.Stop();
    }

    // ── Labirinto ────────────────────────────────────────────────────────────
    private void DrawMaze()
    {
        var maze = ViewModel.Maze;

        if (maze == null) return;

        for (int row = 0; row < maze.GetLength(0); row++)
        {
            for (int col = 0; col < maze.GetLength(1); col++)
            {
                char cell = maze[row, col];
                double x = col * CellSize;
                double y = row * CellSize;

                switch (cell)
                {
                    case '#':
                        var wall = new Rectangle
                        {
                            Width           = CellSize,
                            Height          = CellSize,
                            Fill            = new SolidColorBrush(Colors.Blue),
                            Stroke          = new SolidColorBrush(
                                ColorHelper.FromArgb(255, 61, 90, 254)),
                            StrokeThickness = 1
                        };
                        Canvas.SetLeft(wall, x);
                        Canvas.SetTop(wall, y);
                        GameCanvas.Children.Add(wall);
                        break;

                    case '.':
                        AddDot(row, col, x, y, dotSize: 4, offset: 2);
                        break;

                    case '*':
                        AddDot(row, col, x, y, dotSize: 8, offset: 4);
                        break;
                }
            }
        }
    }

    private void AddDot(int row, int col, double x, double y, double dotSize, double offset)
    {
        var dot = new Ellipse
        {
            Width  = dotSize,
            Height = dotSize,
            Fill   = new SolidColorBrush(
                ColorHelper.FromArgb(255, 255, 227, 76))
        };
        Canvas.SetLeft(dot, x + CellSize / 2.0 - offset);
        Canvas.SetTop(dot,  y + CellSize / 2.0 - offset);
        GameCanvas.Children.Add(dot);
        _dotEllipses[$"{row},{col}"] = dot;
    }

    // ── Pac-Man ──────────────────────────────────────────────────────────────
    private void DrawPacman()
    {
        _pacmanBitmap ??= new BitmapImage(new Uri("ms-appx:///Assets/Sprites/pacman/pacman.png"));

        if (_pacmanImage == null)
        {
            _pacmanImage = new Image
            {
                Width                 = CellSize - 2,
                Height                = CellSize - 2,
                Source                = _pacmanBitmap,
                RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5),
                RenderTransform       = new RotateTransform()
            };
            GameCanvas.Children.Add(_pacmanImage);
        }

        ((RotateTransform)_pacmanImage.RenderTransform).Angle = ViewModel.Pacman.GetRotationAngle() + 180;
        Canvas.SetLeft(_pacmanImage, ViewModel.PacmanX * CellSize + 1);
        Canvas.SetTop(_pacmanImage,  ViewModel.PacmanY * CellSize + 1);
    }

    // ── Fantasmas ────────────────────────────────────────────────────────────
    private void DrawGhosts()
    {
        _frightenedBitmap  ??= new BitmapImage(new Uri("ms-appx:///Assets/Sprites/ghosts/frightened.png"));
        _frightenedBitmap2 ??= new BitmapImage(new Uri("ms-appx:///Assets/Sprites/ghosts/frightened2.png"));

        foreach (var ghost in ViewModel.Ghosts)
        {
            if (!_ghostBitmaps.ContainsKey(ghost.Personality))
                _ghostBitmaps[ghost.Personality] = new BitmapImage(
                    new Uri(GhostSpritePaths[ghost.Personality]));

            if (!_ghostImages.ContainsKey(ghost.Personality))
            {
                var img = new Image
                {
                    Width  = CellSize - 2,
                    Height = CellSize - 2,
                    Source = _ghostBitmaps[ghost.Personality]
                };
                GameCanvas.Children.Add(img);
                _ghostImages[ghost.Personality] = img;
            }

            UpdateGhostVisual(ghost);
        }
    }

    private void UpdateGhostVisual(Ghost ghost)
    {
        if (!_ghostImages.TryGetValue(ghost.Personality, out var img)) return;

        if (ghost.State == GhostState.Frightened)
            img.Source = ViewModel.IsFrightenedWarning && _blinkToggle
                ? _frightenedBitmap2
                : _frightenedBitmap;
        else
            img.Source = _ghostBitmaps[ghost.Personality];

        Canvas.SetLeft(img, ghost.Col * CellSize + 1);
        Canvas.SetTop(img,  ghost.Row * CellSize + 1);
    }

    // ── Tick ─────────────────────────────────────────────────────────────────
    private async void GameTimer_Tick(object? sender, object e)
    {
        _blinkToggle = !_blinkToggle;
        var result = ViewModel.ProcessTick();

        DrawPacman();

        string dotKey = $"{ViewModel.PacmanY},{ViewModel.PacmanX}";
        if (_dotEllipses.TryGetValue(dotKey, out var dotEllipse))
        {
            GameCanvas.Children.Remove(dotEllipse);
            _dotEllipses.Remove(dotKey);
        }

        foreach (var ghost in ViewModel.Ghosts)
            UpdateGhostVisual(ghost);

        switch (result)
        {
            case TickResult.PacmanDied:
                _timer.Stop();
                ShowDeathSprite();
                ViewModel.LoseLife();

                bool gameOver = ViewModel.IsGameOver();
                if (gameOver) ViewModel.Audio.PlayGameOver();
                else          ViewModel.Audio.PlayLoseLife();

                await Task.Delay(700);

                if (gameOver)
                {
                    await ShowGameOverDialog();
                    Frame.GoBack();
                }
                else
                {
                    ViewModel.ResetPositions();
                    if (_pacmanImage != null)
                        _pacmanImage.Source = _pacmanBitmap;
                    DrawPacman();
                    foreach (var ghost in ViewModel.Ghosts)
                        UpdateGhostVisual(ghost);
                    _timer.Start();
                }
                break;

            case TickResult.LevelCleared:
                _timer.Stop();
                ViewModel.Audio.PlayWin();
                await ShowWinDialog();
                Frame.GoBack();
                break;
        }
    }

    // ── Teclado ──────────────────────────────────────────────────────────────
    private void GameCanvas_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case VirtualKey.Up:
            case VirtualKey.W:
                ViewModel.SetDirection(0, -1);
                break;
            case VirtualKey.Down:
            case VirtualKey.S:
                ViewModel.SetDirection(0, 1);
                break;
            case VirtualKey.Left:
            case VirtualKey.A:
                ViewModel.SetDirection(-1, 0);
                break;
            case VirtualKey.Right:
            case VirtualKey.D:
                ViewModel.SetDirection(1, 0);
                break;
            case VirtualKey.Escape:
                _timer.Stop();
                Frame.GoBack();
                break;
        }
        e.Handled = true;
    }

    // ── Sprites de estado ────────────────────────────────────────────────────
    private void ShowDeathSprite()
    {
        _deadPacmanBitmap ??= new BitmapImage(new Uri("ms-appx:///Assets/Sprites/pacman/deadpacman.png"));
        if (_pacmanImage == null) return;

        _pacmanImage.Source = _deadPacmanBitmap;

        // Traz o Pac-Man para frente dos fantasmas no canvas
        GameCanvas.Children.Remove(_pacmanImage);
        GameCanvas.Children.Add(_pacmanImage);
    }

    // ── Diálogos ─────────────────────────────────────────────────────────────
    private async Task ShowWinDialog()
    {
        var dialog = new ContentDialog
        {
            Title           = "VOCÊ VENCEU! 🎉",
            Content         = $"Pontuação: {ViewModel.Score}\n\nPressione OK para voltar ao menu.",
            CloseButtonText = "OK",
            XamlRoot        = this.XamlRoot
        };
        await dialog.ShowAsync();
    }

    private async Task ShowGameOverDialog()
    {
        var dialog = new ContentDialog
        {
            Title           = "GAME OVER",
            Content         = $"Pontuação final: {ViewModel.Score}\n\nPressione OK para voltar ao menu.",
            CloseButtonText = "OK",
            XamlRoot        = this.XamlRoot
        };
        await dialog.ShowAsync();
    }
}
