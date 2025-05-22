using TMPro;
using UnityEngine;

public class PanelsManager : MonoBehaviour
{
    [SerializeField] private Board m_board;
    [SerializeField] private GameObject m_mainPanel;
    [SerializeField] private GameObject m_gamePanel;
    [SerializeField] private GameObject m_waitPanel;
    [SerializeField] private GameObject m_gameEndPanel;
    [SerializeField] private TMP_Text m_gameEndTextRenderer;
    [SerializeField] private GameObject[] m_panels;

    private void Awake()
    {
        m_panels.ForEach(p => p.SetActive(false));
        m_mainPanel.SetActive(true);

        Board.OnPlayerActionMade += OnPlayerActionMade;
        Board.OnOpponentActionMade += OnOpponentActionMade;
        Board.OnGameEnd += OnGameEnd;
    }

    private void OnPlayerActionMade()
    {
        m_gamePanel.SetActive(false);
        m_waitPanel.SetActive(true);
        
        m_board.PerformOpponentTurn();
    }

    private void OnOpponentActionMade(float msTimeSpent)
    {
        m_gamePanel.SetActive(true);
        m_waitPanel.SetActive(false);
    }

    private void OnGameEnd(bool playerWon)
    {
        m_gameEndTextRenderer.text = playerWon ? "Player won!" : "AI won!";
        
        m_gameEndPanel.SetActive(true);
        m_gamePanel.SetActive(false);
        m_waitPanel.SetActive(false);
    }
}
