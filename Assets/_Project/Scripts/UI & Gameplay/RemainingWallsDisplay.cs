using TMPro;
using UnityEngine;

public class RemainingWallsDisplay : MonoBehaviour
{
    [SerializeField] private bool m_isP1;
    [SerializeField] private TMP_Text m_textRenderer;

    public void UpdateCount(int count)
    {
        m_textRenderer.text = $"P{(m_isP1 ? "1" : "2")} remaining walls: {count}";
    }
}
