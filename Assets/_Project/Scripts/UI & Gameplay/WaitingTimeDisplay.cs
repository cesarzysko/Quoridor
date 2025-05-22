using TMPro;
using UnityEngine;

public class WaitingTimeDisplay : MonoBehaviour
{
	[SerializeField] private TMP_Text m_textRenderer;

	public void UpdateTime(float time)
	{
		if (time == 0)
		{
			m_textRenderer.text = "Last waiting time: -";
		}
		else if (time < 0.01f)
		{
			m_textRenderer.text = "Last waiting time: < 0.01 s";
		}
		else if (time >= 100f)
		{
			m_textRenderer.text = "Last waiting time: > 100 s";
		}
		else
		{
			m_textRenderer.text = $"Last waiting time: {time:0.00} s";
		}
	}
}
