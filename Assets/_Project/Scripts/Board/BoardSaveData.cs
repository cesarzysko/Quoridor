using System;
using System.Collections.Generic;

[Serializable]
public struct BoardSaveData
{
	public bool[] map;
	public int playerSpawn;
	public int opponentSpawn;
	public int[] playerGoals;
	public int[] opponentGoals;

	public BoardSaveData(Tile[] tiles)
	{
		map = new bool[InteropConstants.TOTAL_TILES_COUNT];
		playerSpawn = -1;
		opponentSpawn = -1;
		playerGoals = Array.Empty<int>();
		opponentGoals = Array.Empty<int>();
		
		List<int> playerGoalsList = new();
		List<int> opponentGoalsList = new();
		for (int i = 0; i < InteropConstants.TOTAL_TILES_COUNT; ++i)
		{
			if (!tiles[i].IsActive()) { continue; }

			map[i] = true;
			if (tiles[i].IsPlayerSpawn())
				playerSpawn = i;
			else if (tiles[i].IsOpponentSpawn())
				opponentSpawn = i;

			if (tiles[i].IsPlayerGoal())
				playerGoalsList.Add(i);
			else if (tiles[i].IsOpponentGoal())
				opponentGoalsList.Add(i);
		}

		if (playerGoalsList.Count > 0)
			playerGoals = playerGoalsList.ToArray();
		if (opponentGoalsList.Count > 0)
			opponentGoals = opponentGoalsList.ToArray();
	}

	public bool IsValid()
	{
		return map != null
		       && playerSpawn != -1 && opponentSpawn != -1
		       && playerGoals != null && playerGoals.Length > 0
		       && opponentGoals != null && opponentGoals.Length > 0;
	}
}