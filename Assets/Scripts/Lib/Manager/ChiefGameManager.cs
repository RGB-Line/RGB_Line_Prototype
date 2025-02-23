using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


public sealed class ChiefGameManager : MonoBehaviour
{
    [Serializable]
    public struct OuterUI
    {
        public Button m_button_PlayAndPause;
        public Slider m_slider_TimeMover;

        public Button m_button_MoveScreen;

        public Button m_button_RegionMode;
        public Button m_button_LineMode;
        public Button m_button_NoteMode;
    }


    private static ChiefGameManager m_instance;

    private AudioSource m_audioSource;
    private bool m_bisMusicPlaying = false;

    [SerializeField] private OuterUI m_outerUI;
    [SerializeField] private Rigidbody2D m_rigidbody2D_MainCamera;

    [Header("Editor Mode State")]
    [SerializeField] private bool m_bisEditorMode = false;

    //[Header("Game Config Data")]
    //[SerializeField] private GameConfigData m_gameConfigData;


    public void Awake()
    {
        m_instance = this;

        m_audioSource = GetComponent<AudioSource>();

        GameConfigDataBuffer.Instance.LoadGameConfigData();
    }
    public void Update()
    {
        if (m_bisMusicPlaying)
        {
            m_outerUI.m_slider_TimeMover.value = m_audioSource.time / (m_audioSource.clip.length - StageDataBuffer.Instance.CurStageData.Value.StageConfig.MusicStartOffsetTime);

            if (m_audioSource.time >= m_audioSource.clip.length)
            {
                m_audioSource.Stop();
                m_rigidbody2D_MainCamera.velocity = Vector2.zero;

                LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Common, "Music Ended");
            }
        }

        if(Input.mouseScrollDelta.y != 0.0f)
        {
            m_outerUI.m_slider_TimeMover.value += Input.mouseScrollDelta.y * 0.001f;

            AdjestMainCameraPosManually();
        }
    }

    internal static ChiefGameManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    internal AudioClip CurMusicClip
    {
        get
        {
            return m_audioSource.clip;
        }
        set
        {
            if(value == null)
            {
                Debug.LogError("Music Clip is null");
                return;
            }
            m_audioSource.clip = value;
        }
    }

    internal bool BIsMusicPlaying
    {
        get
        {
            return m_bisMusicPlaying;
        }
    }
    internal bool BIsEditorMode
    {
        get
        {
            return m_bisEditorMode;
        }
    }

    internal AudioSource CurAudioSource
    {
        get
        {
            return m_audioSource;
        }
    }

    internal OuterUI UI
    {
        get
        {
            return m_outerUI;
        }
    }
    internal Rigidbody2D MainCameraRigidbody2D
    {
        get
        {
            return m_rigidbody2D_MainCamera;
        }
    }

    #region Unity UI Callbacks
    public void PlayAndPauseMusic()
    {
        if (m_bisMusicPlaying)
        {
            m_audioSource.Stop();

            m_rigidbody2D_MainCamera.transform.position = new Vector3(0.0f, 0.0f, -10.0f);
            m_rigidbody2D_MainCamera.velocity = Vector2.zero;
        }
        else
        {
            m_audioSource.time = StageDataBuffer.Instance.CurStageData.Value.StageConfig.MusicStartOffsetTime;
            m_audioSource.Play();

            m_rigidbody2D_MainCamera.velocity = new Vector2(0.0f, (GridRenderManager.Instance.GetTotalBitCount() * (StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit / StageDataBuffer.Instance.CurStageData.Value.StageConfig.BitSubDivision)) / m_audioSource.clip.length);
        }

        m_bisMusicPlaying = !m_bisMusicPlaying;
    }
    public void AdjestMainCameraPosManually()
    {
        if (m_bisMusicPlaying)
        {
            return;
        }
        else if (!StageDataBuffer.Instance.CurStageData.HasValue)
        {
            return;
        }

        m_rigidbody2D_MainCamera.transform.position = new Vector3
        {
            x = 0.0f,
            y = (m_outerUI.m_slider_TimeMover.value * GridRenderManager.Instance.GetTotalBitCount())
                * (StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit / StageDataBuffer.Instance.CurStageData.Value.StageConfig.BitSubDivision),
            z = -10.0f
        };
    }
    #endregion

    #region Utils
    internal void Setup2MusicUnLoaded()
    {
        m_outerUI.m_button_PlayAndPause.interactable = false;
        m_outerUI.m_slider_TimeMover.interactable = false;
        m_outerUI.m_button_MoveScreen.interactable = false;
        m_outerUI.m_button_RegionMode.interactable = false;
        m_outerUI.m_button_LineMode.interactable = false;
        m_outerUI.m_button_NoteMode.interactable = false;
    }
    internal void Setup2MusicLoaded()
    {
        m_outerUI.m_button_PlayAndPause.interactable = true;
        m_outerUI.m_slider_TimeMover.interactable = true;
        m_outerUI.m_button_MoveScreen.interactable = true;
        m_outerUI.m_button_RegionMode.interactable = true;
        m_outerUI.m_button_LineMode.interactable = true;
        m_outerUI.m_button_NoteMode.interactable = true;
    }
    #endregion
}