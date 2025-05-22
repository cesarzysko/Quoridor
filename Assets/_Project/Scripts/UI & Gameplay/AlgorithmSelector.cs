using TMPro;
using UnityEngine;

public class AlgorithmSelector : MonoBehaviour
{
    [SerializeField] private TMP_Text m_chosenTextRenderer;

    private Algorithm[] m_algorithms = {
        Algorithm.Minimax,
        Algorithm.MinimaxAlphaBeta,
        Algorithm.Negamax,
        Algorithm.NegamaxAlphaBeta
    };
    private int m_algorithmIndex;
    
    public Algorithm GetSelectedAlgorithm()
    {
        return m_algorithms[m_algorithmIndex];
    }

    public void OnRightPressed()
    {
        if (++m_algorithmIndex >= m_algorithms.Length)
        {
            m_algorithmIndex = 0;
        }
        
        UpdateText();
    }

    public void OnLeftPressed()
    {
        if (--m_algorithmIndex < 0)
        {
            m_algorithmIndex = m_algorithms.Length - 1;
        }
        
        UpdateText();
    }

    private void UpdateText()
    {
        m_chosenTextRenderer.text = m_algorithmIndex switch
        {
            0 => "Minimax",
            1 => "Minimax A-B",
            2 => "Negamax",
            3 => "Negamax A-B",
            _ => "-"
        };
    }
}
