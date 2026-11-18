# DownBlocks

Jogo 2D de blocos que caem, feito na Unity 6.6 (URP 2D) para o trabalho individual de Desenvolvimento de Jogos Digitais.

Encaixe as peças de 4 blocos e complete linhas horizontais para pontuar. A cada 10 linhas o nível sobe e as peças caem mais rápido. O jogo acaba quando a pilha chega ao topo.

## Como abrir e jogar

1. Abra a pasta do projeto no Unity Hub (versão **6000.6.0f1**).
2. Abra a cena `Assets/Scenes/SampleScene`.
3. Aperte **Play**. O `GameManager` monta a cena inteira por código (tabuleiro, câmera, interface e sons).

## Controles

| Tecla | Ação |
|---|---|
| Setas esquerda/direita ou A / D | mover |
| Seta para cima, W ou X | girar (horário) |
| Z ou Q | girar (anti-horário) |
| Seta para baixo ou S | descer rápido (+1 ponto por célula) |
| Espaço | queda instantânea (+2 pontos por célula) |
| P ou Esc | pausar |
| R | reiniciar |
| Enter | começar / jogar de novo |

## Pontuação

| Linhas de uma vez | Pontos |
|---|---|
| 1 | 100 × nível |
| 2 | 300 × nível |
| 3 | 500 × nível |
| 4 | 800 × nível |

O recorde fica salvo (PlayerPrefs).

## Conceitos aplicados

| Conceito | Onde |
|---|---|
| Entrada do jogador (Input System) | `GameManager.HandlePlaying` – mover, girar, descer, pausar, com repetição ao segurar a tecla |
| Movimento por Transform | `Piece.GridPos` / `TryMove` – a peça anda alterando `transform.position` na grade |
| Colisões | `Piece.Fits` + `Board.IsFree` – checagem com paredes, chão e blocos fixados; "wall kick" na rotação |
| Pontuação, níveis, derrota e recorde | `GameManager.ClearLinesRoutine`, `EndGame` |
| Estados do jogo | `GameState` (Título, Jogando, Limpando Linhas, Pausado, Fim de Jogo) |
| Interface e feedback | `UIManager` (HUD, próxima peça, telas), peça fantasma, linhas piscando, tremida de câmera, textos "DUPLA! +300" e sons gerados por código (`SoundFx`) |

## Estrutura dos scripts (`Assets/Scripts`)

- `GameManager.cs`: regras, estados, entrada e loop do jogo
- `Board.cs`: grade 10×20, fixação de blocos e remoção de linhas
- `Piece.cs`: peça ativa (movimento, rotação, colisão)
- `BlockShapes.cs`: formatos e cores das 7 peças
- `UIManager.cs`: interface com uGUI
- `SoundFx.cs`: efeitos sonoros sintetizados
- `SpriteFactory.cs`: sprites gerados por código
- `GameBootstrap.cs`: cria o GameManager automaticamente se a cena não tiver um
