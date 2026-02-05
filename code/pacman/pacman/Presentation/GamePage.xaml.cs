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
    public GamePageViewModel ViewModel { get;}
    private DispatcherTimer _timer;
    private Dictionary<string, Rectangle> wallRectangles = new Dictionary<string, Rectangle>();
    private Dictionary<string, Ellipse> dotEllipses = new Dictionary<string, Ellipse>();
    private Ellipse pacmanEllipse;
    private const int CELL_SIZE = 16;
    
    
    public GamePage()
    {
        this.InitializeComponent();
        ViewModel = new GamePageViewModel();
        this.DataContext = ViewModel;

        //Tempo do Jogo
        _timer = new DispatcherTimer();
        _timer.Interval = TimeSpan.FromSeconds(150);
        _timer.Tick += GameTimer_Tick;
        
        // Captura das teclas
        this.KeyDown += GamePage_KeyDown;
        this.Focus(FocusState.Programmatic);
    }
    
    private void GameCanvas_Loaded(object sender, RoutedEventArgs e)
    {
        DrawMaze();
        DrawPacman();
        _timer.Start();
    }
    
    private void DrawMaze()
    {
        var maze = ViewModel.Maze;
        
        for (int row = 0; row < maze.GetLength(0); row++)
        {
            for (int col = 0; col < maze.GetLength(1); col++)
            {
                char cell = maze[row, col];
                double x = col * CELL_SIZE;
                double y = row * CELL_SIZE;

                if (cell == '#')
                {
                    // Parede
                    var wall = new Rectangle
                    {
                        Width = CELL_SIZE,
                        Height = CELL_SIZE,
                        Fill = new SolidColorBrush(Colors.Blue),
                        Stroke = new SolidColorBrush(Microsoft.UI.ColorHelper.FromARGB(255, 61, 90, 254)),
                        StrokeThickness = 1
                    };
                    
                    Canvas.SetLeft(wall, x);
                    Canvas.SetTop(wall, y);
                    GameCanvas.Children.Add(wall);
                    
                    wallRectangles[$"{row},{col}"] = wall;
                }
                else if (cell == '.')
                {
                    // Ponto pequeno
                    var dot = new Ellipse
                    {
                        Width = 4,
                        Height = 4,
                        Fill = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 255, 227, 76))
                    };
                    Canvas.SetLeft(dot, x + CELL_SIZE / 2 - 2);
                    Canvas.SetTop(dot, y + CELL_SIZE / 2 - 2);
                    GameCanvas.Children.Add(dot);
                    
                    dotEllipses[$"{row},{col}"] = dot;
                }
                else if (cell == '*')
                {
                    // Power pellet (ponto grande)
                    var powerPellet = new Ellipse
                    {
                        Width = 8,
                        Height = 8,
                        Fill = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 255, 227, 76))
                    };
                    Canvas.SetLeft(powerPellet, x + CELL_SIZE / 2 - 4);
                    Canvas.SetTop(powerPellet, y + CELL_SIZE / 2 - 4);
                    GameCanvas.Children.Add(powerPellet);
                    
                    dotEllipses[$"{row},{col}"] = powerPellet;
                }
            }
        }
    }
    
    private void DrawPacman()
    {
        if (pacmanEllipse == null)
        {
            pacmanEllipse = new Ellipse
            {
                Width = CELL_SIZE - 2,
                Height = CELL_SIZE - 2,
                Fill = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 255, 227, 76))
            };
            GameCanvas.Children.Add(pacmanEllipse);
        }

        Canvas.SetLeft(pacmanEllipse, ViewModel.PacmanX * CELL_SIZE + 1);
        Canvas.SetTop(pacmanEllipse, ViewModel.PacmanY * CELL_SIZE + 1);
    }
    
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
                // Navegar de volta ao menu
                Frame.GoBack();
                break;
        }
    }
    
    private void GameTimer_Tick(object sender, object e)
    {
        bool moved = ViewModel.MovePacman();
            
        if (moved)
        {
            DrawPacman();
                
            // Verificar se comeu um ponto
            string key = $"{ViewModel.PacmanY},{ViewModel.PacmanX}";
            if (dotEllipses.ContainsKey(key))
            {
                GameCanvas.Children.Remove(dotEllipses[key]);
                dotEllipses.Remove(key);
                ViewModel.EatDot();
            }
        }

        // Verificar vitória
        if (ViewModel.CheckWin())
        {
            _timer.Stop();
            ShowWinMessage();
        }
    }
    
    private async void ShowWinMessage()
    {
        var dialog = new ContentDialog
        {
            Title = "VOCÊ VENCEU!",
            Content = $"Score: {ViewModel.Score}\n\nPressione OK para voltar ao menu",
            CloseButtonText = "OK",
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();
        Frame.GoBack();
    }
}

