using System.IO;
using TMPro;
using UnityEngine;

public class LobbyPanelManager : MonoBehaviour
{
	private static string PATH => Path.Combine(Application.persistentDataPath, "maps");
	
	[Header("ASSETS")]
	[SerializeField] private BoardNameEntry m_boardNameEntryPrefab;

	[Header("REFERENCES")] 
	[SerializeField] private GameObject m_lobbyPanel;
	[SerializeField] private GameObject m_gamePanel;
	[SerializeField] private Transform m_content;
	[SerializeField] private TMP_InputField m_inputField;
	[SerializeField] private Board m_board;
	[SerializeField] private AlgorithmSelector m_algorithmSelector;
	[SerializeField] private DepthSelector m_depthSelector;
	[SerializeField] private WallsSelector m_wallsSelector;
	
	private void Start()
	{
		if (!Directory.Exists(PATH))
		{
			Directory.CreateDirectory(PATH);
		}
	}

	public void OnLoadPressed()
	{
		if (m_inputField.text == string.Empty) { return; }
        
		string path = Path.Combine(PATH, $"{m_inputField.text}.json");
		if (!File.Exists(path)) { return; }
		
		string jsonData = File.ReadAllText(path);
		BoardSaveData boardSaveData = JsonUtility.FromJson<BoardSaveData>(jsonData);
		m_board.LoadBoardSaveData(boardSaveData);
		m_board.EnterPlayMode(
			m_wallsSelector.GetWallsCount(), 
			m_algorithmSelector.GetSelectedAlgorithm(), 
			m_depthSelector.GetDepth());

		m_lobbyPanel.SetActive(false);
		m_gamePanel.SetActive(true);
	}
	
	private void OnEnable()
	{
		BoardNameEntry.OnEntryPress += OnEntryPressed;
		string[] files = Directory.GetFiles(PATH);
		int count = files.Length;
		for (int i = 0; i < count; ++i)
		{
			string jsonData = File.ReadAllText(files[i]);
			BoardSaveData data = JsonUtility.FromJson<BoardSaveData>(jsonData);
			if (!data.IsValid()) { continue; }
			
			files[i] = files[i].Replace('\\', '/');
			files[i] = files[i].Split('/')[^1][..^5];
			Instantiate(m_boardNameEntryPrefab, m_content).Init(files[i]);
		}
	}

	private void OnDisable()
	{
		BoardNameEntry.OnEntryPress -= OnEntryPressed;
		m_inputField.text = string.Empty;
		foreach (Transform entry in m_content)
		{
			Destroy(entry.gameObject);
		}
	}

	private void OnEntryPressed(string boardName)
	{
		m_inputField.text = boardName;
	}
}
