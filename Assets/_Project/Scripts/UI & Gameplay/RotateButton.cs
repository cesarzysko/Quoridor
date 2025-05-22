using TMPro;
using UnityEngine;

public class RotateButton : MonoBehaviour
{
	[SerializeField] private Board m_board;
	[SerializeField] private TMP_Text m_buttonTextRenderer;

	public void OnButtonPress()
	{
		m_board.SwitchWallRotation();
		UpdateDisplay();
	}

	private void OnEnable()
	{
		UpdateDisplay();
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			OnButtonPress();
		}
	}

	private void UpdateDisplay()
	{
		m_buttonTextRenderer.text = m_board.IsWallPlacementVertical() ? "VERTICAL" : "HORIZONTAL";
	}
}
