using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class NoteEditScreenManager : MonoBehaviour
{
    private static NoteEditScreenManager m_instance;

    [Header("UIs")]
    [SerializeField] private TMP_Text m_text_NoteSelectItem;
    [SerializeField] private TMP_InputField m_inputField_NoteStartOffsetFrame;
    [SerializeField] private TMP_InputField m_inputField_NoteLength;
    [SerializeField] private TMP_Dropdown m_dropdown_FlipNoteDirection;
    [SerializeField] private TMP_Dropdown m_dropdown_NoteType;
    [SerializeField] private Toggle m_toggle_NoteVisiable;

    [Header("Note Item")]
    [SerializeField] private Transform m_transform_NoteItemParent;
    [SerializeField] private GameObject m_prefab_NoteItem;

    private bool m_bisNoteSelectMode = false;
    private Guid m_selectedNoteID;

    private Stack<Editor_NoteItem> m_noteItems = new Stack<Editor_NoteItem>();
    private List<Editor_Note_RedLineCurveNote> m_redLineCurvedNotes = new List<Editor_Note_RedLineCurveNote>();


    public void Awake()
    {
        m_instance = this;
        gameObject.SetActive(false);

        if (m_transform_NoteItemParent.childCount > 0)
        {
            for (int childIndex = m_transform_NoteItemParent.childCount - 1; childIndex >= 0; childIndex--)
            {
                Destroy(m_transform_NoteItemParent.GetChild(childIndex).gameObject);
            }
        }
    }
    public void Update()
    {
        if(DetailEditScreenManager.Instance.CurDetailEditScreenType == DetailEditScreenManager.DetailEditScreenType.Note)
        {
            if(Input.GetKeyDown(KeyCode.Q))
            {
                SelectNoteItem();
            }
        }
    }

    internal static NoteEditScreenManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    internal bool BIsNoteSelectMode
    {
        get
        {
            return m_bisNoteSelectMode;
        }
    }
    internal Guid SelectedNoteID
    {
        get
        {
            return m_selectedNoteID;
        }
        set
        {
            m_selectedNoteID = value;

            Render();
        }
    }

    internal List<Editor_Note_RedLineCurveNote> RedLineCurveNotes
    {
        get
        {
            return m_redLineCurvedNotes;
        }
    }

    #region Unity UI Callbacks
    public void AddNoteItem()
    {
        if(m_bisNoteSelectMode)
        {
            return;
        }

        if(LineEditScreenManager.Instance.SelectedLineID == Guid.Empty)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "Line is not selected");
            return;
        }

        Guid noteID = Guid.NewGuid();

        int startOffsetFrame = 0;
        List<int> availableStartOffsetFrame = new List<int>();
        int attachedLineFrameLength = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[LineEditScreenManager.Instance.SelectedLineID].CurvedLinePoints.Last().Y -
                                      StageDataBuffer.Instance.CurStageData.Value.LineDataTable[LineEditScreenManager.Instance.SelectedLineID].CurvedLinePoints.First().Y;
        for (int frameIndex = 0; frameIndex < attachedLineFrameLength; frameIndex++)
        {
            availableStartOffsetFrame.Add(frameIndex);
        }

        foreach (var noteItem in StageDataBuffer.Instance.CurStageData.Value.NoteDataTable.Values)
        {
            if (noteItem.AttachedLineID != LineEditScreenManager.Instance.SelectedLineID)
            {
                continue;
            }
            if (availableStartOffsetFrame.Contains(noteItem.StartOffsetFrame))
            {
                availableStartOffsetFrame.Remove(noteItem.StartOffsetFrame);
            }
        }

        availableStartOffsetFrame.Sort();
        startOffsetFrame = availableStartOffsetFrame.First();

        StageDataBuffer.Instance.CurStageData.Value.NoteDataTable.Add(noteID, new StageData.NoteData()
        {
            AttachedLineID = LineEditScreenManager.Instance.SelectedLineID,
            StartOffsetFrame = startOffsetFrame,
            MinorOffsetTime = 0.0f,
            NoteLength = 0,
            flipNoteDirection = StageData.NoteData.FlipNoteDirection.Left,
            CurNoteType = StageData.NoteData.NoteType.Common
        });

        m_selectedNoteID = noteID;

        Render();
    }
    public void RemoveNoteItem()
    {
        if (m_bisNoteSelectMode)
        {
            return;
        }

        if (m_selectedNoteID == Guid.Empty)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "There's no currently selected note item");
            return;
        }

        StageDataBuffer.Instance.CurStageData.Value.NoteDataTable.Remove(m_selectedNoteID);
        m_selectedNoteID = Guid.Empty;

        Render();
    }

    public void SetNoteStartOffsetFrame()
    {
        if (m_bisNoteSelectMode)
        {
            return;
        }

        if (m_selectedNoteID == Guid.Empty)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "There's no currently selected note item");
            return;
        }

        bool bisSetable = true;

        int startOffsetFrame = 0;
        if (!int.TryParse(m_inputField_NoteStartOffsetFrame.text, out startOffsetFrame))
        {
            bisSetable = false;
        }

        //List<int> availableStartOffsetFrame = new List<int>();
        //int attachedLineFrameLength = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].AttachedLineID].CurvedLinePoints.Last().Y -
        //                              StageDataBuffer.Instance.CurStageData.Value.LineDataTable[StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].AttachedLineID].CurvedLinePoints.First().Y;
        //for (int frameIndex = 0; frameIndex < attachedLineFrameLength; frameIndex++)
        //{
        //    availableStartOffsetFrame.Add(frameIndex);
        //}

        //foreach (var noteItem in StageDataBuffer.Instance.CurStageData.Value.NoteDataTable.Values)
        //{
        //    if (noteItem.AttachedLineID != StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].AttachedLineID)
        //    {
        //        continue;
        //    }

        //    if (noteItem.StartOffsetFrame == startOffsetFrame)
        //    {
        //        bisSetable = false;
        //        break;
        //    }
        //}

        if (bisSetable)
        {
            StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID] = new StageData.NoteData()
            {
                AttachedLineID = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].AttachedLineID,
                StartOffsetFrame = startOffsetFrame,
                MinorOffsetTime = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].MinorOffsetTime,
                NoteLength = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].NoteLength,
                flipNoteDirection = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].flipNoteDirection,
                CurNoteType = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].CurNoteType
            };

            Debug.Log("Set Note Start Offset Frame : " + startOffsetFrame);
            Render();
        }
        else
        {
            m_inputField_NoteStartOffsetFrame.text = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].StartOffsetFrame.ToString();
        }
    }
    public void SetNoteLength()
    {
        if (m_bisNoteSelectMode)
        {
            return;
        }

        if (m_selectedNoteID == Guid.Empty)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "There's no currently selected note item");
            return;
        }

        if (StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].CurNoteType != StageData.NoteData.NoteType.Long)
        {
            return;
        }

        StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID] = new StageData.NoteData()
        {
            AttachedLineID = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].AttachedLineID,
            StartOffsetFrame = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].StartOffsetFrame,
            MinorOffsetTime = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].MinorOffsetTime,
            NoteLength = int.Parse(m_inputField_NoteLength.text),
            flipNoteDirection = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].flipNoteDirection,
            CurNoteType = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].CurNoteType
        };

        Render();
    }

    public void SetFlipNoteDireciton()
    {
        if (m_bisNoteSelectMode)
        {
            return;
        }

        if (m_selectedNoteID == Guid.Empty)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "There's no currently selected note item");
            return;
        }

        StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID] = new StageData.NoteData()
        {
            AttachedLineID = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].AttachedLineID,
            StartOffsetFrame = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].StartOffsetFrame,
            MinorOffsetTime = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].MinorOffsetTime,
            NoteLength = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].NoteLength,
            flipNoteDirection = (StageData.NoteData.FlipNoteDirection)m_dropdown_FlipNoteDirection.value,
            CurNoteType = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].CurNoteType
        };

        Render();
    }
    public void SetNoteType()
    {
        if (m_bisNoteSelectMode)
        {
            return;
        }

        if (m_selectedNoteID == Guid.Empty)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "There's no currently selected note item");
            return;
        }

        StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID] = new StageData.NoteData()
        {
            AttachedLineID = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].AttachedLineID,
            StartOffsetFrame = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].StartOffsetFrame,
            MinorOffsetTime = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].MinorOffsetTime,
            NoteLength = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].NoteLength,
            flipNoteDirection = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].flipNoteDirection,
            CurNoteType = (StageData.NoteData.NoteType)m_dropdown_NoteType.value
        };

        Render();
    }

    public void SetNoteVisiable()
    {
        Render();
    }
    #endregion

    #region Utils
    internal void Render()
    {
        while (true)
        {
            if (m_noteItems.Count == 0)
            {
                break;
            }

            Editor_NoteItem noteItem = m_noteItems.Pop();
            Destroy(noteItem.gameObject);
        }

        if(m_toggle_NoteVisiable.isOn)
        {
            foreach (Guid noteID in StageDataBuffer.Instance.CurStageData.Value.NoteDataTable.Keys)
            {
                Editor_NoteItem noteItem = Instantiate(m_prefab_NoteItem, m_transform_NoteItemParent).GetComponent<Editor_NoteItem>();
                noteItem.RenderNoteItem(noteID);
                m_noteItems.Push(noteItem);
            }
        }

        if(m_selectedNoteID != Guid.Empty)
        {
            m_inputField_NoteStartOffsetFrame.text = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].StartOffsetFrame.ToString();

            m_inputField_NoteLength.text = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].NoteLength.ToString();
            if (StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].CurNoteType == StageData.NoteData.NoteType.Long)
            {
                m_inputField_NoteLength.interactable = true;
            }
            else
            {
                m_inputField_NoteLength.interactable = false;
            }

            m_dropdown_FlipNoteDirection.value = (int)StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].flipNoteDirection;
            m_dropdown_NoteType.value = (int)StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_selectedNoteID].CurNoteType;
        }
        else
        {
            m_inputField_NoteStartOffsetFrame.text = string.Empty;
            m_inputField_NoteLength.text = string.Empty;

            m_dropdown_FlipNoteDirection.value = 0;
            m_dropdown_NoteType.value = 0;
        }
    }

    internal void ClearSelectMode()
    {
        m_bisNoteSelectMode = false;
        m_text_NoteSelectItem.text = "Cur Select Mode : False [Q]";
        m_text_NoteSelectItem.color = Color.white;
    }

    internal Editor_NoteItem GetNoteItem(in Guid targetNoteID)
    {
        Editor_NoteItem result = null;
        foreach(Editor_NoteItem noteItem in m_noteItems)
        {
            if(noteItem.NoteID == targetNoteID)
            {
                result = noteItem;
            }
        }

        return result;
    }
    internal Editor_Note_RedLineCurveNote GetRedLineCurvedNote(in Guid targetNoteID)
    {
        Editor_Note_RedLineCurveNote result = null;
        foreach (Editor_Note_RedLineCurveNote noteItem in m_redLineCurvedNotes)
        {
            if (noteItem.NoteID == targetNoteID)
            {
                result = noteItem;
            }
        }

        return result;
    }

    private void SelectNoteItem()
    {
        if(!m_bisNoteSelectMode)
        {
            m_bisNoteSelectMode = true;
            m_text_NoteSelectItem.text = "Cur Select Mode : True [Q]";
            m_text_NoteSelectItem.color = new Color(199 / 255.0f, 125 / 255.0f, 72 / 255.0f);
        }
        else
        {
            m_bisNoteSelectMode = false;
            m_text_NoteSelectItem.text = "Cur Select Mode : False [Q]";
            m_text_NoteSelectItem.color = Color.white;
        }
    }
    #endregion
}