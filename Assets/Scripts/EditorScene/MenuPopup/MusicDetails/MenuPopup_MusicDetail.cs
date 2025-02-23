using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;


public class MenuPopup_MusicDetail : MonoBehaviour
{
    [Serializable] public struct LoadPathUI
    {
        [Header("Music File Path")]
        public TMP_InputField m_inputField_MusicFilePath;
        public TMP_Text m_text_LoadMusicState;

        [Header("Profile File Path")]
        public TMP_InputField m_inputField_ProfilePath;
        public Button m_button_LoadProfile;
    }
    [Serializable] public struct MusicDetailUI
    {
        [Header("Profile Image")]
        public Image m_image_Profile;
        public Sprite m_sprite_DefaultProfile;

        [Header("Music Details")]
        public TMP_InputField m_inputField_MusicTitle;
        public TMP_InputField m_inputField_MusicArtist;
        public TMP_InputField m_inputField_MusicLength;
        public TMP_InputField m_inputField_MusicDescription;

        [Header("Music Genre")]
        public MenuPopup_MusicDetails_GenreItem[] m_item_Genres;

        [Header("Music Difficulty")]
        public Slider m_slider_Difficulty;

        [Header("Stage Config Data")]
        public TMP_InputField m_inputField_BPM;
        public TMP_InputField m_inputField_BitSubDivision;
        public TMP_InputField m_inputField_LengthPerBit;
        public TMP_InputField m_inputField_MusicStartOffsetTime;
    }


    [SerializeField] private LoadPathUI m_loadPathUI;
    [SerializeField] private MusicDetailUI m_musicDetailUI;

    private string m_targetMusicFilePath;
    private string m_targetMusicFileExtension;


    public void Start()
    {
        Setup2MusicUnLoaded();
    }

    #region Properties
    internal string MusicTitle
    {
        get
        {
            return m_musicDetailUI.m_inputField_MusicTitle.text;
        }
    }

    internal string TargetMusicFilePath
    {
        set
        {
            m_targetMusicFilePath = value;
        }
    }
    internal string TargetMusicFileExtention
    {
        get
        {
            return m_targetMusicFileExtension;
        }
    }
    internal TMP_Text LoadMusicStateText
    {
        get
        {
            return m_loadPathUI.m_text_LoadMusicState;
        }
    }
    #endregion

    #region Unity UI Callbacks
    public void LoadMusic()
    {
        if (string.IsNullOrEmpty(m_loadPathUI.m_inputField_MusicFilePath.text))
        {
            m_loadPathUI.m_text_LoadMusicState.text = "Music File Path is Empty";
            return;
        }

        m_targetMusicFilePath = m_loadPathUI.m_inputField_MusicFilePath.text;

        try
        {
            StartCoroutine(LoadMusicCoroutine());
        }
        catch
        {
            Setup2MusicUnLoaded();
        }
    }
    internal IEnumerator SaveMusicFile()
    {
        string path = DataPathInterface.GetStageMusicPath(MenuPopupManager.Instance.TargetStageName, TargetMusicFileExtention);

        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // try-catch is needed
        try
        {
            File.Copy(m_targetMusicFilePath, path, true);
        }
        catch { }

        yield return null;
    }

    internal IEnumerator LoadMusicCoroutine()
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(m_targetMusicFilePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            switch (www.result)
            {
                case UnityWebRequest.Result.InProgress:
                    m_loadPathUI.m_text_LoadMusicState.text = "Music File Loading...";
                    break;

                case UnityWebRequest.Result.Success:
                    m_loadPathUI.m_text_LoadMusicState.text = "Music File Load Success";

                    m_targetMusicFileExtension = Path.GetExtension(m_targetMusicFilePath);

                    using (DownloadHandlerAudioClip downloadHandler = (DownloadHandlerAudioClip)www.downloadHandler)
                    {
                        ChiefGameManager.Instance.CurMusicClip = downloadHandler.audioClip;
                    }

                    if (!StageDataBuffer.Instance.CurStageData.HasValue)
                    {
                        StageDataBuffer.Instance.CurStageData = new StageData()
                        {
                            RegionDataTable = new Dictionary<Guid, StageData.RegionData>(),
                            LineDataTable = new Dictionary<Guid, StageData.LineData>(),
                            NoteDataTable = new Dictionary<Guid, StageData.NoteData>(),

                            StageConfig = new StageData.StageConfigData()
                            {
                                BPM = 120,
                                BitSubDivision = 4,
                                LengthPerBit = 10f,
                                MusicStartOffsetTime = 0f
                            }
                        };
                    }
                    if (!StageMetaDataBuffer.Instance.CurStageMetadata.HasValue)
                    {
                        StageMetaDataBuffer.Instance.CurStageMetadata = new StageMetadata()
                        {
                            Length = ChiefGameManager.Instance.CurMusicClip.length,
                            Difficulty = 0
                        };
                    }

                    Setup2MusicLoaded();
                    RenderUI();
                    break;

                case UnityWebRequest.Result.ConnectionError:
                    m_loadPathUI.m_text_LoadMusicState.text = "Connection Error";
                    break;

                case UnityWebRequest.Result.ProtocolError:
                    m_loadPathUI.m_text_LoadMusicState.text = "Protocol Error";
                    break;
            }
        }
    }

    public void LoadProfile()
    {
        if (string.IsNullOrEmpty(m_loadPathUI.m_inputField_ProfilePath.text))
        {
            return;
        }

        byte[] bytes = File.ReadAllBytes(m_loadPathUI.m_inputField_ProfilePath.text);
        Texture2D texture = new Texture2D(512, 512);
        texture.LoadImage(bytes);
        m_musicDetailUI.m_image_Profile.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        StageMetaDataBuffer.Instance.CurStageMetadata = new StageMetadata()
        {
            ProfileImageBytes = bytes,
            Title = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Title,
            Length = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Length,
            Artist = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Artist,
            //Description = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Description,
            //Genre = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Genre,
            Difficulty = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Difficulty,
            BestScore = StageMetaDataBuffer.Instance.CurStageMetadata.Value.BestScore
        };
    }

    public void SetMusicTitle()
    {
        if (string.IsNullOrEmpty(m_musicDetailUI.m_inputField_MusicTitle.text) ||
            !StageMetaDataBuffer.Instance.CurStageMetadata.HasValue)
        {
            return;
        }

        StageMetaDataBuffer.Instance.CurStageMetadata = new StageMetadata()
        {
            ProfileImageBytes = StageMetaDataBuffer.Instance.CurStageMetadata.Value.ProfileImageBytes,
            Title = m_musicDetailUI.m_inputField_MusicTitle.text,
            Length = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Length,
            Artist = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Artist,
            //Description = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Description,
            //Genre = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Genre,
            Difficulty = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Difficulty,
            BestScore = StageMetaDataBuffer.Instance.CurStageMetadata.Value.BestScore
        };
    }
    public void SetMusicArtist()
    {
        if (string.IsNullOrEmpty(m_musicDetailUI.m_inputField_MusicArtist.text) ||
            !StageMetaDataBuffer.Instance.CurStageMetadata.HasValue)
        {
            return;
        }

        StageMetaDataBuffer.Instance.CurStageMetadata = new StageMetadata()
        {
            ProfileImageBytes = StageMetaDataBuffer.Instance.CurStageMetadata.Value.ProfileImageBytes,
            Title = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Title,
            Length = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Length,
            Artist = m_musicDetailUI.m_inputField_MusicArtist.text,
            //Description = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Description,
            //Genre = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Genre,
            Difficulty = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Difficulty,
            BestScore = StageMetaDataBuffer.Instance.CurStageMetadata.Value.BestScore
        };
    }
    public void SetMusicDescription()
    {
        if (string.IsNullOrEmpty(m_musicDetailUI.m_inputField_MusicDescription.text) ||
            !StageMetaDataBuffer.Instance.CurStageMetadata.HasValue)
        {
            return;
        }

        StageMetaDataBuffer.Instance.CurStageMetadata = new StageMetadata()
        {
            ProfileImageBytes = StageMetaDataBuffer.Instance.CurStageMetadata.Value.ProfileImageBytes,
            Title = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Title,
            Length = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Length,
            Artist = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Artist,
            //Description = m_musicDetailUI.m_inputField_MusicDescription.text,
            //Genre = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Genre,
            Difficulty = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Difficulty,
            BestScore = StageMetaDataBuffer.Instance.CurStageMetadata.Value.BestScore
        };
    }
    public void SetMusicDifficulty()
    {
        //if (!StageMetaDataBuffer.Instance.CurStageMetadata.HasValue)
        //{
        //    return;
        //}

        //StageMetaDataBuffer.Instance.CurStageMetadata = new StageMetadata()
        //{
        //    ProfileImageBytes = StageMetaDataBuffer.Instance.CurStageMetadata.Value.ProfileImageBytes,
        //    Title = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Title,
        //    Length = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Length,
        //    Artist = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Artist,
        //    //Description = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Description,
        //    //Genre = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Genre,
        //    //Difficulty = (StageMetadata.DifficultyLevel)m_musicDetailUI.m_slider_Difficulty.value,
        //    Difficulty = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Difficulty,
        //    BestScore = StageMetaDataBuffer.Instance.CurStageMetadata.Value.BestScore
        //};
    }

    public void SetBMP()
    {
        if (!StageDataBuffer.Instance.CurStageData.HasValue)
        {
            return;
        }

        if (int.TryParse(m_musicDetailUI.m_inputField_BPM.text, out int bpm))
        {
            StageDataBuffer.Instance.CurStageData = new StageData()
            {
                RegionDataTable = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable,
                LineDataTable = StageDataBuffer.Instance.CurStageData.Value.LineDataTable,
                NoteDataTable = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable,
                StageConfig = new StageData.StageConfigData()
                {
                    BPM = bpm,
                    BitSubDivision = StageDataBuffer.Instance.CurStageData.Value.StageConfig.BitSubDivision,
                    LengthPerBit = StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit,
                    MusicStartOffsetTime = StageDataBuffer.Instance.CurStageData.Value.StageConfig.MusicStartOffsetTime
                }
            };

            GridRenderManager.Instance.RenderGrid();
            RegionEditScreenManager.Instance.Render();
            LineEditScreenManager.Instance.Render();
            NoteEditScreenManager.Instance.Render();
        }
        else
        {
            m_musicDetailUI.m_inputField_BPM.text = StageDataBuffer.Instance.CurStageData.Value.StageConfig.BPM.ToString();
        }
    }
    public void SetBitSubDivision()
    {
        if (!StageDataBuffer.Instance.CurStageData.HasValue)
        {
            return;
        }

        //if(StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Count > 0 ||
        //    StageDataBuffer.Instance.CurStageData.Value.LineDataTable.Count > 0)
        //{
        //    LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "Can't change bit sub division property while some region or line item's exist");
        //    return;
        //}

        if (int.TryParse(m_musicDetailUI.m_inputField_BitSubDivision.text, out int bitSubDivision))
        {
            // Check if bit sub division is power of 2
            if (bitSubDivision > 0 && (bitSubDivision & (bitSubDivision - 1)) == 0)
            {

            }

            StageDataBuffer.Instance.CurStageData = new StageData()
            {
                RegionDataTable = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable,
                LineDataTable = StageDataBuffer.Instance.CurStageData.Value.LineDataTable,
                NoteDataTable = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable,
                StageConfig = new StageData.StageConfigData()
                {
                    BPM = StageDataBuffer.Instance.CurStageData.Value.StageConfig.BPM,
                    BitSubDivision = bitSubDivision,
                    LengthPerBit = StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit,
                    MusicStartOffsetTime = StageDataBuffer.Instance.CurStageData.Value.StageConfig.MusicStartOffsetTime
                }
            };

            GridRenderManager.Instance.RenderGrid();
            RegionEditScreenManager.Instance.Render();
            LineEditScreenManager.Instance.Render();
            NoteEditScreenManager.Instance.Render();
        }
        else
        {
            m_musicDetailUI.m_inputField_BitSubDivision.text = StageDataBuffer.Instance.CurStageData.Value.StageConfig.BitSubDivision.ToString();
        }
    }
    public void SetLengthPerBit()
    {
        if (!StageDataBuffer.Instance.CurStageData.HasValue)
        {
            return;
        }

        if (float.TryParse(m_musicDetailUI.m_inputField_LengthPerBit.text, out float lengthPerBit))
        {
            StageDataBuffer.Instance.CurStageData = new StageData()
            {
                RegionDataTable = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable,
                LineDataTable = StageDataBuffer.Instance.CurStageData.Value.LineDataTable,
                NoteDataTable = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable,
                StageConfig = new StageData.StageConfigData()
                {
                    BPM = StageDataBuffer.Instance.CurStageData.Value.StageConfig.BPM,
                    BitSubDivision = StageDataBuffer.Instance.CurStageData.Value.StageConfig.BitSubDivision,
                    LengthPerBit = lengthPerBit,
                    MusicStartOffsetTime = StageDataBuffer.Instance.CurStageData.Value.StageConfig.MusicStartOffsetTime
                }
            };

            GridRenderManager.Instance.RenderGrid();
            RegionEditScreenManager.Instance.Render();
            LineEditScreenManager.Instance.Render();
            NoteEditScreenManager.Instance.Render();
        }
        else
        {
            m_musicDetailUI.m_inputField_LengthPerBit.text = StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit.ToString();
        }
    }
    public void SetMusicStartOffsetTime()
    {
        if (!StageDataBuffer.Instance.CurStageData.HasValue)
        {
            return;
        }

        if (float.TryParse(m_musicDetailUI.m_inputField_MusicStartOffsetTime.text, out float musicStartOffsetTime))
        {
            StageDataBuffer.Instance.CurStageData = new StageData()
            {
                RegionDataTable = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable,
                LineDataTable = StageDataBuffer.Instance.CurStageData.Value.LineDataTable,
                NoteDataTable = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable,
                StageConfig = new StageData.StageConfigData()
                {
                    BPM = StageDataBuffer.Instance.CurStageData.Value.StageConfig.BPM,
                    BitSubDivision = StageDataBuffer.Instance.CurStageData.Value.StageConfig.BitSubDivision,
                    LengthPerBit = StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit,
                    MusicStartOffsetTime = musicStartOffsetTime
                }
            };
        }
        else
        {
            m_musicDetailUI.m_inputField_MusicStartOffsetTime.text = StageDataBuffer.Instance.CurStageData.Value.StageConfig.MusicStartOffsetTime.ToString();
        }
    }
    #endregion

    #region Render UI
    internal void RenderUI()
    {
        if (!StageMetaDataBuffer.Instance.CurStageMetadata.HasValue)
        {
            return;
        }

        if (StageMetaDataBuffer.Instance.CurStageMetadata.Value.ProfileImageBytes != null)
        {
            Texture2D texture = new Texture2D(512, 512);
            texture.LoadImage(StageMetaDataBuffer.Instance.CurStageMetadata.Value.ProfileImageBytes);
            m_musicDetailUI.m_image_Profile.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            m_musicDetailUI.m_image_Profile.sprite = m_musicDetailUI.m_sprite_DefaultProfile;
        }

        if (!string.IsNullOrEmpty(StageMetaDataBuffer.Instance.CurStageMetadata.Value.Title))
        {
            m_musicDetailUI.m_inputField_MusicTitle.text = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Title;
        }
        if (!string.IsNullOrEmpty(StageMetaDataBuffer.Instance.CurStageMetadata.Value.Artist))
        {
            m_musicDetailUI.m_inputField_MusicArtist.text = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Artist;
        }
        m_musicDetailUI.m_inputField_MusicLength.text = string.Format("{0}:{1}", (int)StageMetaDataBuffer.Instance.CurStageMetadata.Value.Length / 60, (int)StageMetaDataBuffer.Instance.CurStageMetadata.Value.Length % 60);
        //if (!string.IsNullOrEmpty(StageMetaDataBuffer.Instance.CurStageMetadata.Value.Description))
        //{
        //    m_musicDetailUI.m_inputField_MusicDescription.text = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Description;
        //}

        //for (int genreButtonIndex = 0; genreButtonIndex < m_musicDetailUI.m_item_Genres.Length; genreButtonIndex++)
        //{
        //    m_musicDetailUI.m_item_Genres[genreButtonIndex].SetSelectedState(StageMetaDataBuffer.Instance.CurStageMetadata.Value.Genre.HasFlag((StageMetadata.MusicGenre)(0b0000_0001 << genreButtonIndex)));
        //}

        //m_musicDetailUI.m_slider_Difficulty.value = (int)StageMetaDataBuffer.Instance.CurStageMetadata.Value.Difficulty;

        if (StageDataBuffer.Instance.CurStageData.HasValue)
        {
            m_musicDetailUI.m_inputField_BPM.text = StageDataBuffer.Instance.CurStageData.Value.StageConfig.BPM.ToString();
            m_musicDetailUI.m_inputField_BitSubDivision.text = StageDataBuffer.Instance.CurStageData.Value.StageConfig.BitSubDivision.ToString();
            m_musicDetailUI.m_inputField_LengthPerBit.text = StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit.ToString();
            m_musicDetailUI.m_inputField_MusicStartOffsetTime.text = StageDataBuffer.Instance.CurStageData.Value.StageConfig.MusicStartOffsetTime.ToString();
        }

        GridRenderManager.Instance.RenderGrid();
        RegionEditScreenManager.Instance.Render();
        LineEditScreenManager.Instance.Render();
        Invoke("RenderNoteScreen", 1.0f);
    }
    #endregion

    #region Utils
    private void Setup2MusicUnLoaded()
    {
        ChiefGameManager.Instance.Setup2MusicUnLoaded();

        m_targetMusicFilePath = string.Empty;

        m_loadPathUI.m_inputField_ProfilePath.interactable = false;
        m_loadPathUI.m_button_LoadProfile.interactable = false;

        m_musicDetailUI.m_image_Profile.sprite = m_musicDetailUI.m_sprite_DefaultProfile;

        m_musicDetailUI.m_inputField_MusicTitle.interactable = false;
        m_musicDetailUI.m_inputField_MusicArtist.interactable = false;
        m_musicDetailUI.m_inputField_MusicLength.interactable = false;
        m_musicDetailUI.m_inputField_MusicDescription.interactable = false;

        m_musicDetailUI.m_inputField_MusicTitle.text = string.Empty;
        m_musicDetailUI.m_inputField_MusicArtist.text = string.Empty;
        m_musicDetailUI.m_inputField_MusicLength.text = string.Empty;
        //m_musicDetailUI.m_inputField_MusicDescription.text = string.Empty;

        //for (int genreButtonIndex = 0; genreButtonIndex < m_musicDetailUI.m_item_Genres.Length; genreButtonIndex++)
        //{
        //    m_musicDetailUI.m_item_Genres[genreButtonIndex].SetSelectedState(false);
        //    m_musicDetailUI.m_item_Genres[genreButtonIndex].SetInteractable(false);
        //}

        //m_musicDetailUI.m_slider_Difficulty.interactable = false;
        //m_musicDetailUI.m_slider_Difficulty.value = m_musicDetailUI.m_slider_Difficulty.minValue;

        m_musicDetailUI.m_inputField_BPM.interactable = false;
        m_musicDetailUI.m_inputField_BitSubDivision.interactable = false;
        m_musicDetailUI.m_inputField_LengthPerBit.interactable = false;
        m_musicDetailUI.m_inputField_MusicStartOffsetTime.interactable = false;

        m_musicDetailUI.m_inputField_BPM.text = string.Empty;
        m_musicDetailUI.m_inputField_BitSubDivision.text = string.Empty;
        m_musicDetailUI.m_inputField_LengthPerBit.text = string.Empty;
        m_musicDetailUI.m_inputField_MusicStartOffsetTime.text = string.Empty;
    }
    internal void Setup2MusicLoaded()
    {
        ChiefGameManager.Instance.Setup2MusicLoaded();

        m_loadPathUI.m_inputField_ProfilePath.interactable = true;
        m_loadPathUI.m_button_LoadProfile.interactable = true;

        m_musicDetailUI.m_inputField_MusicTitle.interactable = true;
        m_musicDetailUI.m_inputField_MusicArtist.interactable = true;
        m_musicDetailUI.m_inputField_MusicLength.interactable = true;
        m_musicDetailUI.m_inputField_MusicDescription.interactable = true;

        //for (int genreButtonIndex = 0; genreButtonIndex < m_musicDetailUI.m_item_Genres.Length; genreButtonIndex++)
        //{
        //    m_musicDetailUI.m_item_Genres[genreButtonIndex].SetInteractable(true);
        //}

        //m_musicDetailUI.m_slider_Difficulty.interactable = true;

        m_musicDetailUI.m_inputField_BPM.interactable = true;
        m_musicDetailUI.m_inputField_BitSubDivision.interactable = true;
        m_musicDetailUI.m_inputField_LengthPerBit.interactable = true;
        m_musicDetailUI.m_inputField_MusicStartOffsetTime.interactable = true;
    }

    private void RenderNoteScreen()
    {
        NoteEditScreenManager.Instance.Render();
    }
    #endregion
}