using TMPro;
using UnityEngine;

public class ModeButton : MonoBehaviour
{
	[SerializeField] private Board m_board;
	[SerializeField] private TMP_Text m_buttonTextRenderer;
	[SerializeField] private GameObject m_rotateButton;
	
	public void OnButtonPress()
	{
		m_board.SwitchPlayMode();
		UpdateDisplay();
	}

	private void OnEnable()
	{
		UpdateDisplay();
	}

	private void UpdateDisplay()
	{
		m_buttonTextRenderer.text = m_board.IsPlayerMovement() ? "MOVEMENT" : "WALL";
		m_rotateButton.SetActive(!m_board.IsPlayerMovement());
	}
}
