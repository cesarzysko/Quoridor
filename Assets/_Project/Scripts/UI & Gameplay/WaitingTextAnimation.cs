using TMPro;
using UnityEngine;

public class WaitingTextAnimation : MonoBehaviour
{
    [SerializeField] private TMP_Text m_textRenderer;

    private float m_elapsedTime;
    
    private void OnEnable()
    {
        m_elapsedTime = 0;
    }

    private void Update()
    {
        m_elapsedTime += Time.deltaTime;
        m_textRenderer.text = $"Waiting for {m_elapsedTime:0.00} s...";
    }
}
