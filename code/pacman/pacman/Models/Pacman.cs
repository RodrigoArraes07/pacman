namespace pacman.Models;

public class Pacman
{
    // Posição inicial padrão
    public const int StartCol = 13;
    public const int StartRow = 23;

    public int Col { get; private set; } = StartCol;
    public int Row { get; private set; } = StartRow;

    // Direção atual
    public int DirectionX { get; private set; } = 0;
    public int DirectionY { get; private set; } = 0;

    // Direção enfileirada (next input do jogador)
    private int _bufferedDirX = 0;
    private int _bufferedDirY = 0;

    /// <summary>
    /// Define a próxima direção desejada pelo jogador.
    /// Será aplicada no próximo tick se o caminho estiver livre.
    /// </summary>
    public void SetDirection(int dx, int dy)
    {
        _bufferedDirX = dx;
        _bufferedDirY = dy;
    }

    /// <summary>
    /// Tenta mover o Pac-Man na arena fornecida.
    /// Aplica a direção em buffer; se não for possível, mantém a atual.
    /// Retorna true se houve movimento.
    /// </summary>
    public bool Move(Arena arena)
    {
        // Tenta aplicar a direção em buffer primeiro (responsividade)
        if (_bufferedDirX != 0 || _bufferedDirY != 0)
        {
            int newCol = Col + _bufferedDirX;
            int newRow = Row + _bufferedDirY;

            if (arena.IsWalkable(newCol, newRow))
            {
                DirectionX = _bufferedDirX;
                DirectionY = _bufferedDirY;
                _bufferedDirX = 0;
                _bufferedDirY = 0;
            }
        }

        // Se não há direção ativa, fica parado
        if (DirectionX == 0 && DirectionY == 0)
            return false;

        int targetCol = Col + DirectionX;
        int targetRow = Row + DirectionY;

        if (!arena.IsWalkable(targetCol, targetRow))
            return false;

        Col = targetCol;
        Row = targetRow;
        return true;
    }

    /// <summary>
    /// Retorna o ângulo de rotação visual do Pac-Man com base na direção.
    /// </summary>
    public double GetRotationAngle()
    {
        if (DirectionX == 1)  return 0;    // direita
        if (DirectionX == -1) return 180;  // esquerda
        if (DirectionY == -1) return 270;  // cima
        if (DirectionY == 1)  return 90;   // baixo
        return 0;
    }

    /// <summary>
    /// Reposiciona o Pac-Man para o ponto de início padrao e zera a direção.
    /// </summary>
    public void Reset()
    {
        Col = StartCol;
        Row = StartRow;
        DirectionX = 0;
        DirectionY = 0;
        _bufferedDirX = 0;
        _bufferedDirY = 0;
    }
}
