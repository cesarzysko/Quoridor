using System;
using System.Runtime.InteropServices;

public class NativeQuoridorBridge
{
	private int[] m_heuristics = new int[InteropConstants.HEURISTICS_SIZE];
	private WallState[] m_walls = new WallState[InteropConstants.TOTAL_WALLS_COUNT];
	private int[] m_playerPositions = new int[InteropConstants.PLAYER_COUNT];
	private int[] m_remainingWalls = new int[InteropConstants.PLAYER_COUNT];
	private Algorithm m_alg;
	private int m_depth;
	
	private int[] m_playerGoals;
	private int[] m_opponentGoals;

	public NativeQuoridorBridge(
		Tile[] tiles, 
		int wallsCount, 
		Algorithm algorithm, 
		int depth)
	{
		// Fill in heuristics with a default value of 0 to map out where tiles exist.
		for (int i = 0; i < InteropConstants.TOTAL_TILES_COUNT; ++i)
		{
			m_heuristics[i] = tiles[i].IsActive() ? 0 : InteropConstants.NONE;
			m_heuristics[i + InteropConstants.TOTAL_TILES_COUNT] = tiles[i].IsActive() ? 0 : InteropConstants.NONE;
		}
		
		Array.Fill(m_walls, WallState.Empty);
		m_playerPositions[InteropConstants.AI_INDEX] = tiles.IndexOf(t => t.IsOpponentSpawn());
		m_playerPositions[InteropConstants.PLAYER_INDEX] = tiles.IndexOf(t => t.IsPlayerSpawn());
		m_remainingWalls[0] = wallsCount;
		m_remainingWalls[1] = wallsCount;
		m_alg = algorithm;
		m_depth = depth;

		m_playerGoals = tiles.IndexesOf(t => t.IsPlayerGoal());
		m_opponentGoals = tiles.IndexesOf(t => t.IsOpponentGoal());
		
		RecalculateHeuristics();
	}

	public void PlaceWall(
		bool playerWall, 
		int wallPosition, 
		bool isVerticalWall)
	{
		m_walls[wallPosition] = isVerticalWall ? WallState.Vertical : WallState.Horizontal;
		--m_remainingWalls[playerWall ? InteropConstants.PLAYER_INDEX : InteropConstants.AI_INDEX];
		RecalculateHeuristics();
	}

	public int GetPlayerWallCount() => m_remainingWalls[InteropConstants.PLAYER_INDEX];
	
	public int GetOpponentWallCount() => m_remainingWalls[InteropConstants.AI_INDEX];

	public void UpdatePlayerPosition(
		int position)
	{
		m_playerPositions[InteropConstants.PLAYER_INDEX] = position;
	}

	public void UpdateOpponentPosition(
		int position)
	{
		m_playerPositions[InteropConstants.AI_INDEX] = position;
	}

	public GameAction ComputePcAction()
	{
		return ComputePcAction_Internal(m_heuristics, m_walls, m_playerPositions, m_remainingWalls, m_alg, m_depth);
	}

	public GameAction[] ComputePlayerActions()
	{
		GameAction[] actionsBuffer = new GameAction[InteropConstants.BUFFER_LAYER_SIZE];
		int possibleActions = ComputePlayerActions_Internal(m_heuristics, m_walls, m_playerPositions, m_remainingWalls[InteropConstants.PLAYER_INDEX], actionsBuffer);
		Array.Resize(ref actionsBuffer, possibleActions);

		return actionsBuffer;
	}

	private void RecalculateHeuristics()
	{
		int[] heuristics = new int[InteropConstants.TOTAL_TILES_COUNT];
		// AI heuristics
		Array.ConstrainedCopy(m_heuristics, 0, heuristics, 0, InteropConstants.TOTAL_TILES_COUNT);
		heuristics = HeuristicsCalculator.FloodFillBFS(heuristics, m_opponentGoals, m_walls);
		Array.ConstrainedCopy(heuristics, 0, m_heuristics, 0, InteropConstants.TOTAL_TILES_COUNT);
		// Player heuristics
		Array.ConstrainedCopy(m_heuristics, InteropConstants.TOTAL_TILES_COUNT, heuristics, 0, InteropConstants.TOTAL_TILES_COUNT);
		heuristics = HeuristicsCalculator.FloodFillBFS(heuristics, m_playerGoals, m_walls);
		Array.ConstrainedCopy(heuristics, 0, m_heuristics, InteropConstants.TOTAL_TILES_COUNT, InteropConstants.TOTAL_TILES_COUNT);
	}
	
	[DllImport("QuoridorAI", CallingConvention = CallingConvention.Cdecl, EntryPoint = "compute_pc_action")]
	private static extern GameAction ComputePcAction_Internal(
		[MarshalAs(UnmanagedType.LPArray, SizeConst = InteropConstants.HEURISTICS_SIZE)]
		int[] heuristics,
		[MarshalAs(UnmanagedType.LPArray, SizeConst = InteropConstants.TOTAL_WALLS_COUNT)]
		WallState[] walls,
		[MarshalAs(UnmanagedType.LPArray, SizeConst = InteropConstants.PLAYER_COUNT)]
		int[] playerPositions,
		[MarshalAs(UnmanagedType.LPArray, SizeConst = InteropConstants.PLAYER_COUNT)]
		int[] remainingWalls,
		Algorithm algorithm,
		int maxDepth);

	[DllImport("QuoridorAI", CallingConvention = CallingConvention.Cdecl, EntryPoint = "compute_player_actions")]
	private static extern int ComputePlayerActions_Internal(
		[MarshalAs(UnmanagedType.LPArray, SizeConst = InteropConstants.HEURISTICS_SIZE)]
		int[] heuristics,
		[MarshalAs(UnmanagedType.LPArray, SizeConst = InteropConstants.TOTAL_WALLS_COUNT)]
		WallState[] walls,
		[MarshalAs(UnmanagedType.LPArray, SizeConst = InteropConstants.PLAYER_COUNT)]
		int[] playerPositions,
		int remainingWalls,
		[MarshalAs(UnmanagedType.LPArray, SizeConst = InteropConstants.BUFFER_LAYER_SIZE)]
		GameAction[] actionsBuffer);
}
