using System;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class Tile : MonoBehaviour, IEquatable<Tile>, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    /*** [ Events ] **********************************/
    public static event Action<Tile> OnTileClicked;
    
    /*** [ Serialized fields ] ***********************/
    [Header("Self-References")]
    [SerializeField] private SpriteRenderer m_tileRenderer;
    [SerializeField] private SpriteRenderer m_icon1Renderer;
    [SerializeField] private SpriteRenderer m_icon2Renderer;
    [SerializeField] private SpriteRenderer m_pawnRenderer;

    /*** [ Private fields ] **************************/
    private float m_alpha = 1;
    private float m_pawnAlpha = 1;
    private TileState m_tileState = TileState.None;
    private ClickState m_clickState;

    /*** [ Public methods ] **************************/
    public void SetPawnActive(bool active)
    {
        Color color = m_pawnRenderer.color;
        m_pawnAlpha = color.a = active ? 1 : 0;
        m_pawnRenderer.color = color;
    }

    public void SetPawn(PawnType pawnType)
    {
        (Sprite pawnSprite, Color pawnColor) = TileSpritesLibrarySO.GetPawn(pawnType);
        pawnColor.a = m_pawnAlpha;
        m_pawnRenderer.sprite = pawnSprite;
        m_pawnRenderer.color = pawnColor;
    }
    
    public void SetState(TileState state, bool active)
    {
        if (active && IsState(state) || !active && !IsState(state)) { return; }
        
        switch (state)
        {
            case TileState.None:
                m_tileState = TileState.None;
                m_icon1Renderer.sprite = null;
                m_icon2Renderer.sprite = null;
                return;
            case TileState.Active:
                if (!active)
                {
                    m_tileState = TileState.None;
                    m_icon1Renderer.sprite = null;
                    m_icon2Renderer.sprite = null;
                    return;
                }
                SetStateFlag(TileState.Placeable, false);
                break;
            case TileState.Placeable:
                if (active)
                {
                    m_tileState = TileState.Placeable;
                    m_icon1Renderer.sprite = null;
                    m_icon2Renderer.sprite = null;
                    return;
                }
                break;
            case TileState.PlayerSpawn:
                SetStateFlag(TileState.OpponentSpawn, false);
                SetStateFlag(TileState.PlayerGoal, false);
                break;
            case TileState.OpponentSpawn:
                SetStateFlag(TileState.PlayerSpawn, false);
                SetStateFlag(TileState.OpponentGoal, false);
                break;
            case TileState.PlayerGoal:
                SetStateFlag(TileState.OpponentGoal, false);
                SetStateFlag(TileState.PlayerSpawn, false);
                break;
            case TileState.OpponentGoal:
                SetStateFlag(TileState.PlayerGoal, false);
                SetStateFlag(TileState.OpponentSpawn, false);
                break;
        }
        
        SetStateFlag(state, active);
    }

    private void SetStateFlag(TileState flag, bool active)
    {
        if (active && IsState(flag) || !active && !IsState(flag)) { return; }
        
        if (active)
            m_tileState |= flag;
        else
            m_tileState &= ~flag;
        
        (Sprite iconSprite, Color iconColor) = TileSpritesLibrarySO.GetIconByState(flag, active);
        SpriteRenderer iconRenderer = flag is >= TileState.Placeable and <= TileState.OpponentSpawn ? m_icon1Renderer : m_icon2Renderer;
        iconRenderer.sprite = iconSprite;
        iconRenderer.color = iconColor;
    }

    public bool IsActive()
        => IsState(TileState.Active);

    public bool IsPlaceable() 
        => IsState(TileState.Placeable);

    public bool IsPlayerSpawn()
        => IsState(TileState.PlayerSpawn);

    public bool IsOpponentSpawn()
        => IsState(TileState.OpponentSpawn);

    public bool IsPlayerGoal()
        => IsState(TileState.PlayerGoal);

    public bool IsOpponentGoal()
        => IsState(TileState.OpponentGoal);

    public void SetAlpha(float alpha)
    {
        m_alpha = Mathf.Clamp01(alpha);
        SetColor(m_tileRenderer, m_tileRenderer.color);
        SetColor(m_icon1Renderer, m_icon1Renderer.color);
        SetColor(m_icon2Renderer, m_icon2Renderer.color);
    }

    public void SetColor(Color color)
        => SetColor(m_tileRenderer, color);

    private void SetColor(SpriteRenderer renderer, Color color)
    {
        color.a = m_alpha;
        renderer.color = color;
    }

    public void SetScale(float scale)
    {
        transform.localScale = scale * Vector3.one;
    }

    /*** [ Private methods ] *************************/
    private void Awake()
    {
        SetAlpha(0);
        SetScale(1);
        SetColor(m_tileRenderer, Color.white); 
        m_icon1Renderer.sprite = null;
        m_icon2Renderer.sprite = null;
    }

    private bool IsState(TileState state)
    {
        return m_tileState.HasFlag(state);
    }
    
    private bool HasClickStateFlag(ClickState state)
    {
        return m_clickState.HasFlag(state);
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

    private bool IsPressed()
        => HasClickStateFlag(ClickState.Press);

    private bool IsHovered()
        => HasClickStateFlag(ClickState.Hover);

    /*** [ IEquatable Implementation ] ***************/
    bool IEquatable<Tile>.Equals(Tile other)
    {
        return other.GetInstanceID() == GetInstanceID();
    }

    /*** [ IPointer Implementation ] *****************/
    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        => SetClickStateFlag(ClickState.Hover, true);

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        => SetClickStateFlag(ClickState.Hover, false);

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        => SetClickStateFlag(ClickState.Press, true);

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (IsPressed())
        {
            if (IsHovered())
            {
                OnTileClicked?.Invoke(this);
            }
            
            SetClickStateFlag(ClickState.Press, false);
        }
    }
}
