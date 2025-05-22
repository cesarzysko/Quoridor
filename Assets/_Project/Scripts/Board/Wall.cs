using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Wall : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IEquatable<Wall>
{
	public static event Action<Wall> OnClicked;
	public static event Action<Wall> OnHoverStart;
	public static event Action<Wall> OnHoverEnd;
	
	[SerializeField] private SpriteRenderer m_wallRenderer;
	[SerializeField] private Collider2D m_collider;
		
	private ClickState m_clickState;
	private WallState m_state;

	public void SetState(WallState state)
	{
		m_state = state;
		transform.rotation = Quaternion.Euler(0, 0, state == WallState.Vertical ? 90 : 0);
		m_wallRenderer.color = IsWallPlaced() ? TileSpritesLibrarySO.GetDefaultWallColor() : new Color(0, 0, 0, 0);
	}

	public void SetTemporaryState(WallState state)
	{
		transform.rotation = Quaternion.Euler(0, 0, state == WallState.Vertical ? 90 : 0);
		m_wallRenderer.color = state != WallState.Empty ? TileSpritesLibrarySO.GetTempWallColor() : new Color(0, 0, 0, 0);
	}

	public bool IsWallPlaced()
		=> m_state != WallState.Empty;

	public void SetColliderActive(bool active)
	{
		m_collider.enabled = active;
	}
	
	private void SetClickStateFlag(ClickState state, bool active)
	{
		if (active)
		{
			m_clickState |= state;
		}
		else
		{
			m_clickState &= ~state;
		}
	}
	
	private bool HasClickStateFlag(ClickState state)
	{
		return m_clickState.HasFlag(state);
	}
	
	private bool IsPressed()
		=> HasClickStateFlag(ClickState.Press);

	private bool IsHovered()
		=> HasClickStateFlag(ClickState.Hover);

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		SetClickStateFlag(ClickState.Hover, true);
		OnHoverStart?.Invoke(this);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		SetClickStateFlag(ClickState.Hover, false);
		OnHoverEnd?.Invoke(this);
	}

	void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
		=> SetClickStateFlag(ClickState.Press, true);

	void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
	{
		if (IsPressed())
		{
			if (IsHovered())
			{
				OnClicked?.Invoke(this);
			}
            
			SetClickStateFlag(ClickState.Press, false);
		}
	}

	bool IEquatable<Wall>.Equals(Wall other)
	{
		return GetInstanceID() == other.GetInstanceID();
	}
}