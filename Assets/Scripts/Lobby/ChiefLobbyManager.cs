using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

using Newtonsoft.Json;
using static MenuPopup_MusicDetail;
using UnityEngine.Networking;


public class ChiefLobbyManager : MonoBehaviour
{
    public enum MaxFrameRate
    {
        Frame_30,
        Frame_60,
        Frame_90,
        Frame_120
    }

    private enum ChangeMusicDirection
    {
        Init,
        Difficulty,
        Left, Right
    }

    [Serializable] public struct MainLayoutUIs
    {
        public GameObject m_gameObject_MainLayout;

        public TMP_Text m_text_MusicTitle;
        public TMP_Text m_text_ArtistName;

        public TMP_Text m_text_Difficulty;
        public TMP_Text m_text_BestScore;
    }
    [Serializable] public struct SettingLayoutUIs
    {
        public GameObject m_gameObject_SettingLayout;

        [Header("Music Bolume")]
        public Slider m_slider_MusicVolume;
        public TMP_Text m_text_MusicVolume;

        [Header("Max Frame Rate")]
        public Slider m_slider_MaxFrameRate;
        public TMP_Text m_text_MaxFrameRate;

        [Header("VSync Count")]
        public Slider m_slider_VSyncCount;
        public TMP_Text m_text_VSyncCount;
    }


    [Header("UIs")]
    [SerializeField] private MainLayoutUIs m_mainLayoutUIs;
    [SerializeField] private SettingLayoutUIs m_settingLayoutUIs;

    [Header("For Background")]
    [SerializeField] private RegionTransitionEffectManager m_regionTransitionEffectManager;
    [SerializeField] private Transform m_transform_Background;

    [SerializeField] private float m_leftButtonRatio = 0.11f;

    //[Header("Game Config Data")]
    //[SerializeField] private GameConfigData m_gameConfigData;

    [Header("Prefabs")]
    [SerializeField] private GameObject m_prefab_StageLoadInfoDTO;

    private AudioSource m_audioSource;
    private string m_targetMusicFilePath;

    private int m_selectedMatIndex;

    private string[] m_stageNames;
    private int m_curSelectedStageIndex = 0;

    private StageMetadata.DifficultyLevel m_curDifficulty = StageMetadata.DifficultyLevel.Easy;


    public void Awake()
    {
        m_audioSource = GetComponent<AudioSource>();
    }
    public void Start()
    {
        LoadStageNames();
        GameConfigDataBuffer.Instance.LoadGameConfigData();

        if (!File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], m_curDifficulty)))
        {
            if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Easy)))
            {
                m_curDifficulty = StageMetadata.DifficultyLevel.Easy;
            }
            else if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Normal)))
            {
                m_curDifficulty = StageMetadata.DifficultyLevel.Normal;
            }
            else if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Hard)))
            {
                m_curDifficulty = StageMetadata.DifficultyLevel.Hard;
            }
        }

        m_targetMusicFilePath = DataPathInterface.GetStageMusicPath(m_stageNames[m_curSelectedStageIndex], ".mp3");
        StartCoroutine(LoadMusic());

        DisplayStageMetadata();
        CallRegionTransitionEffect(ChangeMusicDirection.Init);

        RenderSetting();
    }
    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Change2PrevMusic();
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Change2NextMusic();
        }

        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            ChangeDifficulty(true);
        }
        else if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangeDifficulty(false);
        }
    }

    #region Change Music
    public void Change2PrevMusic()
    {
        if (m_curSelectedStageIndex - 1 >= 0)
        {
            m_curSelectedStageIndex--;
        }
        else
        {
            m_curSelectedStageIndex = m_stageNames.Length - 1;
        }

        m_targetMusicFilePath = DataPathInterface.GetStageMusicPath(m_stageNames[m_curSelectedStageIndex], ".mp3");
        StartCoroutine(LoadMusic());

        DisplayStageMetadata();
        CallRegionTransitionEffect(ChangeMusicDirection.Left);
    }
    public void Change2NextMusic()
    {
        if (m_curSelectedStageIndex + 1 < m_stageNames.Length)
        {
            m_curSelectedStageIndex++;
        }
        else
        {
            m_curSelectedStageIndex = 0;
        }

        m_targetMusicFilePath = DataPathInterface.GetStageMusicPath(m_stageNames[m_curSelectedStageIndex], ".mp3");
        StartCoroutine(LoadMusic());

        DisplayStageMetadata();
        CallRegionTransitionEffect(ChangeMusicDirection.Right);
    }

    public void ChangeDifficulty(bool bisUpperDirection)
    {
        switch(m_curDifficulty)
        {
            case StageMetadata.DifficultyLevel.Easy:
                //m_curDifficulty = StageMetadata.DifficultyLevel.Normal;
                if(bisUpperDirection)
                {
                    if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Normal)))
                    {
                        m_curDifficulty = StageMetadata.DifficultyLevel.Normal;
                    }
                    else if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Hard)))
                    {
                        m_curDifficulty = StageMetadata.DifficultyLevel.Hard;
                    }
                    else if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Easy)))
                    {
                        m_curDifficulty = StageMetadata.DifficultyLevel.Easy;
                    }
                }
                else
                {
                    if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Hard)))
                    {
                        m_curDifficulty = StageMetadata.DifficultyLevel.Hard;
                    }
                    else if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Normal)))
                    {
                        m_curDifficulty = StageMetadata.DifficultyLevel.Normal;
                    }
                    else if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Easy)))
                    {
                        m_curDifficulty = StageMetadata.DifficultyLevel.Easy;
                    }
                }
                break;

            case StageMetadata.DifficultyLevel.Normal:
                //m_curDifficulty = StageMetadata.DifficultyLevel.Hard;
                if (bisUpperDirection)
                {
                    if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Hard)))
                    {
                        m_curDifficulty = StageMetadata.DifficultyLevel.Hard;
                    }
                    else if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Easy)))
                    {
                        m_curDifficulty = StageMetadata.DifficultyLevel.Easy;
                    }
                    else if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Normal)))
                    {
                        m_curDifficulty = StageMetadata.DifficultyLevel.Normal;
                    }
                }
                else
                {
                    if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Easy)))
                    {
                        m_curDifficulty = StageMetadata.DifficultyLevel.Easy;
                    }
                    else if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Hard)))
                    {
                        m_curDifficulty = StageMetadata.DifficultyLevel.Hard;
                    }
                    else if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Normal)))
                    {
                        m_curDifficulty = StageMetadata.DifficultyLevel.Normal;
                    }
                }
                break;

            case StageMetadata.DifficultyLevel.Hard:
                //m_curDifficulty = StageMetadata.DifficultyLevel.Easy;
                if (bisUpperDirection)
                {
                    if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Easy)))
                    {
                        m_curDifficulty = StageMetadata.DifficultyLevel.Easy;
                    }
                    else if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Normal)))
                    {
                        m_curDifficulty = StageMetadata.DifficultyLevel.Normal;
                    }
                    else if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Hard)))
                    {
                        m_curDifficulty = StageMetadata.DifficultyLevel.Hard;
                    }
                }
                else
                {
                    if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Normal)))
                    {
                        m_curDifficulty = StageMetadata.DifficultyLevel.Normal;
                    }
                    else if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Easy)))
                    {
                        m_curDifficulty = StageMetadata.DifficultyLevel.Easy;
                    }
                    else if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Hard)))
                    {
                        m_curDifficulty = StageMetadata.DifficultyLevel.Hard;
                    }
                }
                break;
        }

        DisplayStageMetadata();
        CallRegionTransitionEffect(ChangeMusicDirection.Difficulty);
    }

    private void CallRegionTransitionEffect(in ChangeMusicDirection changeMusicDirection)
    {
        switch (StageMetaDataBuffer.Instance.CurStageMetadata.Value.Difficulty)
        {
            case StageMetadata.DifficultyLevel.Easy:
                m_selectedMatIndex = 1;
                break;

            case StageMetadata.DifficultyLevel.Normal:
                m_selectedMatIndex = 2;
                break;

            case StageMetadata.DifficultyLevel.Hard:
                m_selectedMatIndex = 0;
                break;
        }

        float effectStartXPos = 0.0f;
        switch(changeMusicDirection)
        {
            case ChangeMusicDirection.Init:
                LobbyBackgroundManager.Instance.RandomMatSelectIndex = m_selectedMatIndex;
                effectStartXPos = 0.5f;
                break;

            case ChangeMusicDirection.Difficulty:
                effectStartXPos = 0.5f;
                break;

            case ChangeMusicDirection.Left:
                effectStartXPos = m_leftButtonRatio;
                break;

            case ChangeMusicDirection.Right:
                effectStartXPos = (1.0f - m_leftButtonRatio);
                break;

        }
        m_regionTransitionEffectManager.StartTransition(m_selectedMatIndex,
                                                        new Vector2()
                                                        {
                                                            x = effectStartXPos,
                                                            y = 0.5f
                                                        });

        Invoke("Apply2LobbyBackgroundManager", RegionTransitionEffectManager.Instance.Timeout);
    }
    private void Apply2LobbyBackgroundManager()
    {
        LobbyBackgroundManager.Instance.RandomMatSelectIndex = m_selectedMatIndex;
    }

    private IEnumerator LoadMusic()
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(m_targetMusicFilePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            switch (www.result)
            {
                case UnityWebRequest.Result.Success:
                    using (DownloadHandlerAudioClip downloadHandler = (DownloadHandlerAudioClip)www.downloadHandler)
                    {
                        m_audioSource.clip = downloadHandler.audioClip;
                        m_audioSource.Play();
                    }
                    break;
            }
        }
    }
    #endregion

    #region Load Stage Metadatas
    private void LoadStageNames()
    {
        m_stageNames = DataPathInterface.GetStageNames();
    }

    private void DisplayStageMetadata()
    {
        string path = string.Empty;
        if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], m_curDifficulty)))
        {
            path = DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], m_curDifficulty);
        }
        else
        {
            if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Easy)))
            {
                m_curDifficulty = StageMetadata.DifficultyLevel.Easy;
            }
            else if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Normal)))
            {
                m_curDifficulty = StageMetadata.DifficultyLevel.Normal;
            }
            else if (File.Exists(DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], StageMetadata.DifficultyLevel.Hard)))
            {
                m_curDifficulty = StageMetadata.DifficultyLevel.Hard;
            }
            path = DataPathInterface.GetStageMetaDataPath(m_stageNames[m_curSelectedStageIndex], m_curDifficulty);
        }
        string jsonData = File.ReadAllText(path);
        StageMetaDataBuffer.Instance.CurStageMetadata = JsonConvert.DeserializeObject<StageMetadata>(jsonData);

        m_mainLayoutUIs.m_text_MusicTitle.text = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Title;
        m_mainLayoutUIs.m_text_ArtistName.text = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Artist;

        string difficulty = string.Empty;
        switch (StageMetaDataBuffer.Instance.CurStageMetadata.Value.Difficulty)
        {
            case StageMetadata.DifficultyLevel.Easy:
                difficulty = "Easy";
                break;

            case StageMetadata.DifficultyLevel.Normal:
                difficulty = "Normal";
                break;

            case StageMetadata.DifficultyLevel.Hard:
                difficulty = "Hard";
                break;
        }
        m_mainLayoutUIs.m_text_Difficulty.text = difficulty;
        m_mainLayoutUIs.m_text_BestScore.text = "Best Score - " + StageMetaDataBuffer.Instance.CurStageMetadata.Value.BestScore.ToString();
    }
    #endregion

    #region Setting Layout
    public void OpenSettingLayout()
    {
        if(m_settingLayoutUIs.m_gameObject_SettingLayout.activeSelf)
        {
            m_mainLayoutUIs.m_gameObject_MainLayout.SetActive(true);
            m_settingLayoutUIs.m_gameObject_SettingLayout.SetActive(false);

            GameConfigDataBuffer.Instance.SaveGameConfigData();
        }
        else
        {
            m_mainLayoutUIs.m_gameObject_MainLayout.SetActive(false);
            m_settingLayoutUIs.m_gameObject_SettingLayout.SetActive(true);
        }
    }

    public void SetMusicVolumeAdjustment()
    {
        GameConfigDataBuffer.Instance.GameConfigData = new GameConfigData()
        {
            noteHitJudgingStrandard = GameConfigDataBuffer.Instance.GameConfigData.Value.noteHitJudgingStrandard,
            MusicVolume = m_settingLayoutUIs.m_slider_MusicVolume.value,
            MaxFrameRate = GameConfigDataBuffer.Instance.GameConfigData.Value.MaxFrameRate,
            VSyncCount = GameConfigDataBuffer.Instance.GameConfigData.Value.VSyncCount
        };

        RenderMusicVolume();
    }
    public void SetSync()
    {

    }
    public void SetMaxFrameRate()
    {
        GameConfigDataBuffer.Instance.GameConfigData = new GameConfigData()
        {
            noteHitJudgingStrandard = GameConfigDataBuffer.Instance.GameConfigData.Value.noteHitJudgingStrandard,
            MusicVolume = GameConfigDataBuffer.Instance.GameConfigData.Value.MusicVolume,
            MaxFrameRate = (MaxFrameRate)m_settingLayoutUIs.m_slider_MaxFrameRate.value,
            VSyncCount = GameConfigDataBuffer.Instance.GameConfigData.Value.VSyncCount
        };

        RenderMaxFrameRate();
    }
    public void SetVSyncCount()
    {
        GameConfigDataBuffer.Instance.GameConfigData = new GameConfigData()
        {
            noteHitJudgingStrandard = GameConfigDataBuffer.Instance.GameConfigData.Value.noteHitJudgingStrandard,
            MusicVolume = GameConfigDataBuffer.Instance.GameConfigData.Value.MusicVolume,
            MaxFrameRate = GameConfigDataBuffer.Instance.GameConfigData.Value.MaxFrameRate,
            VSyncCount = (int)m_settingLayoutUIs.m_slider_VSyncCount.value
        };

        RenderVSyncCount();
    }

    private void RenderSetting()
    {
        RenderMusicVolume();
        RenderMaxFrameRate();
        RenderVSyncCount();
    }
    private void RenderMusicVolume()
    {
        float musicVolume = GameConfigDataBuffer.Instance.GameConfigData.Value.MusicVolume;

        m_settingLayoutUIs.m_slider_MusicVolume.value = musicVolume;
        m_settingLayoutUIs.m_text_MusicVolume.text = ((int)(musicVolume * 100.0f)).ToString();
    }
    private void RenderMaxFrameRate()
    {
        MaxFrameRate maxFrameRate = GameConfigDataBuffer.Instance.GameConfigData.Value.MaxFrameRate;

        m_settingLayoutUIs.m_slider_MaxFrameRate.value = (int)maxFrameRate;
        switch (maxFrameRate)
        {
            case MaxFrameRate.Frame_30:
                m_settingLayoutUIs.m_text_MaxFrameRate.text = "30";
                Application.targetFrameRate = 30;
                break;

            case MaxFrameRate.Frame_60:
                m_settingLayoutUIs.m_text_MaxFrameRate.text = "60";
                Application.targetFrameRate = 60;
                break;

            case MaxFrameRate.Frame_90:
                m_settingLayoutUIs.m_text_MaxFrameRate.text = "90";
                Application.targetFrameRate = 90;
                break;

            case MaxFrameRate.Frame_120:
                m_settingLayoutUIs.m_text_MaxFrameRate.text = "120";
                Application.targetFrameRate = 120;
                break;
        }
    }
    private void RenderVSyncCount()
    {
        int vSyncCount = GameConfigDataBuffer.Instance.GameConfigData.Value.VSyncCount;

        m_settingLayoutUIs.m_slider_VSyncCount.value = vSyncCount;
        switch (vSyncCount)
        {
            case 0:
                m_settingLayoutUIs.m_text_VSyncCount.text = "Off";
                break;

            case 1:
            case 2:
            case 3:
                m_settingLayoutUIs.m_text_VSyncCount.text = vSyncCount.ToString();
                break;

            case 4:
                m_settingLayoutUIs.m_text_VSyncCount.text = "Full";
                break;
        }

        QualitySettings.vSyncCount = vSyncCount;
    }
    #endregion

    #region Main Layout
    public void StartStage()
    {
        GameObject stageLoadInfoDTO = Instantiate(m_prefab_StageLoadInfoDTO);
        stageLoadInfoDTO.GetComponent<StageLoadInfoDTO>().StageName = m_stageNames[m_curSelectedStageIndex];

        DontDestroyOnLoad(stageLoadInfoDTO);

        UnityEngine.SceneManagement.SceneManager.LoadScene("MainGame");
    }
    #endregion
}