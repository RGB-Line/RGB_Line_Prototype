using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;

using Newtonsoft.Json;


public class MenuPopupManager : MonoBehaviour
{
    private static MenuPopupManager m_instance;


    [SerializeField] private MenuPopup_MusicDetail m_menuPopup_MusicDetail;

    [SerializeField] private TMP_InputField m_inputField_TargetStageName;


    public void Awake()
    {
        m_instance = this;
    }

    internal static MenuPopupManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    internal string TargetStageName
    {
        get
        {
            return m_inputField_TargetStageName.text;
        }
    }

    public void OpenPopup()
    {
        gameObject.SetActive(true);
    }
    public void ClosePopup()
    {
        gameObject.SetActive(false);
    }

    public void LoadStageData()
    {
        if (string.IsNullOrEmpty(m_inputField_TargetStageName.text))
        {
            Debug.LogError("Title is empty");
            return;
        }
        
        // Check Stage Data
        if (!File.Exists(DataPathInterface.GetStageDataPath(m_inputField_TargetStageName.text, MenuPopup_MusicDetails_DifficultyItem.CurDifficulty)))
        {
            m_menuPopup_MusicDetail.LoadMusicStateText.text = "No Stage File";
            return;
        }

        // Check Music File
        string musicFileExtension = string.Empty;
        if(File.Exists(DataPathInterface.GetStageMusicPath(m_inputField_TargetStageName.text, ".mp3")))
        {
            musicFileExtension = ".mp3";
        }
        else if (File.Exists(DataPathInterface.GetStageMusicPath(m_inputField_TargetStageName.text, ".wav")))
        {
            musicFileExtension = ".wav";
        }
        else
        {
            m_menuPopup_MusicDetail.LoadMusicStateText.text = "No Music File";
            return;
        }

        // Check Stage Meta Data
        if (!File.Exists(DataPathInterface.GetStageMetaDataPath(m_inputField_TargetStageName.text, MenuPopup_MusicDetails_DifficultyItem.CurDifficulty)))
        {
            m_menuPopup_MusicDetail.LoadMusicStateText.text = "No Meta File";
            return;
        }

        // Load Datas
        {
            string path = DataPathInterface.GetStageDataPath(m_inputField_TargetStageName.text, MenuPopup_MusicDetails_DifficultyItem.CurDifficulty);

            string jsonData = File.ReadAllText(path);
            StageDataBuffer.Instance.CurStageData = JsonConvert.DeserializeObject<StageData>(jsonData);
        }
        {
            string path = DataPathInterface.GetStageMetaDataPath(m_inputField_TargetStageName.text, MenuPopup_MusicDetails_DifficultyItem.CurDifficulty);
            string jsonData = File.ReadAllText(path);
            StageMetaDataBuffer.Instance.CurStageMetadata = JsonConvert.DeserializeObject<StageMetadata>(jsonData);
        }
        {
            m_menuPopup_MusicDetail.TargetMusicFilePath = DataPathInterface.GetStageMusicPath(m_inputField_TargetStageName.text, musicFileExtension);
            StartCoroutine(m_menuPopup_MusicDetail.LoadMusicCoroutine());
        }
    }
    public void SaveStageData()
    {
        if (string.IsNullOrEmpty(m_inputField_TargetStageName.text) ||
            !StageDataBuffer.Instance.CurStageData.HasValue ||
            !StageMetaDataBuffer.Instance.CurStageMetadata.HasValue)
        {
            return;
        }

        {
            string path = DataPathInterface.GetStageDataPath(m_inputField_TargetStageName.text, StageMetaDataBuffer.Instance.CurStageMetadata.Value.Difficulty);

            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, JsonConvert.SerializeObject(StageDataBuffer.Instance.CurStageData.Value));
        }
        {
            StartCoroutine(m_menuPopup_MusicDetail.SaveMusicFile());
        }
        {
            string path = DataPathInterface.GetStageMetaDataPath(m_inputField_TargetStageName.text, StageMetaDataBuffer.Instance.CurStageMetadata.Value.Difficulty);

            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(path, JsonConvert.SerializeObject(StageMetaDataBuffer.Instance.CurStageMetadata.Value));
        }

        m_menuPopup_MusicDetail.LoadMusicStateText.text = "Save Success";
    }
}