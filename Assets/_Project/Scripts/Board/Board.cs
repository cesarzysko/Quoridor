using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public sealed class Board : MonoBehaviour
{
    /*** [ Events ] *********************************/
    public static event Action OnPlayerActionMade;
    public static event Action<float> OnOpponentActionMade;
    public static event Action<bool> OnGameEnd; // True if player has won, otherwise AI won.
    
    /*** [ Constants ] ******************************/
    public const float BOARD_BOUNDARY = TILE_POS_OFFSET * InteropConstants.TILES_ROW_SIZE / 2;

    private static readonly Color TILE_COLOR_DEFAULT = Color.white;
    private static readonly Color TILE_COLOR_REMOVE = Color.red;
    private static readonly Color TILE_COLOR_START = new Color(0.369f, 1f, 0.569f);
    private static readonly Color TILE_COLOR_END = new Color(0, 1f, 0.318f);
    private const float TILES_ANIMATION_RATE = 0.9f;
    private const float TILES_SCALE_MIN = 0.9f;
    private const float TILES_SCALE_MAX = 1.1f;
    private const float TILE_POS_OFFSET = 0.2f;
    private const int DIRECTIONS_COUNT = 4;
    private static readonly GridPosition[] NEIGHBOUR_DIRECTIONS = new GridPosition[DIRECTIONS_COUNT]
    {
        new GridPosition { x = 1, y = 0 },
        new GridPosition { x = -1, y = 0 },
        new GridPosition { x = 0, y = 1 },
        new GridPosition { x = 0, y = -1 }
    };
    
    /*** [ Serialized fields ] ***********************/
    [Header("Assets")] 
    [SerializeField] private Tile m_tilePrefab;
    [SerializeField] private Wall m_wallPrefab;
    [Header("References")]
    [SerializeField] private WaitingTimeDisplay m_waitingTimeDisplay;
    [SerializeField] private RemainingWallsDisplay m_wallDisplayP1;
    [SerializeField] private RemainingWallsDisplay m_wallDisplayP2;

    /*** [ Private fields ] **************************/
    private Tile[] m_tiles = Array.Empty<Tile>();
    private Wall[] m_walls = Array.Empty<Wall>();
    private BoardState m_boardState = BoardState.AddTiles;
    private float m_animationProgress;
    private int m_playerPosition;
    private int m_opponentPosition;
    private NativeQuoridorBridge m_bridge;
    private bool m_isPlayerMovement;
    private bool m_isWallPlacementVertical;
    private Wall m_currentWallHover;
    private GameAction[] m_playerActions;
    
    /*** [ Public methods ] **************************/
    public void PerformOpponentTurn()
    {
        Task task = PerformOpponentTurnAsync();
    }

    private async Task PerformOpponentTurnAsync()
    {
        Stopwatch sw = Stopwatch.StartNew();
        GameAction result = await Task.Run(() => m_bridge.ComputePcAction());
        sw.Stop();
        m_waitingTimeDisplay.UpdateTime((float)sw.Elapsed.TotalSeconds);

        switch (result.actionType)
        {
            case ActionType.Movement:
                m_opponentPosition = result.to;
                m_bridge.UpdateOpponentPosition(m_opponentPosition);
                if (m_tiles.IndexesOf(t => t.IsOpponentGoal()).Contains(m_opponentPosition))
                {
                    m_playerActions = null;
                    RefreshTiles();
                    OnGameEnd?.Invoke(false);
                    return;
                }
                break;
            case ActionType.WallVertical:
                m_walls[result.to].SetState(WallState.Vertical);
                m_bridge.PlaceWall(false, result.to, true);
                m_wallDisplayP2.UpdateCount(m_bridge.GetOpponentWallCount());
                break;
            case ActionType.WallHorizontal:
                m_walls[result.to].SetState(WallState.Horizontal);
                m_bridge.PlaceWall(false, result.to, false);
                m_wallDisplayP2.UpdateCount(m_bridge.GetOpponentWallCount());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        m_playerActions = m_bridge.ComputePlayerActions();
        RefreshTiles();
        OnOpponentActionMade?.Invoke((float)sw.Elapsed.TotalMilliseconds);
    }
    
    public BoardSaveData GetBoardSaveData()
    {
        return new BoardSaveData(m_tiles);
    }

    public void LoadBoardSaveData(BoardSaveData boardSaveData)
    {
        DestroyExistingBoard();
        CreateEmptyBoard(false);
        
        if (boardSaveData.map != null)
        {
            for (int i = 0; i < InteropConstants.TOTAL_TILES_COUNT; ++i)
            {
                if (boardSaveData.map[i])
                {
                    HandleAddTile(m_tiles[i], true);
                }
            }
        }

        if (boardSaveData.playerSpawn != -1 && m_tiles[boardSaveData.playerSpawn].IsActive())
        {
            HandleSetPlayerSpawn(m_tiles[boardSaveData.playerSpawn]);
        }

        if (boardSaveData.opponentSpawn != -1 && m_tiles[boardSaveData.opponentSpawn].IsActive())
        {
            HandleSetOpponentSpawn(m_tiles[boardSaveData.opponentSpawn]);
        }

        boardSaveData.playerGoals?.ForEach(g => {
            if (m_tiles[g].IsActive()) 
            {
                HandleSetPlayerGoal(m_tiles[g]);
            }
        });
        
        boardSaveData.opponentGoals?.ForEach(g => {
            if (m_tiles[g].IsActive())
            {
                HandleSetOpponentGoal(m_tiles[g]);
            }
        });
        
        RefreshTiles();
    }

    public void SwitchPlayMode() // On button press in-game.
    {
        m_isPlayerMovement = !m_isPlayerMovement;
        
        SetWallsActive(!m_isPlayerMovement);
        RefreshTiles();
    }

    public bool IsPlayerMovement()
    {
        return m_isPlayerMovement;
    }

    public void SwitchWallRotation() // On button press in-game / on R-key press.
    {
        m_isWallPlacementVertical = !m_isWallPlacementVertical;
        if (m_currentWallHover)
        {
            m_currentWallHover.SetTemporaryState(m_isWallPlacementVertical ? WallState.Vertical : WallState.Horizontal);
        }
    }

    public bool IsWallPlacementVertical()
    {
        return m_isWallPlacementVertical;
    }
    
    public void EnterPlayMode(int wallCount, Algorithm algorithm, int depth) 
    {
        m_waitingTimeDisplay.UpdateTime(0);
        m_wallDisplayP1.UpdateCount(wallCount);
        m_wallDisplayP2.UpdateCount(wallCount);
        
        SwitchStatePlay();
        SetWallsActive(false);
        
        m_isPlayerMovement = true;
        m_bridge = new NativeQuoridorBridge(m_tiles, wallCount, algorithm, depth);
        m_playerActions = m_bridge.ComputePlayerActions();

        m_playerPosition = m_tiles.IndexOf(t => t.IsPlayerSpawn());
        m_tiles[m_playerPosition].SetState(TileState.PlayerSpawn, false);
        m_tiles[m_playerPosition].SetPawn(PawnType.Player);
        m_tiles[m_playerPosition].SetPawnActive(true);
        
        m_opponentPosition = m_tiles.IndexOf(t => t.IsOpponentSpawn());
        m_tiles[m_opponentPosition].SetState(TileState.OpponentSpawn, false);
        m_tiles[m_opponentPosition].SetPawn(PawnType.Opponent);
        m_tiles[m_opponentPosition].SetPawnActive(true);
        
        RefreshTiles();
    }

    public void EnterEditMode()
    {
        SwitchStateAddTiles();
        DestroyExistingBoard();
        CreateEmptyBoard();
        RefreshTiles();
        SetWallsActive(false);
    }

    public void SwitchStatePlay()
        => SwitchState(BoardState.Play);

    public void SwitchStateAddTiles() 
        => SwitchState(BoardState.AddTiles);
    
    public void SwitchStateRemoveTiles() 
        => SwitchState(BoardState.RemoveTiles);
    
    public void SwitchStateSetPlayerSpawn() 
        => SwitchState(BoardState.SetPlayerSpawn);
    
    public void SwitchStateSetPlayerGoal() 
        => SwitchState(BoardState.SetPlayerGoal);
    
    public void SwitchStateSetOpponentSpawn() 
        => SwitchState(BoardState.SetOpponentSpawn);
    
    public void SwitchStateSetOpponentGoal() 
        => SwitchState(BoardState.SetOpponentGoal);

    /*** [ Private methods ] *************************/
    private void Awake()
    {
        ConfigureEvents();
    }

    private void SetWallsActive(bool active)
    {
        foreach (Wall wall in m_walls)
        {
            wall.SetColliderActive(active);
        }
    }
    
    private void Update()
    {
        if (IsState(BoardState.AddTiles))
        {
            HandleAddTilesUpdate();
        }
    }

    private void SwitchState(BoardState state)
    {
        if (m_boardState == state) { return; }

        SetState(state);
        RefreshTiles();
    }

    private void ConfigureEvents()
    {
        Tile.OnTileClicked += OnTileClicked;
        Wall.OnHoverStart += OnWallHoverStart;
        Wall.OnHoverEnd += OnWallHoverEnd;
        Wall.OnClicked += OnWallClicked;
    }

    private bool IsState(BoardState state)
    {
        return m_boardState == state;
    }

    private void SetState(BoardState state)
    {
        m_boardState = state;
    }

    private void HandleAddTilesUpdate()
    {
        Tile[] placeableTiles = m_tiles.Filter(t => t.IsPlaceable());
        foreach (Tile tile in placeableTiles)
        {
            float animationProgress = GetTilesAnimationProgress();
            tile.SetScale(Mathf.Lerp(TILES_SCALE_MIN, TILES_SCALE_MAX, animationProgress));
            tile.SetColor(Color.Lerp(TILE_COLOR_START, TILE_COLOR_END, animationProgress));
        }

        m_animationProgress += Time.deltaTime * TILES_ANIMATION_RATE;
        m_animationProgress %= 1f;
    }

    private float GetTilesAnimationProgress()
    {
        float offset = -Mathf.PI / 2;
        return (Mathf.Sin(2 * Mathf.PI * m_animationProgress + offset) + 1) / 2;
    }

    /**
     * Destroys all the board tiles and empties the tiles array.
     */
    public void DestroyExistingBoard()
    {
        int length = m_tiles.Length;
        for (int i = 0; i < length; ++i)
        {
            Destroy(m_tiles[i].gameObject);
        }

        length = m_walls.Length;
        for (int i = 0; i < length; ++i)
        {
            Destroy(m_walls[i].gameObject);
        }

        m_tiles = Array.Empty<Tile>();
        m_walls = Array.Empty<Wall>();
    }

    /**
     * Creates a default empty board from scratch.
     */
    private void CreateEmptyBoard(bool addMiddle = true)
    {
        m_tiles = new Tile[InteropConstants.TOTAL_TILES_COUNT];
        int tileIndex = 0;
        for (int y = 0; y < InteropConstants.TILES_ROW_SIZE; ++y)
        {
            const float TILE_START_OFFSET = -TILE_POS_OFFSET * InteropConstants.TILES_ROW_SIZE / 2f;
            float yPos = TILE_START_OFFSET + y * TILE_POS_OFFSET;
            for (int x = 0; x < InteropConstants.TILES_ROW_SIZE; ++x)
            {
                float xPos = TILE_START_OFFSET + x * TILE_POS_OFFSET;
                Vector3 position = new Vector3(xPos, -yPos, 0);
                Tile tile = Instantiate(m_tilePrefab, position, Quaternion.identity, transform);
                tile.name = $"Tile {tileIndex}";
                m_tiles[tileIndex++] = tile;
            }
        }

        m_walls = new Wall[InteropConstants.TOTAL_WALLS_COUNT];
        int wallIndex = 0;
        for (int y = 0; y < InteropConstants.WALLS_ROW_SIZE; ++y)
        {
            const float WALL_START_OFFSET = -TILE_POS_OFFSET * (InteropConstants.TILES_ROW_SIZE - 1) / 2f;
            float yPos = WALL_START_OFFSET + y * TILE_POS_OFFSET;
            for (int x = 0; x < InteropConstants.WALLS_ROW_SIZE; ++x)
            {
                float xPos = WALL_START_OFFSET + x * TILE_POS_OFFSET;
                Vector3 position = new Vector3(xPos, -yPos, 0);
                Wall wall = Instantiate(m_wallPrefab, position, Quaternion.identity, transform);
                m_walls[wallIndex++] = wall;
            }
        }

        if (!addMiddle) { return; }

        const int MID = InteropConstants.TILES_ROW_SIZE / 2;
        const int CENTER_TILE_INDEX = MID * InteropConstants.TILES_ROW_SIZE + MID; 
        m_tiles[CENTER_TILE_INDEX].SetState(TileState.Active, true);
        m_tiles[CENTER_TILE_INDEX].SetAlpha(1);

        int[] neighbouringTiles = new int[DIRECTIONS_COUNT]
        {
            CENTER_TILE_INDEX + 1,
            CENTER_TILE_INDEX - 1,
            CENTER_TILE_INDEX - InteropConstants.TILES_ROW_SIZE,
            CENTER_TILE_INDEX + InteropConstants.TILES_ROW_SIZE
        };

        for (int i = 0; i < DIRECTIONS_COUNT; ++i)
        {
            int neighbourIndex = neighbouringTiles[i];
            m_tiles[neighbourIndex].SetState(TileState.Placeable, true);
            m_tiles[neighbourIndex].SetAlpha(1);
        }
    }
    
    /**
     * Writes all valid neighbours of tile with index tileIndex into the neighboursBuffer array.
     */
    private int GetTileNeighbours(int tileIndex, int[] neighboursBuffer)
    {
        Assertions.EnsureTrue(
            tileIndex is >= 0 and < InteropConstants.TOTAL_TILES_COUNT, 
            $"{nameof(Board)}::{nameof(GetTileNeighbours)} --> Integer \"{nameof(tileIndex)}\" was out of bounds.");
        Assertions.EnsureTrue(
            neighboursBuffer.Length == DIRECTIONS_COUNT,
            $"{nameof(Board)}::{nameof(GetTileNeighbours)} --> Array \"{nameof(neighboursBuffer)}\" wasn't of length {DIRECTIONS_COUNT}");
        
        GridPosition tilePosition = new GridPosition {
            x = tileIndex % InteropConstants.TILES_ROW_SIZE,
            y = tileIndex / InteropConstants.TILES_ROW_SIZE
        };

        int neighboursCount = 0;
        for (int i = 0; i < DIRECTIONS_COUNT; ++i)
        {
            GridPosition neighbour = tilePosition + NEIGHBOUR_DIRECTIONS[i];
            if (neighbour.x is < 0 or >= InteropConstants.TILES_ROW_SIZE || neighbour.y is < 0 or >= InteropConstants.TILES_ROW_SIZE)
            {
                continue;
            }

            neighboursBuffer[neighboursCount++] = neighbour.y * InteropConstants.TILES_ROW_SIZE + neighbour.x;
        }

        return neighboursCount;
    }

    private bool HasActiveNeighbours(int tileIndex)
    {
        int[] neighboursBuffer = new int[DIRECTIONS_COUNT];
        int neighboursCount = GetTileNeighbours(tileIndex, neighboursBuffer);
        for (int i = 0; i < neighboursCount; ++i)
        {
            int neighbour = neighboursBuffer[i];
            if (m_tiles[neighbour].IsActive())
            {
                return true;
            }
        }

        return false;
    }

    private void OnTileClicked(Tile tile)
    {
        if (m_boardState == BoardState.Play)
        {
            OnPlayTileClicked(tile);
        }
        else
        {
            OnEditorTileClicked(tile);
        }
    }

    private void OnWallHoverStart(Wall wall)
    {
        if (!IsState(BoardState.Play)) { return; } // Can't place walls during edit mode.
        if (wall.IsWallPlaced()) { return; } // Can't do anything with a placed wall, ignore.
        if (m_isPlayerMovement) { return; } // Can't preview wall placement on player movement mode.

        int wallIndex = m_walls.IndexOf(wall);
        if (m_playerActions.Filter(a => a.actionType ==
                                        (m_isWallPlacementVertical ? ActionType.WallVertical : ActionType.WallHorizontal) 
                                        && a.to == wallIndex).Length == 0) { return; }

        if (m_currentWallHover != wall && m_currentWallHover)
        {
            m_currentWallHover.SetTemporaryState(WallState.Empty);
        }
        
        m_currentWallHover = wall;
        wall.SetTemporaryState(m_isWallPlacementVertical ? WallState.Vertical : WallState.Horizontal);
    }

    private void OnWallHoverEnd(Wall wall)
    {
        if (!IsState(BoardState.Play)) { return; } // Can't place walls during edit mode.
        if (wall.IsWallPlaced()) { return; } // Can't do anything with a placed wall, ignore.
        
        int wallIndex = m_walls.IndexOf(wall);
        if (m_playerActions.Filter(a => a.actionType ==
                                        (m_isWallPlacementVertical ? ActionType.WallVertical : ActionType.WallHorizontal) 
                                        && a.to == wallIndex).Length == 0) { return; }

        if (m_currentWallHover == wall)
        {
            m_currentWallHover = null;
        }
        
        wall.SetTemporaryState(WallState.Empty);
    }

    private void OnWallClicked(Wall wall)
    {
        if (!IsState(BoardState.Play)) { return; } // Can't place walls during edit mode.
        if (wall.IsWallPlaced()) { return; } // Can't do anything with a placed wall, ignore.
        if (m_isPlayerMovement) { return; } // Can't place a wall while in movement mode.

        int wallIndex = m_walls.IndexOf(wall);
        if (m_playerActions.Filter(a => a.actionType ==
                                        (m_isWallPlacementVertical ? ActionType.WallVertical : ActionType.WallHorizontal) 
                                        && a.to == wallIndex).Length == 0) { return; }
        
        if (m_currentWallHover == wall)
        {
            m_currentWallHover = null;
        }
        
        wall.SetState(m_isWallPlacementVertical ? WallState.Vertical : WallState.Horizontal);
        m_bridge.PlaceWall(true, wallIndex, m_isWallPlacementVertical);
        m_wallDisplayP1.UpdateCount(m_bridge.GetPlayerWallCount());
        OnPlayerActionMade?.Invoke();
    }

    private void OnEditorTileClicked(Tile tile)
    {
        Action<Tile> tileAction = m_boardState switch
        {
            BoardState.AddTiles => HandleAddTile,
            BoardState.RemoveTiles => HandleRemoveTile,
            BoardState.SetPlayerSpawn => HandleSetPlayerSpawn,
            BoardState.SetOpponentSpawn => HandleSetOpponentSpawn,
            BoardState.SetPlayerGoal => HandleSetPlayerGoal,
            BoardState.SetOpponentGoal => HandleSetOpponentGoal,
            _ => throw new ArgumentOutOfRangeException(nameof(m_boardState))
        };
        
        tileAction.Invoke(tile);
    }

    private void HandleAddTile(Tile tile) => HandleAddTile(tile, false);
    
    private void HandleAddTile(Tile tile, bool force)
    {
        if (tile.IsActive() || !force && !tile.IsPlaceable()) { return; }

        tile.SetState(TileState.Active, true);
        tile.SetColor(TILE_COLOR_DEFAULT);
        tile.SetAlpha(1);
        tile.SetScale(1);
        int[] neighboursBuffer = new int[DIRECTIONS_COUNT];
        int neighboursCount = GetTileNeighbours(m_tiles.IndexOf(tile), neighboursBuffer);
        for (int i = 0; i < neighboursCount; ++i)
        {
            int neighbour = neighboursBuffer[i];
            if (!m_tiles[neighbour].IsActive())
            {
                m_tiles[neighbour].SetState(TileState.Placeable, true);
                m_tiles[neighbour].SetAlpha(1);
            }
        }
    }

    private void HandleRemoveTile(Tile tile)
    {
        if (!tile.IsActive()) { return; }

        int tileIndex = m_tiles.IndexOf(tile);
        if (!TileRemovalValidator.Check(m_tiles, tileIndex)) { return; }
        
        tile.SetState(TileState.Placeable, true);
        tile.SetAlpha(0);
        int[] neighboursBuffer = new int[DIRECTIONS_COUNT];
        int neighboursCount = GetTileNeighbours(m_tiles.IndexOf(tile), neighboursBuffer);
        for (int i = 0; i < neighboursCount; ++i)
        {
            int neighbour = neighboursBuffer[i];
            if (m_tiles[neighbour].IsPlaceable() && !HasActiveNeighbours(neighbour))
            {
                m_tiles[neighbour].SetState(TileState.Placeable, false);
                m_tiles[neighbour].SetAlpha(0);
            }
        }
    }

    private void HandleSetPlayerSpawn(Tile tile)
    {
        if (!tile.IsActive()) { return; }
        
        m_tiles.FirstOrDefault(t => t.IsPlayerSpawn())?.SetState(TileState.PlayerSpawn, false);
        tile.SetState(TileState.PlayerSpawn, true);
    }

    private void HandleSetOpponentSpawn(Tile tile)
    {
        if (!tile.IsActive()) { return; }

        m_tiles.FirstOrDefault(t => t.IsOpponentSpawn())?.SetState(TileState.OpponentSpawn, false);
        tile.SetState(TileState.OpponentSpawn, true);
    }

    private void HandleSetPlayerGoal(Tile tile)
    {
        if (!tile.IsActive()) { return; }
        
        tile.SetState(TileState.PlayerGoal, true);
    }

    private void HandleSetOpponentGoal(Tile tile)
    {
        if (!tile.IsActive()) { return; }
        
        tile.SetState(TileState.OpponentGoal, true);
    }
    
    private void RefreshTiles()
    {
        Action refreshAction = m_boardState switch
        {
            BoardState.AddTiles => HandleRefreshAddTiles,
            BoardState.RemoveTiles => HandleRefreshRemoveTiles,
            BoardState.SetPlayerSpawn => HandleRefreshDefault,
            BoardState.SetOpponentSpawn => HandleRefreshDefault,
            BoardState.SetPlayerGoal => HandleRefreshDefault,
            BoardState.SetOpponentGoal => HandleRefreshDefault,
            BoardState.Play => HandleRefreshPlay,
            _ => throw new ArgumentOutOfRangeException()
        };
        
        refreshAction.Invoke();
    }

    private void HandleRefreshAddTiles()
    {
        // Reset animation timer.
        m_animationProgress = 0;
        
        // Reset visibility of placeable tiles.
        m_tiles.Filter(t => t.IsPlaceable()).ForEach(t => {
            t.SetAlpha(1);
        });
        
        // Reset color of active tiles.
        m_tiles.Filter(t => t.IsActive()).ForEach(t => {
            t.SetColor(TILE_COLOR_DEFAULT);
        });
    }

    private void HandleRefreshRemoveTiles()
    {
        // Hide placeable tiles.
        m_tiles.Filter(t => t.IsPlaceable()).ForEach(t => {
            t.SetAlpha(0);
        });
        
        // Change active tiles color.
        m_tiles.Filter(t => t.IsActive()).ForEach(t => {
            t.SetColor(TILE_COLOR_REMOVE);
        });
    }

    private void HandleRefreshDefault()
    {
        // Hide placeable tiles.
        m_tiles.Filter(t => t.IsPlaceable()).ForEach(t => {
            t.SetAlpha(0);
        });
        
        // Reset color of active tiles.
        m_tiles.Filter(t => t.IsActive()).ForEach(t => {
            t.SetColor(TILE_COLOR_DEFAULT);
        });
    }

    private void HandleRefreshPlay()
    {
        HandleRefreshDefault();
        
        m_tiles.ForEach(t => t.SetPawnActive(false));
        m_tiles[m_playerPosition].SetPawn(PawnType.Player);
        m_tiles[m_playerPosition].SetPawnActive(true);
        m_tiles[m_opponentPosition].SetPawn(PawnType.Opponent);
        m_tiles[m_opponentPosition].SetPawnActive(true);
        
        if (m_isPlayerMovement)
        {
            GameAction[] movementActions = m_playerActions.Filter(a => a.actionType == ActionType.Movement);
            foreach (GameAction movementAction in movementActions)
            {
                if (!m_tiles[movementAction.to].IsActive()) { continue; }
                
                m_tiles[movementAction.to].SetPawn(PawnType.Movement);
                m_tiles[movementAction.to].SetPawnActive(true);
            }
        }
    }

    private void OnPlayTileClicked(Tile tile) 
    {
        if (!m_isPlayerMovement) { return; }
        
        int tileIndex = m_tiles.IndexOf(tile);
        GameAction[] results = m_playerActions.Filter(a => a.actionType == ActionType.Movement && a.to == tileIndex);
        if (results.Length == 0) { return; }

        m_playerPosition = results[0].to;
        m_bridge.UpdatePlayerPosition(m_playerPosition);
        m_isPlayerMovement = false;
        RefreshTiles();
        m_isPlayerMovement = true;

        if (m_tiles.IndexesOf(t => t.IsPlayerGoal()).Contains(m_playerPosition))
        {
            OnGameEnd?.Invoke(true);
            return;
        }
        
        OnPlayerActionMade?.Invoke();
    }
}
