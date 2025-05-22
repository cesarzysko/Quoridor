using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Tile Sprites Library", fileName = "Tile Sprites Library")]
public class TileSpritesLibrarySO : ScriptableObject, ILoadable
{
	private static readonly Color PlayerColor = new Color(0.388f, 0.224f, 0.891f);
	private static readonly Color OpponentColor = new Color(0.925f, 0.941f, 0.086f);
	private static readonly Color MovementColor = new Color(0.4f, 0.4f, 0.4f, 0.5f);
	private static readonly Color WallColor = new Color(0.91f, 0.59f, 0.34f);
	private static readonly Color TempWallColor = new Color(0.97f, 0.27f, 0.29f);
	
	private static TileSpritesLibrarySO Instance;
	
	[SerializeField] private Sprite m_spawn;
	[SerializeField] private Sprite m_goal;

	private (Sprite, Color) None => (null, default);
	private (Sprite, Color) PlayerSpawn => (m_spawn, PlayerColor);
	private (Sprite, Color) PlayerGoal => (m_goal, PlayerColor);
	private (Sprite, Color) OpponentSpawn => (m_spawn, OpponentColor);
	private (Sprite, Color) OpponentGoal => (m_goal, OpponentColor);
	private (Sprite, Color) MovementPawn => (m_spawn, MovementColor);

	public void Load()
	{
		Assertions.EnsureTrue(!Instance, "There already is a TileSpritesLibrarySO singleton reference set.");
		Instance = this;
	}

	public static Color GetDefaultWallColor() 
		=> WallColor;
	
	public static Color GetTempWallColor() 
		=> TempWallColor;

	public static (Sprite, Color) GetPawn(PawnType pawnType)
	{
		Assertions.EnsureTrue(Instance, "TileSpritesLibrarySO singleton reference is not set.");
		return Instance.GetPawn_Implementation(pawnType);
	}

	public static (Sprite, Color) GetIconByState(TileState state, bool active)
	{
		Assertions.EnsureTrue(Instance, "TileSpritesLibrarySO singleton reference is not set.");
		return Instance.GetIconByState_Implementation(state, active);
	}

	private (Sprite, Color) GetPawn_Implementation(PawnType pawnType)
	{
		return pawnType switch
		{
			PawnType.Player => PlayerSpawn,
			PawnType.Opponent => OpponentSpawn,
			PawnType.Movement => MovementPawn,
			_ => throw new ArgumentOutOfRangeException(nameof(pawnType), pawnType, null)
		};
	}

	private (Sprite, Color) GetIconByState_Implementation(TileState state, bool active)
	{
		if (!active) { return None; }
		
		return state switch
		{
			TileState.Placeable => None,
			TileState.Active => None,
			TileState.PlayerSpawn => PlayerSpawn,
			TileState.PlayerGoal => PlayerGoal,
			TileState.OpponentSpawn => OpponentSpawn,
			TileState.OpponentGoal => OpponentGoal,
			_ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
		};
	}
}