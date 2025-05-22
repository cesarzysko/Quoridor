using System;
using TMPro;
using UnityEngine;

public class BoardNameEntry : MonoBehaviour
{
    public static event Action<string> OnEntryPress;

    [SerializeField] private TMP_Text m_boardTextRenderer;

    public void Init(string text)
    {
        m_boardTextRenderer.text = text;
    }
    
    public void OnEntryPressed()
    {
        OnEntryPress?.Invoke(m_boardTextRenderer.text);
    }
}
