using TMPro;
using UnityEngine;

public class DepthSelector : MonoBehaviour
{
	private const int MIN_DEPTH = 1;
	private const int MAX_DEPTH = 5;
    
	[SerializeField] private TMP_Text m_depthTextRenderer;
    
	private int m_depth = 2;
    
	public int GetDepth()
	{
		return m_depth;
	}

	public void OnRightPressed()
	{
		if (++m_depth > MAX_DEPTH)
		{
			m_depth = MAX_DEPTH;
		}
        
		UpdateText();
	}

	public void OnLeftPressed()
	{
		if (--m_depth < MIN_DEPTH)
		{
			m_depth = MIN_DEPTH;
		}
        
		UpdateText();
	}

	private void UpdateText()
	{
		m_depthTextRenderer.text = $"{m_depth}";
	}
}
