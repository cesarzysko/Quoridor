using System.Collections.Generic;
using System.Linq;

public static class TileRemovalValidator
{
	public static bool Check(Tile[] tiles, int tileIndexToRemove)
	{
		bool[] map = new bool[InteropConstants.TOTAL_TILES_COUNT];
		for (int i = 0; i < InteropConstants.TOTAL_TILES_COUNT; ++i)
		{
			map[i] = tiles[i].IsActive();
		}

		map[tileIndexToRemove] = false;
		int activeCount = map.Count(e => e);
		if (activeCount == 0) { return false; } // Tiles graph can't be empty.
		if (activeCount == 1) { return true; } // Only one tile left, no need to check anything.

		List<int> removedTileNeighbours = GetNeighbours(map, tileIndexToRemove);
		int anyNeighbour = removedTileNeighbours.Count > 0 ? removedTileNeighbours[0] : -1;
		if (anyNeighbour == -1) { return false; } // activeCount is at least 2, yet no neighbour was found.

		Queue<int> openSet = new(new int[]{anyNeighbour});
		map[anyNeighbour] = false;
		while (openSet.Count > 0)
		{
			int index = openSet.Dequeue();
			foreach (int nIndex in GetNeighbours(map, index))
			{
				openSet.Enqueue(nIndex);
				map[nIndex] = false;
			}
		}

		return map.Count(e => e) == 0;
	}
	
	private static List<int> GetNeighbours(bool[] map, int sourceTileIndex)
	{
		List<int> neighbours = new List<int>(4);
		int x = sourceTileIndex % InteropConstants.TILES_ROW_SIZE;
		int y = sourceTileIndex / InteropConstants.TILES_ROW_SIZE;

		if (x != 0 && map[sourceTileIndex - 1])
			neighbours.Add(sourceTileIndex - 1);
		if (x != InteropConstants.TILES_ROW_SIZE - 1 && map[sourceTileIndex + 1])
			neighbours.Add(sourceTileIndex + 1);
		if (y != 0 && map[sourceTileIndex - InteropConstants.TILES_ROW_SIZE])
			neighbours.Add(sourceTileIndex - InteropConstants.TILES_ROW_SIZE);
		if (y != InteropConstants.TILES_ROW_SIZE - 1 && map[sourceTileIndex + InteropConstants.TILES_ROW_SIZE])
			neighbours.Add(sourceTileIndex + InteropConstants.TILES_ROW_SIZE);

		return neighbours;
	}
}