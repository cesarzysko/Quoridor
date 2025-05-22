public static class InteropConstants
{
	public const int TILES_ROW_SIZE = 16;
	public const int TOTAL_TILES_COUNT = TILES_ROW_SIZE * TILES_ROW_SIZE;
	public const int PLAYER_COUNT = 2;
	public const int WALLS_ROW_SIZE = TILES_ROW_SIZE - 1;
	public const int TOTAL_WALLS_COUNT = WALLS_ROW_SIZE * WALLS_ROW_SIZE;
	public const int BUFFER_LAYER_SIZE = 2 * TOTAL_WALLS_COUNT + 5;
	public const int PLAYER_INDEX = 1;
	public const int AI_INDEX = 0;
	public const int HEURISTICS_SIZE = PLAYER_COUNT * TOTAL_TILES_COUNT;
	public const int NONE = -1;
}
