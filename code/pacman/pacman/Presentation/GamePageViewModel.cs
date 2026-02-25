using System.Collections.ObjectModel;

namespace pacman.Presentation;

public partial class GamePageViewModel : ObservableObject
{
    [ObservableProperty] private int score = 0;
    [ObservableProperty] private int level = 1;
    [ObservableProperty] private ObservableCollection<int> lives = new ObservableCollection<int> { 1, 2, 3 };
    
    // Posicao do pacman exposta para view
    [ObservableProperty] private int pacmanX;
    [ObservableProperty] private int pacmanY;
    
    // Importacao dos Models
    public Arena Arena { get; }
    public Pacman Pacman { get; }
    public IReadOnlyList<Ghost> Ghosts { get; }
    
    // Exposicao do labirinto
    public char[,] Maze => Arena.Maze;
    
    // ──────────────────────── Estado de Susto ──────────────────────────────────────
    private int _frightenedTicksLeft = 0;
    private const int FrightenedDuration = 30; // ticks

    public IAudioService Audio { get; }
    private readonly IScoreService _scoreService;
    public bool IsFrightenedWarning => _frightenedTicksLeft is > 0 and <= 10;

    public GamePageViewModel(IAudioService audio, IScoreService scoreService)
    {
        Audio = audio;
        _scoreService = scoreService;
        Arena = new Arena();
        Pacman = new Pacman();
        Ghosts = new List<Ghost>
        {
            new Ghost(GhostPersonality.Blinky),
            new Ghost(GhostPersonality.Pinky),
            new Ghost(GhostPersonality.Inky),
            new Ghost(GhostPersonality.Clyde)
        };

        SyncPacmanPosition();
    }
    
    // ────────────────────── Input ─────────────────────────
    /// <summary>Repassa a direção desejada ao modelo do pacman.</summary>
    public void SetDirection(int dx, int dy) => Pacman.SetDirection(dx, dy);
    
    // ── Tick principal ───────────────────────────────────────────────────────
    /// <summary>
    /// Processa a logica de movimentacao do jogo: move Pac-Man, come pontos, move fantasmas,
    /// verifica colisões.
    /// </summary>

    public TickResult ProcessTick()
    {
        // 1) Mover Pac-Man
        bool moved = Pacman.Move(Arena);
        if (moved)
        {
            SyncPacmanPosition();

            // 2) Comer ponto
            int earned = Arena.EatDot(Pacman.Col, Pacman.Row);
            if (earned > 0)
            {
                Score += earned;
                Audio.PlayEatDot();

                // Power pellet: assustar fantasmas
                if (earned == 50)
                {
                    _frightenedTicksLeft = FrightenedDuration;
                    foreach (var g in Ghosts)
                        g.SetFrightened();
                    Audio.PlayFrightened();
                }
            }
        }

        // 3) Tick do estado frightened
        if (_frightenedTicksLeft > 0)
        {
            _frightenedTicksLeft--;
            if (_frightenedTicksLeft == 0)
            {
                foreach (var g in Ghosts)
                    if (g.State == GhostState.Frightened)
                        g.SetChasing();
                Audio.StopFrightened();
            }
        }

        // 4) Mover fantasmas
        foreach (var ghost in Ghosts)
            ghost.Move(Arena, Pacman.Col, Pacman.Row);

        // 5) Verificar colisão Pac-Man × Fantasmas
        foreach (var ghost in Ghosts)
        {
            if (!ghost.IsAt(Pacman.Col, Pacman.Row)) continue;

            if (ghost.State == GhostState.Frightened)
            {
                // Pac-Man comeu o fantasma
                Score += 200;
                ghost.SetReturning();
            }
            else if (ghost.State == GhostState.Chasing)
            {
                // Pac-Man morreu
                return TickResult.PacmanDied;
            }
        }

        // 6) Verificar vitória
        if (Arena.IsCleared())
            return TickResult.LevelCleared;

        return TickResult.Normal;
    }
    
    // ────────────────────── Fim de vida ────────────────────────────
    public void LoseLife()
    {
        if (Lives.Count > 0)
            Lives.RemoveAt(Lives.Count - 1);
    }
    
    public bool IsGameOver() => Lives.Count == 0;
    
    /// <summary>Reposiciona Pac-Man e fantasmas sem reiniciar o labirinto.</summary>
    public void ResetPositions()
    {
        Pacman.Reset();
        foreach (var g in Ghosts) g.Reset();
        _frightenedTicksLeft = 0;
        SyncPacmanPosition();
    }

    /// <summary>Reinicia o jogo completo (labirinto + posições + score).</summary>
    public void FullReset()
    {
        Arena.Reset();
        Pacman.Reset();
        foreach (var g in Ghosts) g.Reset();
        Score = 0;
        Level = 1;
        Lives = new ObservableCollection<int> { 1, 2, 3 };
        _frightenedTicksLeft = 0;
        SyncPacmanPosition();
    }
    
    // ──────────────────────── Score Persistence ───────────────────
    public async Task SaveScoreAsync(string playerName)
    {
        if (Score <= 0) return;

        var name = string.IsNullOrWhiteSpace(playerName) ? "???" : playerName.Trim().ToUpper();
        if (name.Length > 10) name = name[..10];

        await _scoreService.SaveScoreAsync(new ScoreEntry
        {
            PlayerName = name,
            Score = Score,
            Date = DateTime.Now
        });
    }

    // ──────────────────────── Helpers ─────────────────────────────
    private void SyncPacmanPosition()
    {
        PacmanX = Pacman.Col;
        PacmanY = Pacman.Row;
    }
}

/// <summary>Resultado de um tick do jogo.</summary>
public enum TickResult
{
    Normal,
    PacmanDied,
    LevelCleared
}
