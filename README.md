# PACMAN

`GAME PACMAN` | `START - JANUARY 29 2026`

Implementação do clássico jogo Pac-Man desenvolvida com C# e Uno Platform.

This is a Jala University project to create a pacman game.

## Integrantes

* Amaury Magno
* Rodrigo Arraes

---

## Requisitos do Projeto

### 1. Tela Inicial e Menu
- Janela com título "PACMAN" e botões para iniciar jogo, ver quadro de pontuação e sair
- Quadro de pontuação funcionando e salvando em arquivo (`.json`)
- Botão para mutar/tocar música

### 2. Movimento do Pac-Man
- Movimentação pelas teclas de seta
- Tecla mantém o movimento contínuo (sem necessidade de segurar)
- Colisão com paredes
- Sons: comer pontos, movimentação, UI, vitória e derrota

### 3. Tabuleiro
- Mapa estático com Dots e Pills (4 power pellets)
  - Dot concede pontos
  - Pill permite consumir fantasmas por tempo limitado, com indicação visual do início e fim do efeito
- Pontuação visível na tela, salva ao fim da partida

### 4. Fantasmas
- 4 fantasmas surgem no centro do mapa
- Movimento aleatório ou em perseguição ao Pac-Man
- Fantasma comido durante o efeito da Pill retorna ao centro para se resetar
- Contato com fantasma sem efeito ativo subtrai uma vida (máximo 3 vidas)

### 5. Fim de Jogo
- **Game Over** ao perder todas as vidas — pop-up com opção de reiniciar
- **Vitória** ao consumir todos os Dots

---

## Tecnologias

- **.NET 9.0** — runtime
- **Uno Platform 6.4.58** — framework cross-platform (WinUI 3 / XAML)
- **MVVM Community Toolkit** — padrão de apresentação
- **Microsoft.Extensions.DependencyInjection** — injeção de dependência
- **System.Text.Json** — persistência de pontuações

## Estrutura do Projeto

```
pacman/
├── Models/          # Lógica do jogo (Arena, Pacman, Ghost, ScoreEntry)
├── Services/        # Serviços de negócio (ScoreService, AudioService)
├── Presentation/    # Camada MVVM (Pages + ViewModels)
├── Assets/
│   ├── Sprites/     # Imagens do Pac-Man e fantasmas
│   └── Sounds/      # Efeitos sonoros
└── Platforms/       # Ponto de entrada por plataforma
```

## Arquitetura

O projeto segue o padrão **Models → Services → Presentation (MVVM)**:

- **Models** — encapsulam a lógica pura do jogo, sem dependência de UI
- **Services** — gerenciam persistência de pontuações e reprodução de áudio
- **ViewModels** — expõem estado observável e comandos para as Views
- **Pages (Views)** — renderizam o jogo em canvas e gerenciam input do teclado

## Funcionamento do Jogo

### Loop Principal (150ms por tick)

1. **Mover Pac-Man** — aplica direção com buffer de input para controles responsivos
2. **Comer pontos** — detecta ponto ou power pellet na posição atual
3. **Estado Frightened** — power pellet ativa por 30 ticks; fantasmas ficam vulneráveis
4. **Mover fantasmas** — cada fantasma age conforme seu estado e personalidade
5. **Detectar colisão** — Pac-Man come fantasma assustado ou perde vida
6. **Verificar vitória** — fase concluída ao comer todos os pontos

### Pontuação

| Evento | Pontos |
|--------|--------|
| Ponto pequeno | 10 |
| Power pellet | 50 |
| Fantasma comido | 200 |

### Vidas

- Início com **3 vidas**
- Perda de vida ao tocar fantasma perseguindo
- **Game Over** ao zerar vidas
- Posições reiniciam ao perder vida; labirinto e pontuação são preservados

### IA dos Fantasmas

Cada fantasma possui uma personalidade com comportamento distinto:

| Fantasma | Cor | Comportamento |
|----------|-----|---------------|
| **Blinky** | Vermelho | Perseguição direta ao Pac-Man |
| **Pinky** | Rosa | Antecipa 4 células à frente do Pac-Man |
| **Inky** | Ciano | Combinação entre posição atual e Pac-Man |
| **Clyde** | Laranja | Persegue quando longe, dispersa quando perto |

Estados dos fantasmas: **Chasing → Frightened → Returning** (para casa após ser comido).

### Navegação

- **Menu Principal** → Iniciar jogo / Ver recordes / Som / Sair
- **Gameplay** → HUD com pontuação, nível e vidas
- **Fim de jogo / Vitória** → Salva pontuação com nome do jogador
- **Recordes** → Top 10, com destaque para os 3 primeiros

## Controles

| Tecla | Ação |
|-------|------|
| `↑ ↓ ← →` | Mover Pac-Man |
| `ESC` | Voltar ao menu |

## Como Executar

Requisitos: .NET 9.0 SDK instalado.

```bash
# Restaurar dependências
dotnet restore

# Executar o projeto
dotnet run --project code/pacman/pacman/pacman.csproj
```

## Persistência

Pontuações salvas em JSON no diretório de dados da aplicação:
`ApplicationData/LocalFolder/scores.json`

## Áudio

Efeitos sonoros para: início de jogo, comer ponto, perder vida, game over, vitória, power pellet e cliques de UI.

- **Windows** — Windows Media Player (COM)
- **Linux** — FFplay / MPG123
