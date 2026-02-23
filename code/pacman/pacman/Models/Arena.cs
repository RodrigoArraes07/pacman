namespace pacman.Models;

/// <summary>
/// Classe pertencente a Arena para geracao dos labirintos
/// e logica dos pontos comiveis
/// </summary>

public class Arena
{
    public char[,] Maze { get; private set; }
    public int Rows { get; private set; }
    public int Cols { get; private set; }
    public int TotalDots { get; private set; }
    public int DotsEaten { get; private set; }

    private static readonly string[] MazeLayout = new string[]
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

    public Arena()
    {
        InitializeMaze();
        CountDots();
    }

    private void InitializeMaze()
    {
        Rows = MazeLayout.Length;
        Cols = MazeLayout[0].Length;
        Maze = new char[Rows, Cols];

        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < MazeLayout[row].Length; col++)
            {
                Maze[row, col] = MazeLayout[row][col];
            }
        }
    }

    private void CountDots()
    {
        TotalDots = 0;
        for (int row = 0; row < Rows; row++)
        {
            for (int col = 0; col < Cols; col++)
            {
                if (Maze[row, col] == '.' || Maze[row, col] == '*')
                    TotalDots++;
            }
        }
    }

    /// <summary>
    /// Verifica se uma célula é navegável (não é parede nem porta de fantasma).
    /// </summary>
    public bool IsWalkable(int col, int row)
    {
        if (row < 0 || row >= Rows || col < 0 || col >= Cols)
            return false;

        char cell = Maze[row, col];
        return cell != '#' && cell != '-';
    }

    /// <summary>
    /// Verifica se uma célula é navegável apenas por fantasmas (inclui a porta '-').
    /// </summary>
    public bool IsWalkableForGhost(int col, int row)
    {
        if (row < 0 || row >= Rows || col < 0 || col >= Cols)
            return false;

        return Maze[row, col] != '#';
    }

    /// <summary>
    /// Tenta comer o ponto na posição indicada. Retorna a pontuação obtida (0 se não havia ponto).
    /// </summary>
    public int EatDot(int col, int row)
    {
        char cell = Maze[row, col];

        if (cell == '.')
        {
            Maze[row, col] = ' ';
            DotsEaten++;
            return 10;
        }
        else if (cell == '*')
        {
            Maze[row, col] = ' ';
            DotsEaten++;
            return 50;
        }

        return 0;
    }

    public bool IsCleared() => DotsEaten >= TotalDots;

    /// <summary>
    /// Reinicia o labirinto para o estado inicial.
    /// </summary>
    public void Reset()
    {
        DotsEaten = 0;
        InitializeMaze();
    }
}
