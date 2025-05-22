using TMPro;
using UnityEngine;

public class WallsSelector : MonoBehaviour
{
    private const int MIN_WALLS = 1;
    private const int MAX_WALLS = 25;
    
    [SerializeField] private TMP_Text m_wallsTextRenderer;
    
    private int m_wallsCount = 10;
    
    public int GetWallsCount()
    {
        return m_wallsCount;
    }

    public void OnRightPressed()
    {
        if (++m_wallsCount > MAX_WALLS)
        {
            m_wallsCount = MAX_WALLS;
        }
        
        UpdateText();
    }

    public void OnLeftPressed()
    {
        if (--m_wallsCount < MIN_WALLS)
        {
            m_wallsCount = MIN_WALLS;
        }
        
        UpdateText();
    }

    private void UpdateText()
    {
        m_wallsTextRenderer.text = $"{m_wallsCount}";
    }
}
