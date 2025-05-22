using System.IO;
using TMPro;
using UnityEngine;

public class SaveLoadPanelManager : MonoBehaviour
{
    private static string PATH => Path.Combine(Application.persistentDataPath, "maps");
    
    [Header("ASSETS")]
    [SerializeField] private BoardNameEntry m_boardNameEntryPrefab;
    [Header("REFERENCES")]
    [SerializeField] private Transform m_content;
    [SerializeField] private TMP_InputField m_inputField;
    [SerializeField] private Board m_board;

    private void Start()
    {
        if (!Directory.Exists(PATH))
        {
            Directory.CreateDirectory(PATH);
        }
    }

    public void OnSavePressed()
    {
        if (m_inputField.text == string.Empty) { return; }
        
        BoardSaveData boardSaveData = m_board.GetBoardSaveData();
        string path = Path.Combine(PATH, $"{m_inputField.text}.json");
        string jsonData = JsonUtility.ToJson(boardSaveData);
        File.WriteAllText(path, jsonData);
        OnDisable();
        OnEnable();
    }

    public void OnLoadPressed()
    {
        if (m_inputField.text == string.Empty) { return; }
        
        string path = Path.Combine(PATH, $"{m_inputField.text}.json");
        if (!File.Exists(path)) { return; }

        string jsonData = File.ReadAllText(path);
        BoardSaveData boardSaveData = JsonUtility.FromJson<BoardSaveData>(jsonData);
        m_board.LoadBoardSaveData(boardSaveData);
    }

    public void OnDeletePressed()
    {
        if (m_inputField.text == string.Empty) { return; }
        
        string path = Path.Combine(PATH, $"{m_inputField.text}.json");
        if (!File.Exists(path)) { return; }
        
        File.Delete(path);
        OnDisable();
        OnEnable();
    }

    private void OnEnable()
    {
        BoardNameEntry.OnEntryPress += OnEntryPressed;
        string[] files = Directory.GetFiles(PATH);
        int count = files.Length;
        for (int i = 0; i < count; ++i)
        {
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
