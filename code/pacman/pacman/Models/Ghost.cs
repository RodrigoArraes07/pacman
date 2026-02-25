namespace pacman.Models;

public enum GhostState
{
    Chasing,    // Perseguindo o Pac-Man
    Frightened, // Vulnerável após power pellet
    Returning   // Voltando para a casa após ser comido
}

public enum GhostPersonality
{
    Blinky, // Vermelho  – persegue diretamente
    Pinky,  // Rosa      – tenta ficar à frente
    Inky,   // Ciano     – comportamento híbrido
    Clyde   // Laranja   – alterna entre perseguir e dispersar
}

public class Ghost
{
    // ───────────────────────── Posição e movimento ────────────────────────────
    public int Col { get; private set; }
    public int Row { get; private set; }
    public int DirectionX { get; private set; } = 0;
    public int DirectionY { get; private set; } = -1;

    // ───────────────────────────── Identidade ──────────────────────────────────
    public GhostPersonality Personality { get; }
    public GhostState State { get; private set; } = GhostState.Chasing;

    // Cor base de cada fantasma
    public (byte R, byte G, byte B) BaseColor => Personality switch
    {
        GhostPersonality.Blinky => (255, 0,   0),
        GhostPersonality.Pinky  => (255, 184, 255),
        GhostPersonality.Inky   => (0,   255, 255),
        GhostPersonality.Clyde  => (255, 184, 81),
        _                       => (255, 255, 255)
    };

    // ─────────────────── Posições iniciais por personalidade ───────────────────
    private static (int Col, int Row) StartPosition(GhostPersonality p) => p switch
    {
        GhostPersonality.Blinky => (13, 11),
        GhostPersonality.Pinky  => (13, 14),
        GhostPersonality.Inky   => (11, 14),
        GhostPersonality.Clyde  => (15, 14),
        _                       => (13, 14)
    };

    private readonly int _startCol;
    private readonly int _startRow;

    private readonly Random _rng = new();

    // ───────────────────────────── Construtor ────────────────────────────────
    public Ghost(GhostPersonality personality)
    {
        Personality = personality;
        (_startCol, _startRow) = StartPosition(personality);
        Col = _startCol;
        Row = _startRow;
    }

    // ──────────────────────────── Estado ──────────────────────────────────────
    public void SetFrightened() => State = GhostState.Frightened;
    public void SetReturning()  => State = GhostState.Returning;
    public void SetChasing()    => State = GhostState.Chasing;

    // ────────────────────────── Movimento ──────────────────────────────────────
    /// <summary>
    /// Move o fantasma de acordo com sua personalidade e o estado atual.
    /// Deve ser chamado a cada tick do jogo.
    /// </summary>
    public void Move(Arena arena, int pacmanCol, int pacmanRow)
    {
        (int targetCol, int targetRow) = State switch
        {
            GhostState.Returning  => (_startCol, _startRow),
            GhostState.Frightened => GetRandomTarget(arena),
            _                     => GetChaseTarget(pacmanCol, pacmanRow)
        };

        TryMoveToward(arena, targetCol, targetRow);

        // Ao retornar à base, volta a perseguir
        if (State == GhostState.Returning && Col == _startCol && Row == _startRow)
            State = GhostState.Chasing;
    }

    // ───────────────────────────── Targets ────────────────────────────────────
    private (int Col, int Row) GetChaseTarget(int pacmanCol, int pacmanRow)
    {
        return Personality switch
        {
            // Blinky: vai direto ao Pac-Man
            GhostPersonality.Blinky => (pacmanCol, pacmanRow),

            // Pinky: tenta 4 células à frente do Pac-Man
            GhostPersonality.Pinky  => (pacmanCol + DirectionX * 4,
                                        pacmanRow + DirectionY * 4),

            // Inky: combinação entre posição atual e Pac-Man
            GhostPersonality.Inky   => ((Col + pacmanCol) / 2,
                                        (Row + pacmanRow) / 2),

            // Clyde: persegue quando longe, dispersa quando perto
            GhostPersonality.Clyde  => ManhattanDistance(Col, Row, pacmanCol, pacmanRow) > 8
                                        ? (pacmanCol, pacmanRow)
                                        : (1, 29), // canto de dispersão
            _                       => (pacmanCol, pacmanRow)
        };
    }

    private (int Col, int Row) GetRandomTarget(Arena arena)
    {
        // Escolhe um vizinho aleatório navegável
        (int dx, int dy)[] dirs = { (1,0),(-1,0),(0,1),(0,-1) };
        var options = new System.Collections.Generic.List<(int, int)>();

        foreach (var (dx, dy) in dirs)
        {
            int nc = Col + dx;
            int nr = Row + dy;
            if (arena.IsWalkableForGhost(nc, nr))
                options.Add((nc, nr));
        }

        if (options.Count == 0) return (Col, Row);
        var pick = options[_rng.Next(options.Count)];
        return pick;
    }

    // ────────────────────── Navegação por BFS simplificada ────────────────────
    /// <summary>
    /// Tenta dar um passo em direção ao alvo evitando andar para trás (anti-loop simples).
    /// </summary>
    private void TryMoveToward(Arena arena, int targetCol, int targetRow)
    {
        (int dx, int dy)[] dirs = { (1,0),(-1,0),(0,1),(0,-1) };

        int bestDist = int.MaxValue;
        int bestDx = DirectionX;
        int bestDy = DirectionY;
        bool found = false;

        foreach (var (dx, dy) in dirs)
        {
            // Não voltar na direção oposta (a menos que seja a única opção)
            if (dx == -DirectionX && dy == -DirectionY) continue;

            int nc = Col + dx;
            int nr = Row + dy;

            bool walkable = arena.IsWalkableForGhost(nc, nr);

            if (!walkable) continue;

            int dist = ManhattanDistance(nc, nr, targetCol, targetRow);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestDx = dx;
                bestDy = dy;
                found = true;
            }
        }

        // Se não achou nenhum (beco sem saída), permite reverter
        if (!found)
        {
            foreach (var (dx, dy) in dirs)
            {
                int nc = Col + dx;
                int nr = Row + dy;
                bool walkable = arena.IsWalkableForGhost(nc, nr);
                if (!walkable) continue;
                int dist = ManhattanDistance(nc, nr, targetCol, targetRow);
                if (dist < bestDist) { bestDist = dist; bestDx = dx; bestDy = dy; }
            }
        }

        int nextCol = Col + bestDx;
        int nextRow = Row + bestDy;

        bool canMove = arena.IsWalkableForGhost(nextCol, nextRow);

        if (canMove)
        {
            Col = nextCol;
            Row = nextRow;
            DirectionX = bestDx;
            DirectionY = bestDy;
        }
    }

    // ─────────────────────────────── Utilitário ────────────────────────────────
    public bool IsAt(int col, int row) => Col == col && Row == row;

    public void Reset()
    {
        Col = _startCol;
        Row = _startRow;
        DirectionX = 0;
        DirectionY = -1;
        State = GhostState.Chasing;
    }

    private static int ManhattanDistance(int c1, int r1, int c2, int r2)
        => Math.Abs(c1 - c2) + Math.Abs(r1 - r2);
}
