using System;
using System.Collections.Generic;

public static class HeuristicsCalculator
{
	public static int[] FloodFillBFS(
		int[] heuristics,
		int[] goalPositions,
		WallState[] walls)
	{
		int[] results = new int[InteropConstants.TOTAL_TILES_COUNT];
		Array.Fill(results, int.MaxValue);
		
		Queue<(int, int)> nodes = new();
		foreach (int goal in goalPositions)
		{
			nodes.Enqueue((goal, 0));
			results[goal] = 0;
		}

		int[] neighboursBuffer = new int[4];
		while (nodes.Count > 0)
		{
			(int pos, int depth) = nodes.Dequeue();
			int nCount = GetNeighbours(pos, heuristics, walls, neighboursBuffer);
			for (int i = 0; i < nCount; ++i)
			{
				if (results[neighboursBuffer[i]] == int.MaxValue) // Neither explored nor added to the openSet.
				{
					results[neighboursBuffer[i]] = depth + 1;
					nodes.Enqueue((neighboursBuffer[i], depth + 1));
				}
			}
		}

		for (int i = 0; i < InteropConstants.TOTAL_TILES_COUNT; ++i)
		{
			if (results[i] == int.MaxValue)
			{
				results[i] = InteropConstants.NONE;
			}
		}

		return results;
	}
	
	private static int GetNeighbours(
		int origin,
		int[] hScores,
		WallState[] walls,
		int[] outBuffer)
	{
		int[] DIRECTIONS = {
			1, -1, // Left & Right
			InteropConstants.TILES_ROW_SIZE, -InteropConstants.TILES_ROW_SIZE // Up & Down
		};

		int n_count = 0;
		for (int i = 0; i < 4; ++i)
		{
			int n = origin + DIRECTIONS[i];

			// Bounds check
			if (n < 0 || n >= InteropConstants.TOTAL_TILES_COUNT || hScores[n] == InteropConstants.NONE)
			{
				continue;
			}

			// Walls check
			int min = Math.Min(origin, n);
			int depth = min / InteropConstants.TILES_ROW_SIZE;
			int w2 = min - depth;
			if ((DIRECTIONS[i] & 1) == 1) // Moving left or right
			{
				// Row step check
				if (DIRECTIONS[i] == -1)
				{
					if (origin % InteropConstants.TILES_ROW_SIZE == 0)
					{
						continue;
					}
				}
				else
				{
					if (origin % InteropConstants.TILES_ROW_SIZE == InteropConstants.TILES_ROW_SIZE - 1)
					{
						continue;
					}
				}

				if (w2 < InteropConstants.TOTAL_TILES_COUNT && walls[w2] == WallState.Vertical)
				{
					continue;
				}

				int w1 = w2 - InteropConstants.WALLS_ROW_SIZE;
				if (w1 >= 0 && walls[w1] == WallState.Vertical)
				{
					continue;
				}
			}
			else // Moving up or down
			{
				if (w2 < InteropConstants.TOTAL_TILES_COUNT && walls[w2] == WallState.Horizontal)
				{
					continue;
				}

				int w1 = w2 - 1;
				if (w1 >= 0 && walls[w1] == WallState.Horizontal)
				{
					continue;
				}
			}

			outBuffer[n_count++] = n;
		}

		return n_count;
	}
}
