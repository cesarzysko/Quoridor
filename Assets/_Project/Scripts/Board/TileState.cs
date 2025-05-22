using System;

[Flags]
public enum TileState
{
	None = 0,
	Placeable     = 1 << 0,
	Active        = 1 << 1,
	PlayerSpawn   = 1 << 2,
	OpponentSpawn = 1 << 3,
	PlayerGoal    = 1 << 4,
	OpponentGoal  = 1 << 5
}
