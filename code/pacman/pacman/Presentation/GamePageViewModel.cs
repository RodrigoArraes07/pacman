using System.Collections.ObjectModel;

namespace pacman.Presentation;

public partial class GamePageViewModel : ObservableObject
{
    [ObservableProperty] private int score = 0;
    [ObservableProperty] private int level = 1;
    [ObservableProperty] private ObservableCollection<int> lives = new ObservableCollection<int> { 1, 2, 3 };
    [ObservableProperty] private int pacmanX = 13;
    [ObservableProperty] private int pacmanY = 23;

    private int directionX = 0;
    private int directionY = 0;
    private int totalDots = 0;
    private int dotsEaten = 0;
    
    public char[,] Maze { get; private set; }

    public GamePageViewModel()
    {
        InitializeMaze();
        CountDots();
    }
    
    private void InitializeMaze()
    {
        string[] mazeLayout = new string[]
        {
            "############################",
            "#............##............#",
            "#.####.#####.##.#####.####.#",
            "#*####.#####.##.#####.####*#",
            "#.####.#####.##.#####.####.#",
            "#..........................#",
            "#.####.##.########.##.####.#",
            "#.####.##.########.##.####.#",
            "#......##....##....##......#",
            "######.##### ## #####.######",
            "     #.##### ## #####.#     ",
            "     #.##          ##.#     ",
            "     #.## ###--### ##.#     ",
            "######.## #      # ##.######",
            "      .   #      #   .      ",
            "######.## #      # ##.######",
            "     #.## ######## ##.#     ",
            "     #.##          ##.#     ",
            "     #.## ######## ##.#     ",
            "######.## ######## ##.######",
            "#............##............#",
            "#.####.#####.##.#####.####.#",
            "#.####.#####.##.#####.####.#",
            "#*..##................##..*#",
            "###.##.##.########.##.##.###",
            "###.##.##.########.##.##.###",
            "#......##....##....##......#",
            "#.##########.##.##########.#",
            "#.##########.##.##########.#",
            "#..........................#",
            "############################"
        };

        Maze = new char[mazeLayout.Length, mazeLayout[0].Length];

        for (int row = 0; row < mazeLayout.Length; row++)
        {
            for (int col = 0; col < mazeLayout[row].Length; col++)
            {
                Maze[row, col] = mazeLayout[row][col];
            }
        }
    }
    
    private void CountDots()
    {
        totalDots = 0;
        for (int row = 0; row < Maze.GetLength(0); row++)
        {
            for (int col = 0; col < Maze.GetLength(1); col++)
            {
                if (Maze[row, col] == '.' || Maze[row, col] == '*')
                {
                    totalDots++;
                }
            }
        }
    }

    public void SetDirection(int dx, int dy)
    {
        directionX = dx;
        directionY = dy;
    }
    
    public bool MovePacman()
    {
        if (directionX == 0 && directionY == 0)
            return false;
        
        int newX = PacmanX + directionX;
        int newY = PacmanY + directionY;
        
        //TRATAMENTO DAS EXCECOES E LIMITACOES DE MOVIMENTO DO PACMAN
        // Verificar limites na array
        if (newY < 0 || newY >= Maze.GetLength(0) || 
            newX < 0 || newX >= Maze.GetLength(1))
            return false;

        // Verificar colisão
        char cell = Maze[newY, newX];
        if (cell == '#' || cell == '-')
            return false;

        // Mover Pacman
        PacmanX = newX;
        PacmanY = newY;
        return true;
    }
    
    public void EatDot()
    {
        char cell = Maze[PacmanY, PacmanX];
            
        if (cell == '.')
        {
            Score += 10;
            dotsEaten++;
            Maze[PacmanY, PacmanX] = ' ';
        }
        else if (cell == '*')
        {
            Score += 50;
            dotsEaten++;
            Maze[PacmanY, PacmanX] = ' ';
        }
    }
    
    public bool CheckWin()
    {
        return dotsEaten >= totalDots;
    }
    
    public void LoseLife()
    {
        if (Lives.Count > 0)
        {
            Lives.RemoveAt(Lives.Count - 1);
        }
    }

    public void ResetPosition()
    {
        PacmanX = 13;
        PacmanY = 23;
        directionX = 0;
        directionY = 0;
    }
}
