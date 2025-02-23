using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class RegionEditScreenManager : MonoBehaviour
{
    [Serializable] public struct StartOffsetFrameUI
    {
        public TMP_Text m_text_TotalFrameCount;
        public TMP_InputField m_inputField_StartOffsetFrame;
        public Slider m_slider_StartOffsetFrame;
    }
    [Serializable] public struct ColorTypeUI
    {
        public TMP_Dropdown m_dropdown_ColorType;
    }


    private static RegionEditScreenManager m_instance;

    [Header("UIs")]
    [SerializeField] private TMP_Text m_text_RegionSelectItem;
    [SerializeField] private StartOffsetFrameUI m_startOffsetFrameUI;
    [SerializeField] private Slider m_slider_MinorOfsetTime;
    [SerializeField] private ColorTypeUI m_colorTypeUI;

    [Header("Region Item")]
    [SerializeField] private Transform m_transform_RegionItemParent;
    [SerializeField] private GameObject m_prefab_RegionItem;

    private bool m_bisRegionSelectMode = false;
    private Guid m_selectedRegionID;

    private Stack<Editor_RegionItem> m_regionItems = new Stack<Editor_RegionItem>();


    public void Awake()
    {
        m_instance = this;
        gameObject.SetActive(false);
    }
    public void Update()
    {
        if(DetailEditScreenManager.Instance.CurDetailEditScreenType == DetailEditScreenManager.DetailEditScreenType.Region)
        {
            if(Input.GetKeyDown(KeyCode.Q))
            {
                SelectRegionItem();
            }
        }
        //else
        //{
        //    m_bisRegionSelectMode = false;
        //    m_text_RegionSelectItem.text = "Cur Select Mode : False [Q]";
        //    m_text_RegionSelectItem.color = Color.white;
        //}
    }

    internal static RegionEditScreenManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    internal Guid SelectedRegionID
    {
        get
        {
            return m_selectedRegionID;
        }
        set
        {
            m_selectedRegionID = value;

            //Debug.Log($"Cur Region Index: {GetRegionIndex()}");

            Render();
        }
    }
    internal bool BIsRegionSelectMode
    {
        get
        {
            return m_bisRegionSelectMode;
        }
        set
        {
            m_bisRegionSelectMode = value;
        }
    }

    #region Unity UI Callbacks
    // RegionItemUI
    public void AddRegionItem()
    {
        if(m_bisRegionSelectMode)
        {
            return;
        }

        int defaultStartOffsetFrame = 0;
        if(StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Count > 0)
        {
            defaultStartOffsetFrame = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Values.Last().StartOffsetFrame + 1;
        }

        StageData.RegionData.ColorType defaultColorType = StageData.RegionData.ColorType.Blue;
        if(StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Count > 0)
        {
            switch (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Values.Last().CurColorType)
            {
                case StageData.RegionData.ColorType.Red:
                    defaultColorType = StageData.RegionData.ColorType.Green;
                    break;

                case StageData.RegionData.ColorType.Green:
                    defaultColorType = StageData.RegionData.ColorType.Blue;
                    break;

                case StageData.RegionData.ColorType.Blue:
                    defaultColorType = StageData.RegionData.ColorType.Red;
                    break;
            }
        }

        Guid regionID = Guid.NewGuid();
        StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Add(regionID, new StageData.RegionData()
        {
            StartOffsetFrame = defaultStartOffsetFrame,
            CurColorType = defaultColorType
        });

        m_selectedRegionID = regionID;

        //Debug.Log($"Add Region Item: {regionID}");

        Render();
    }
    public void RemoveRegionItem()
    {
        if (m_selectedRegionID == Guid.Empty)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "There's no currently selected region item");
            return;
        }

        if(GetRegionIndex() == 0)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "Can't remove first region item");
            return;
        }

        foreach (var lineItem in StageDataBuffer.Instance.CurStageData.Value.LineDataTable.Values)
        {
            if (lineItem.AttachedRegionID == m_selectedRegionID)
            {
                LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "There's attached line item to selected region item");
                return;
            }
        }

        if (m_bisRegionSelectMode)
        {
            return;
        }

        StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Remove(m_selectedRegionID);
        m_selectedRegionID = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.Last();

        Render();
    }
    
    // Start Offset Frame UI
    public void SetStartOffsetFrame_InputField()
    {
        if (m_selectedRegionID == Guid.Empty)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "There's no currently selected region item");
            return;
        }

        bool bisSetable = true;
        if (m_bisRegionSelectMode)
        {
            bisSetable = false;
        }

        int startOffsetFrame = int.MinValue;
        if(!int.TryParse(m_startOffsetFrameUI.m_inputField_StartOffsetFrame.text, out startOffsetFrame))
        {
            Debug.LogError("Failed to parse start offset frame");
            bisSetable = false;
        }

        if (startOffsetFrame < 0)
        {
            startOffsetFrame = 0;
        }
        else if (startOffsetFrame > GridRenderManager.Instance.GetTotalBitCount())
        {
            startOffsetFrame = GridRenderManager.Instance.GetTotalBitCount();
        }

        int curRegionIndex = GetRegionIndex();
        if (curRegionIndex == 0)
        {
            Debug.LogError("Cur Region Index is 0");
            bisSetable = false;
        }
        else if (curRegionIndex - 1 >= 0 && startOffsetFrame < StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Values.ElementAt(curRegionIndex - 1).StartOffsetFrame)
        {
            Debug.LogError("Start Offset Frame is smaller than previous region's start offset frame");
            bisSetable = false;
        }

        if(bisSetable)
        {
            StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID] = new StageData.RegionData()
            {
                StartOffsetFrame = startOffsetFrame,
                MinorOffsetTime = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID].MinorOffsetTime,
                CurColorType = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID].CurColorType
            };

            ApplyRegionStartOffsetFrameAffect();
            Render();
        }
        else
        {
            m_startOffsetFrameUI.m_inputField_StartOffsetFrame.text = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID].StartOffsetFrame.ToString();
        }
    }
    public void SetStartOffsetFrame_Slider()
    {
        if (m_selectedRegionID == Guid.Empty)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "There's no currently selected region item");
            return;
        }

        bool bisSetable = true;
        if (m_bisRegionSelectMode)
        {
            bisSetable = false;
        }

        int curRegionIndex = GetRegionIndex();
        if (curRegionIndex == 0)
        {
            bisSetable = false;
        }
        else if (curRegionIndex - 1 >= 0 && (int)m_startOffsetFrameUI.m_slider_StartOffsetFrame.value < StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Values.ElementAt(curRegionIndex - 1).StartOffsetFrame)
        {
            bisSetable = false;
        }

        if(bisSetable)
        {
            StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID] = new StageData.RegionData()
            {
                StartOffsetFrame = (int)m_startOffsetFrameUI.m_slider_StartOffsetFrame.value,
                MinorOffsetTime = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID].MinorOffsetTime,
                CurColorType = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID].CurColorType
            };

            ApplyRegionStartOffsetFrameAffect();
            Render();
        }
        else
        {
            m_startOffsetFrameUI.m_slider_StartOffsetFrame.value = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID].StartOffsetFrame;
        }
    }

    // Minor Offset Time UI
    public void SetMinorOffsetTime()
    {
        if (m_selectedRegionID == Guid.Empty)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "There's no currently selected region item");
            return;
        }

        bool bisSetable = true;
        if (m_bisRegionSelectMode)
        {
            bisSetable = false;
        }

        if (bisSetable)
        {
            StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID] = new StageData.RegionData()
            {
                StartOffsetFrame = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID].StartOffsetFrame,
                MinorOffsetTime = m_slider_MinorOfsetTime.value,
                CurColorType = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID].CurColorType
            };

            Render();
        }
        else
        {
            m_slider_MinorOfsetTime.value = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID].MinorOffsetTime;
        }
    }

    // Color Type UI
    public void SetColorType()
    {
        if (m_selectedRegionID == Guid.Empty)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "There's no currently selected region item");
            return;
        }

        bool bisSetable = true;
        if (m_bisRegionSelectMode)
        {
            bisSetable = false;
        }

        int curRegionIndex = GetRegionIndex();
        if ((curRegionIndex - 1 >= 0 && StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.ElementAt(curRegionIndex - 1).Value.CurColorType == (StageData.RegionData.ColorType)m_colorTypeUI.m_dropdown_ColorType.value) ||
           (curRegionIndex + 1 < StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Count && StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.ElementAt(curRegionIndex + 1).Value.CurColorType == (StageData.RegionData.ColorType)m_colorTypeUI.m_dropdown_ColorType.value))
        {
            bisSetable = false;
        }

        int attachedLineCount = 0;
        foreach(var lineItem in StageDataBuffer.Instance.CurStageData.Value.LineDataTable.Values)
        {
            if(lineItem.AttachedRegionID == m_selectedRegionID)
            {
                attachedLineCount++;
            }
        }
        if((StageData.RegionData.ColorType)m_colorTypeUI.m_dropdown_ColorType.value != StageData.RegionData.ColorType.Blue &&
            attachedLineCount > 1)
        {
            bisSetable = false;
        }

        if(bisSetable)
        {
            StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID] = new StageData.RegionData()
            {
                StartOffsetFrame = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID].StartOffsetFrame,
                MinorOffsetTime = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID].MinorOffsetTime,
                CurColorType = (StageData.RegionData.ColorType)m_colorTypeUI.m_dropdown_ColorType.value
            };

            Render();
        }
        else
        {
            m_colorTypeUI.m_dropdown_ColorType.value = (int)StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID].CurColorType;
        }
    }
    #endregion

    #region Utils
    internal void Render()
    {
        while (true)
        {
            if (m_regionItems.Count == 0)
            {
                break;
            }

            Editor_RegionItem regionItem = m_regionItems.Pop();
            Destroy(regionItem.gameObject);
        }

        foreach (Guid regionID in StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys)
        {
            Editor_RegionItem regionItem = Instantiate(m_prefab_RegionItem, m_transform_RegionItemParent).GetComponent<Editor_RegionItem>();
            regionItem.RenderRegionItem(regionID);
            m_regionItems.Push(regionItem);
        }

        if(m_selectedRegionID != Guid.Empty)
        {
            m_startOffsetFrameUI.m_text_TotalFrameCount.text = "Total Frame Count : " + GridRenderManager.Instance.GetTotalBitCount().ToString();

            m_startOffsetFrameUI.m_inputField_StartOffsetFrame.text = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID].StartOffsetFrame.ToString();
            m_startOffsetFrameUI.m_slider_StartOffsetFrame.maxValue = GridRenderManager.Instance.GetTotalBitCount();
            m_startOffsetFrameUI.m_slider_StartOffsetFrame.value = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID].StartOffsetFrame;

            m_slider_MinorOfsetTime.value = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID].MinorOffsetTime;

            m_colorTypeUI.m_dropdown_ColorType.value = (int)StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_selectedRegionID].CurColorType;
        }
        else
        {
            m_startOffsetFrameUI.m_text_TotalFrameCount.text = "Total Frame Count : 0";

            m_startOffsetFrameUI.m_inputField_StartOffsetFrame.text = string.Empty;
            m_startOffsetFrameUI.m_slider_StartOffsetFrame.maxValue = GridRenderManager.Instance.GetTotalBitCount();
            m_startOffsetFrameUI.m_slider_StartOffsetFrame.value = 0;

            m_slider_MinorOfsetTime.value = 0.0f;

            m_colorTypeUI.m_dropdown_ColorType.value = (int)StageData.RegionData.ColorType.Red;
        }

        LineEditScreenManager.Instance.Render();
        Invoke("RenderNoteScreen", 1.0f);
        //NoteEditScreenManager.Instance.Render();
    }
    
    internal int GetRegionIndex(in Guid regionID)
    {
        int curRegionIndex = 0;
        for (int index = 0; index < StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Count; index++)
        {
            if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.ElementAt(index).Key == regionID)
            {
                curRegionIndex = index;
                break;
            }
        }

        return curRegionIndex;
    }
    internal void ClearSelectMode()
    {
        m_bisRegionSelectMode = false;
        m_text_RegionSelectItem.text = "Cur Select Mode : False [Q]";
        m_text_RegionSelectItem.color = Color.white;
    }

    private void SelectRegionItem()
    {
        if (!m_bisRegionSelectMode)
        {
            m_bisRegionSelectMode = true;
            m_text_RegionSelectItem.text = "Cur Select Mode : True [Q]";
            m_text_RegionSelectItem.color = new Color(199 / 255.0f, 125 / 255.0f, 72 / 255.0f);
        }
        else
        {
            m_bisRegionSelectMode = false;
            m_text_RegionSelectItem.text = "Cur Select Mode : False [Q]";
            m_text_RegionSelectItem.color = Color.white;
        }
    }

    private int GetRegionIndex()
    {
        return GetRegionIndex(m_selectedRegionID);
    }

    private void ApplyRegionStartOffsetFrameAffect()
    {
        foreach(Guid lineID in StageDataBuffer.Instance.CurStageData.Value.LineDataTable.Keys)
        {
            if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[lineID].AttachedRegionID].CurColorType != StageData.RegionData.ColorType.Blue)
            {
                StageDataBuffer.Instance.CurStageData.Value.LineDataTable[lineID].CurvedLinePoints[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[lineID].CurvedLinePoints.Count - 1] = new HalfFloatVector2()
                {
                    X = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[lineID].CurvedLinePoints[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[lineID].CurvedLinePoints.Count - 1].X,
                    Y = LineEditScreenManager.Instance.GetLineMaxFrameCount(lineID)
                };
            }
        }
    }

    private void RenderNoteScreen()
    {
        NoteEditScreenManager.Instance.Render();
    }
    #endregion
}