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

    [SerializeField] private Transform m_transform_Tracker;
    private Dictionary<Guid, float> regionTimeTable = new Dictionary<Guid, float>();


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

            Guid curRegionID = Guid.Empty;
            foreach (Guid regionID in regionTimeTable.Keys)
            {
                if (m_transform_Tracker.position.y >= regionTimeTable[regionID])
                {
                    curRegionID = regionID;
                }
            }

            if (curRegionID != Guid.Empty)
            {
                if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[curRegionID].CurColorType == StageData.RegionData.ColorType.Red)
                {
                    Guid redLineID = Guid.Empty;
                    foreach (Guid lineID in StageDataBuffer.Instance.CurStageData.Value.LineDataTable.Keys)
                    {
                        if (StageDataBuffer.Instance.CurStageData.Value.LineDataTable[lineID].AttachedRegionID == curRegionID)
                        {
                            redLineID = lineID;
                            break;
                        }
                    }

                    float xPos = GetCameraXPos(m_transform_Tracker.position.y / GridRenderManager.Instance.GetUnitFramePosition(), redLineID);
                    m_rigidbody2D_MainCamera.position = new Vector2(xPos, m_rigidbody2D_MainCamera.position.y);

                    m_transform_Tracker.gameObject.SetActive(true);
                    m_transform_Tracker.localPosition = new Vector3()
                    {
                        x = 0.0f,
                        y = -3.5f,
                        z = 5.0f
                    };
                }
                else if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[curRegionID].CurColorType == StageData.RegionData.ColorType.Green)
                {
                    Guid greenLineID = Guid.Empty;
                    foreach (Guid lineID in StageDataBuffer.Instance.CurStageData.Value.LineDataTable.Keys)
                    {
                        if (StageDataBuffer.Instance.CurStageData.Value.LineDataTable[lineID].AttachedRegionID == curRegionID)
                        {
                            greenLineID = lineID;
                            break;
                        }
                    }

                    m_transform_Tracker.gameObject.SetActive(true);
                    m_transform_Tracker.localPosition = new Vector3()
                    {
                        x = GetCameraXPos(m_transform_Tracker.position.y / GridRenderManager.Instance.GetUnitFramePosition(), greenLineID),
                        y = -3.5f,
                        z = 5.0f
                    };
                }
                else if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[curRegionID].CurColorType == StageData.RegionData.ColorType.Blue)
                {
                    m_transform_Tracker.gameObject.SetActive(false);
                    m_transform_Tracker.localPosition = new Vector3()
                    {
                        x = 0.0f,
                        y = -3.5f,
                        z = 5.0f
                    };
                }
            }

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
            regionTimeTable.Clear();
            foreach (Guid regionID in StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys)
            {
                regionTimeTable.Add(regionID, GridRenderManager.Instance.GetFramePosition(StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[regionID].StartOffsetFrame + StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[regionID].MinorOffsetTime));
                //Debug.Log($"Region ID: {regionID}, Time: {regionTimeTable[regionID]}");
            }

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

    private float GetCameraXPos(in float targetFrame, in Guid lineID)
    {
        float NoteYPos = GridRenderManager.Instance.GetFramePosition(targetFrame);
        //Debug.Log("NoteYPos : " + NoteYPos);

        List<int> nearestLinePosIndexes = new List<int>(2);
        LineRenderer attachedLineRenderer = LineEditScreenManager.Instance.GetLineItem(lineID).LineRenderer;

        //// For Blue Line
        //if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[attachedLineID].AttachedRegionID].CurColorType == StageData.RegionData.ColorType.Blue)
        //{
        //    return attachedLineRenderer.GetPosition(0).x;
        //}

        //for (int index = 0; index < attachedLineRenderer.positionCount; index++)
        //{
        //    if (attachedLineRenderer.GetPosition(index).y == NoteYPos)
        //    {
        //        return attachedLineRenderer.GetPosition(index).x;
        //    }
        //}

        // Most nearest line pos
        int curNearestLinePosIndex = -1;
        float curNearestLinePos = float.MaxValue;
        for (int linePosIndex = 0; linePosIndex < attachedLineRenderer.positionCount; linePosIndex++)
        {
            float linePos = attachedLineRenderer.GetPosition(linePosIndex).y;
            if (linePos > NoteYPos && Mathf.Abs(linePos - NoteYPos) < Mathf.Abs(curNearestLinePos - NoteYPos))
            {
                curNearestLinePosIndex = linePosIndex;
                curNearestLinePos = linePos;
            }
        }
        nearestLinePosIndexes.Add(curNearestLinePosIndex);

        if (Mathf.Abs(attachedLineRenderer.GetPosition(nearestLinePosIndexes[0]).y - NoteYPos) == 0)
        {
            return attachedLineRenderer.GetPosition(curNearestLinePosIndex).x;
        }

        // Second nearest line pos
        curNearestLinePosIndex = -1;
        curNearestLinePos = float.MaxValue;
        for (int linePosIndex = 0; linePosIndex < attachedLineRenderer.positionCount; linePosIndex++)
        {
            if (nearestLinePosIndexes.Contains(linePosIndex))
            {
                continue;
            }

            float linePos = attachedLineRenderer.GetPosition(linePosIndex).y;
            if (linePos < NoteYPos && Mathf.Abs(linePos - NoteYPos) < Mathf.Abs(curNearestLinePos - NoteYPos))
            {
                curNearestLinePosIndex = linePosIndex;
                curNearestLinePos = linePos;
            }
        }
        nearestLinePosIndexes.Add(curNearestLinePosIndex);

        // Mathf.Lerp를 사용하기 위한 준비
        float[] nearestLinePosGaps = new float[2];
        for (int index = 0; index < nearestLinePosIndexes.Count; index++)
        {
            nearestLinePosGaps[index] = MathF.Abs(attachedLineRenderer.GetPosition(nearestLinePosIndexes[index]).y - NoteYPos);
        }

        return Mathf.Lerp(attachedLineRenderer.GetPosition(nearestLinePosIndexes[0]).x,
                          attachedLineRenderer.GetPosition(nearestLinePosIndexes[1]).x,
                          (nearestLinePosGaps[0] / (nearestLinePosGaps[0] + nearestLinePosGaps[1])));
    }
    #endregion
}