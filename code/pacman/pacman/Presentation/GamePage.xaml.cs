using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Composition;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Rectangle = Microsoft.UI.Xaml.Shapes.Rectangle;

namespace pacman.Presentation;

public sealed partial class GamePage : Page
{
    // ──────────────────────── ViewModel e timer ────────────────────────────────────────────────────
    public GamePageViewModel ViewModel => DataContext as GamePageViewModel;
    private readonly DispatcherTimer _timer;

    // ─────────────────────── Constantes visuais ───────────────────────────────────────────────────
    private const int CellSize = 16;

    // ─────────────────────── Elementos visuais ────────────────────────────────────────────────────
    private readonly Dictionary<string, Rectangle> _wallRects   = new();
    private readonly Dictionary<string, Ellipse>   _dotEllipses = new();
    private Ellipse _pacmanEllipse;

    // Um ellipse por fantasma, indexado por personalidade
    private readonly Dictionary<GhostPersonality, Ellipse> _ghostEllipses = new();

    // ── Cores dos fantasmas assustados ───────────────────────────────────────
    private static readonly Windows.UI.Color FrightenedColor =
        Microsoft.UI.ColorHelper.FromArgb(255, 0, 0, 255); // azul

    // ────────────────────────────────── Construtor ─────────────────────────────────────
    public GamePage()
    {
        this.InitializeComponent();

        // Timer: 150 ms ≈ ~6 fps
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(150) };
        _timer.Tick += GameTimer_Tick;

        // this.KeyDown += GamePage_KeyDown;
        
        this.DataContextChanged += (s, e) =>
        {
            if (ViewModel == null) return;
        
            DispatcherQueue.TryEnqueue(() =>
            {
                if (ViewModel?.Maze == null) return;
                
                GameCanvas.Children.Clear();
                _wallRects.Clear();
                _dotEllipses.Clear();
                _ghostEllipses.Clear();
                _pacmanEllipse = null;
            
                DrawMaze();
                DrawPacman();
                DrawGhosts();
                _timer.Start();

                GameCanvas.Focus(FocusState.Programmatic);
            });
        };
        
    }
    
    // Responsavel pela navegacao via teclado
    //private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, 
    //    Windows.UI.Core.KeyEventArgs args)
    //{
    //    switch (args.VirtualKey)
    //    {
    //        case VirtualKey.Up:
    //        case VirtualKey.W:
    //            ViewModel?.SetDirection(0, -1);
    //            break;
    //        case VirtualKey.Down:
    //        case VirtualKey.S:
    //            ViewModel?.SetDirection(0, 1);
    //            break;
    //        case VirtualKey.Left:
    //        case VirtualKey.A:
    //            ViewModel?.SetDirection(-1, 0);
    //            break;
    //        case VirtualKey.Right:
    //        case VirtualKey.D:
    //            ViewModel?.SetDirection(1, 0);
    //            break;
    //        case VirtualKey.Escape:
    //            _timer.Stop();
    //            DispatcherQueue.TryEnqueue(() => Frame.GoBack());
    //            break;
    //    }
    // }

    //protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
    //{
    //    base.OnNavigatingFrom(e);
    //    _timer.Stop();
    //
    //    // Desregistra para não vazar memória
    //    var window = Microsoft.UI.Xaml.Window.Current;
    //    if (window != null)
    //        window.CoreWindow.KeyDown -= CoreWindow_KeyDown;
    //}

    // ────────────────────────────── Canvas carregado ───────────────────────────
    // protected override void OnNavigatedTo(NavigationEventArgs e)
    // {
    //     base.OnNavigatedTo(e);
    //
    //     DispatcherQueue.TryEnqueue(() =>
    //     {
    //         TestCanvas();
    //         
    //         // DEBUG
    //         System.Diagnostics.Debug.WriteLine($"ViewModel: {ViewModel != null}");
    //         System.Diagnostics.Debug.WriteLine($"Maze: {ViewModel?.Maze != null}");
    //         System.Diagnostics.Debug.WriteLine($"Maze rows: {ViewModel?.Maze?.GetLength(0)}");
    //         System.Diagnostics.Debug.WriteLine($"Maze cols: {ViewModel?.Maze?.GetLength(1)}");
    //         DrawMaze();
    //         DrawPacman();
    //         DrawGhosts();
    //         _timer.Start();
    //         this.Focus(FocusState.Programmatic);
    //     });
    // }
    
    private void TestCanvas()
    {
        var rect = new Rectangle
        {
            Width = 100,
            Height = 100,
            Fill = new SolidColorBrush(Colors.Red)
        };
        Canvas.SetLeft(rect, 50);
        Canvas.SetTop(rect, 50);
        GameCanvas.Children.Add(rect);
    }
    
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
        _timer.Stop();
    }

    // ── Desenho do labirinto ─────────────────────────────────────────────────
    private void DrawMaze()
    {
        var maze = ViewModel.Maze;
        
        if (maze == null)
        {
            // Mostra aviso visual
            var txt = new TextBlock
            {
                Text = "MAZE NULL!",
                Foreground = new SolidColorBrush(Colors.Red),
                FontSize = 20
            };
            Canvas.SetLeft(txt, 50);
            Canvas.SetTop(txt, 200);
            GameCanvas.Children.Add(txt);
            return;
        }

        // Mostra quantas linhas encontrou
        var info = new TextBlock
        {
            Text = $"Maze: {maze.GetLength(0)} x {maze.GetLength(1)}",
            Foreground = new SolidColorBrush(Colors.Green),
            FontSize = 16
        };
        Canvas.SetLeft(info, 10);
        Canvas.SetTop(info, 10);
        GameCanvas.Children.Add(info);

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
                            Width  = CellSize,
                            Height = CellSize,
                            Fill   = new SolidColorBrush(Colors.Blue),
                            Stroke = new SolidColorBrush(
                                Microsoft.UI.ColorHelper.FromArgb(255, 61, 90, 254)),
                            StrokeThickness = 1
                        };
                        Canvas.SetLeft(wall, x);
                        Canvas.SetTop(wall, y);
                        GameCanvas.Children.Add(wall);
                        _wallRects[$"{row},{col}"] = wall;
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
                Microsoft.UI.ColorHelper.FromArgb(255, 255, 227, 76))
        };
        Canvas.SetLeft(dot, x + CellSize / 2.0 - offset);
        Canvas.SetTop(dot,  y + CellSize / 2.0 - offset);
        GameCanvas.Children.Add(dot);
        _dotEllipses[$"{row},{col}"] = dot;
    }

    // ── Desenho e atualização do Pac-Man ─────────────────────────────────────
    private void DrawPacman()
    {
        if (_pacmanEllipse == null)
        {
            _pacmanEllipse = new Ellipse
            {
                Width  = CellSize - 2,
                Height = CellSize - 2,
                Fill   = new SolidColorBrush(
                    Microsoft.UI.ColorHelper.FromArgb(255, 255, 227, 76))
            };
            GameCanvas.Children.Add(_pacmanEllipse);
        }

        Canvas.SetLeft(_pacmanEllipse, ViewModel.PacmanX * CellSize + 1);
        Canvas.SetTop(_pacmanEllipse,  ViewModel.PacmanY * CellSize + 1);
    }

    // ── Desenho e atualização dos fantasmas ──────────────────────────────────
    private void DrawGhosts()
    {
        foreach (var ghost in ViewModel.Ghosts)
        {
            if (!_ghostEllipses.ContainsKey(ghost.Personality))
            {
                var (r, g, b) = ghost.BaseColor;
                var ellipse = new Ellipse
                {
                    Width  = CellSize - 2,
                    Height = CellSize - 2,
                    Fill   = new SolidColorBrush(
                        Microsoft.UI.ColorHelper.FromArgb(255, r, g, b))
                };
                GameCanvas.Children.Add(ellipse);
                _ghostEllipses[ghost.Personality] = ellipse;
            }

            UpdateGhostVisual(ghost);
        }
    }

    private void UpdateGhostVisual(Ghost ghost)
    {
        if (!_ghostEllipses.TryGetValue(ghost.Personality, out var ellipse)) return;

        // Cor: azul quando assustado
        Windows.UI.Color color;
        if (ghost.State == GhostState.Frightened)
        {
            color = FrightenedColor;
        }
        else
        {
            var (r, g, b) = ghost.BaseColor;
            color = Microsoft.UI.ColorHelper.FromArgb(255, r, g, b);
        }

        ellipse.Fill = new SolidColorBrush(color);
        Canvas.SetLeft(ellipse, ghost.Col * CellSize + 1);
        Canvas.SetTop(ellipse,  ghost.Row * CellSize + 1);
    }

    // ──────────────────── Tick do jogo ──────────────────────────────
    private async void GameTimer_Tick(object sender, object e)
    {
        var result = ViewModel.ProcessTick();

        // Atualiza visual do Pac-Man
        DrawPacman();

        // Remove ponto comido do canvas
        string dotKey = $"{ViewModel.PacmanY},{ViewModel.PacmanX}";
        if (_dotEllipses.TryGetValue(dotKey, out var dotEllipse))
        {
            GameCanvas.Children.Remove(dotEllipse);
            _dotEllipses.Remove(dotKey);
        }

        // Atualiza visual dos fantasmas
        foreach (var ghost in ViewModel.Ghosts)
            UpdateGhostVisual(ghost);

        // Trata resultado
        switch (result)
        {
            case TickResult.PacmanDied:
                _timer.Stop();
                ViewModel.LoseLife();

                if (ViewModel.IsGameOver())
                {
                    await ShowGameOverDialog();
                    Frame.GoBack();
                }
                else
                {
                    ViewModel.ResetPositions();
                    DrawPacman();
                    foreach (var ghost in ViewModel.Ghosts)
                        UpdateGhostVisual(ghost);
                    _timer.Start();
                }
                break;

            case TickResult.LevelCleared:
                _timer.Stop();
                await ShowWinDialog();
                Frame.GoBack();
                break;
        }
    }

    // ── Input de teclado ─────────────────────────────────────────────────────
    private void GamePage_KeyDown(object sender, KeyRoutedEventArgs e)
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
    }

    // ── Diálogos ─────────────────────────────────────────────────────────────
    private async System.Threading.Tasks.Task ShowWinDialog()
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

    private async System.Threading.Tasks.Task ShowGameOverDialog()
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

    private void GameCanvas_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case VirtualKey.Up:
            case VirtualKey.W:
                ViewModel?.SetDirection(0, -1);
                break;
            case VirtualKey.Down:
            case VirtualKey.S:
                ViewModel?.SetDirection(0, 1);
                break;
            case VirtualKey.Left:
            case VirtualKey.A:
                ViewModel?.SetDirection(-1, 0);
                break;
            case VirtualKey.Right:
            case VirtualKey.D:
                ViewModel?.SetDirection(1, 0);
                break;
            case VirtualKey.Escape:
                _timer.Stop();
                Frame.GoBack();
                break;
        }
        e.Handled = true;
    }
}

