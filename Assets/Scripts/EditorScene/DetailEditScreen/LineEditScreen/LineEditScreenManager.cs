using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class LineEditScreenManager : MonoBehaviour
{
    internal enum LineEditScreenSelectModeType
    {
        None,
        LineItem,
        LinePointItem
    }

    [Serializable] public struct StartAndEndOffsetFrameUI
    {
        public TMP_Text m_text_TotalFrameCount;
        public TMP_InputField m_inputField_StartOffsetFrame;
        public TMP_InputField m_inputField_EndOffsetFrame;
    }
    [Serializable] public struct LineWidthUI
    {
        public TMP_InputField m_inputField_LineWidth;
    }


    private static LineEditScreenManager m_instance;

    [Header("UIs")]
    [SerializeField] private TMP_Text m_text_LineSelectItem;
    //[SerializeField] private Slider m_slider_SelectLineItem;
    [SerializeField] private StartAndEndOffsetFrameUI m_startAndEndOffsetFrameUI;
    [SerializeField] private TMP_Text m_text_LinePointSelectItem;
    [SerializeField] private LineWidthUI m_lineWidthUI;

    [Header("Line Item")]
    [SerializeField] private Transform m_transform_LineItemParent;
    [SerializeField] private GameObject m_prefab_LineItem;

    private LineEditScreenSelectModeType m_selectedMode = LineEditScreenSelectModeType.None;
    private Guid m_selectedLineID;

    private Stack<Editor_LineItem> m_lineItems = new Stack<Editor_LineItem>();


    public void Awake()
    {
        m_instance = this;
        gameObject.SetActive(false);

        if(m_transform_LineItemParent.childCount > 0)
        {
            for(int childIndex = m_transform_LineItemParent.childCount - 1; childIndex >= 0; childIndex--)
            {
                Destroy(m_transform_LineItemParent.GetChild(childIndex).gameObject);
            }
        }
    }
    public void Update()
    {
        if (DetailEditScreenManager.Instance.CurDetailEditScreenType == DetailEditScreenManager.DetailEditScreenType.Line)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SelectLineitem();
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                SelectLinePointItem();
            }
        }
        //else
        //{
        //    m_selectedMode = LineEditScreenSelectModeType.None;

        //    m_text_LineSelectItem.text = "Cur Line Select Mode : False [Q]";
        //    m_text_LineSelectItem.color = Color.white;

        //    m_text_LinePointSelectItem.text = "Cur Line Point Select Mode : False [W]";
        //    m_text_LinePointSelectItem.color = Color.white;
        //}
    }

    internal static LineEditScreenManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    internal Guid SelectedLineID
    {
        get
        {
            return m_selectedLineID;
        }
        set
        {
            m_selectedLineID = value;

            Render();
        }
    }
    internal LineEditScreenSelectModeType SelectedMode
    {
        get
        {
            return m_selectedMode;
        }
    }

    #region Unity UI Callbacks
    // LineItemUI
    public void AddLineItem()
    {
        if (m_selectedMode != LineEditScreenSelectModeType.None)
        {
            return;
        }

        if(RegionEditScreenManager.Instance.SelectedRegionID == Guid.Empty)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "There's no currently selected region item");
            return;
        }

        if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[RegionEditScreenManager.Instance.SelectedRegionID].CurColorType != StageData.RegionData.ColorType.Blue)
        {
            if(GetCurRegionLineCount() >= 1)
            {
                LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "You can't add more line in that region");
                return;
            }
        }

        Guid lineID = Guid.NewGuid();
        List<HalfFloatVector2> defaultCurvedLinePoints = new List<HalfFloatVector2>();

        float startXPos = 0;
        if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.ToList().IndexOf(RegionEditScreenManager.Instance.SelectedRegionID) - 1 >= 0)
        {
            int prevRegionIndex = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.ToList().IndexOf(RegionEditScreenManager.Instance.SelectedRegionID) - 1;
            Guid prevRegionID = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.ElementAt(prevRegionIndex);

            foreach (var lineItem in StageDataBuffer.Instance.CurStageData.Value.LineDataTable.Values)
            {
                if (lineItem.AttachedRegionID == prevRegionID)
                {
                    startXPos = lineItem.CurvedLinePoints.Last().X;
                    break;
                }
            }
        }

        float endXPos = 0;
        if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.ToList().IndexOf(RegionEditScreenManager.Instance.SelectedRegionID) + 1 < StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Count)
        {
            int nextRegionIndex = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.ToList().IndexOf(RegionEditScreenManager.Instance.SelectedRegionID) + 1;
            Guid nextRegionID = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.ElementAt(nextRegionIndex);
            foreach (var lineItem in StageDataBuffer.Instance.CurStageData.Value.LineDataTable.Values)
            {
                if (lineItem.AttachedRegionID == nextRegionID)
                {
                    endXPos = lineItem.CurvedLinePoints.First().X;
                    break;
                }
            }
        }

        defaultCurvedLinePoints.Add(new HalfFloatVector2()
        {
            X = startXPos,
            Y = 0
        });
        defaultCurvedLinePoints.Add(new HalfFloatVector2()
        {
            X = endXPos,
            Y = GetSelectedRegionMaxFrameCount()
        });

        List<float> minorOffsetTimes = new List<float>()
        {
            0.0f,
            0.0f
        };

        StageDataBuffer.Instance.CurStageData.Value.LineDataTable.Add(lineID, new StageData.LineData
        {
            AttachedRegionID = RegionEditScreenManager.Instance.SelectedRegionID,
            //StartOffsetFrame = 0,
            //EndOffsetFrame = 1,
            CurvedLinePoints = defaultCurvedLinePoints,
            MinorOffsetTimes = minorOffsetTimes,
            LineWidth = 0.1f,
            CurLineSmoothType = GetLineSmoothTypeBasedRegionColor(StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[RegionEditScreenManager.Instance.SelectedRegionID].CurColorType)
        });

        m_selectedLineID = lineID;

        Render();
    }
    public void RemoveLineItem()
    {
        if (m_selectedLineID == Guid.Empty)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "There's no currently selected line item");
            return;
        }

        if (m_selectedMode != LineEditScreenSelectModeType.None)
        {
            return;
        }
        if(GetCurRegionLineCount() - 1 < 1)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "You can't remove more line in that region");
            return;
        }

        foreach(var noteItem in StageDataBuffer.Instance.CurStageData.Value.NoteDataTable.Values)
        {
            if (noteItem.AttachedLineID == m_selectedLineID)
            {
                LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "You can't remove that line item because there's note item attached to that line item");
                return;
            }
        }

        StageDataBuffer.Instance.CurStageData.Value.LineDataTable.Remove(m_selectedLineID);
        m_selectedLineID = Guid.Empty;

        Render();
    }
    //public void SelectLineItem()
    //{
    //    if((int)m_slider_SelectLineItem.value >= StageDataBuffer.Instance.CurStageData.Value.LineDataTable.Count)
    //    {
    //        return;
    //    }

    //    m_selectedLineID = StageDataBuffer.Instance.CurStageData.Value.LineDataTable.Keys.ElementAt((int)m_slider_SelectLineItem.value);

    //    Render();
    //}

    // StartAndEndOffsetFrameUI
    public void SetStartOffsetFrame()
    {
        //if (m_selectedLineID == Guid.Empty)
        //{
        //    LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "There's no currently selected line item");
        //    return;
        //}

        //bool bisSetable = true;
        //if (m_selectedMode != LineEditScreenSelectModeType.None)
        //{
        //    bisSetable = false;
        //}

        //int startOffsetFrame = int.MinValue;
        //int.TryParse(m_startAndEndOffsetFrameUI.m_inputField_StartOffsetFrame.text, out startOffsetFrame);
        //if (startOffsetFrame < 0)
        //{
        //    startOffsetFrame = 0;
        //}
        //else if (startOffsetFrame >= StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].EndOffsetFrame)
        //{
        //    startOffsetFrame = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].EndOffsetFrame - 1;
        //}

        //if (bisSetable)
        //{
        //    StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID] = new StageData.LineData()
        //    {
        //        AttachedRegionID = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].AttachedRegionID,
        //        //StartOffsetFrame = startOffsetFrame,
        //        //EndOffsetFrame = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].EndOffsetFrame,
        //        CurvedLinePoints = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].CurvedLinePoints,
        //        LineWidth = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].LineWidth,
        //        CurLineSmoothType = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].CurLineSmoothType
        //    };

        //    Render();
        //}
        //else
        //{
        //    m_startAndEndOffsetFrameUI.m_inputField_StartOffsetFrame.text = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].StartOffsetFrame.ToString();
        //}
    }
    public void SetEndOffsetFrame()
    {
        //if (m_selectedLineID == Guid.Empty)
        //{
        //    LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "There's no currently selected line item");
        //    return;
        //}

        //bool bisSetable = true;
        //if (m_selectedMode != LineEditScreenSelectModeType.None)
        //{
        //    bisSetable = false;
        //}

        //int endOffsetFrame = int.MinValue;
        //int.TryParse(m_startAndEndOffsetFrameUI.m_inputField_EndOffsetFrame.text, out endOffsetFrame);
        //if (endOffsetFrame <= StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].StartOffsetFrame)
        //{
        //    endOffsetFrame = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].StartOffsetFrame + 1;
        //}
        //else if (endOffsetFrame >= GetCurLineMaxFrameCount())
        //{
        //    endOffsetFrame = GetCurLineMaxFrameCount() - 1;
        //}

        //if (bisSetable)
        //{
        //    StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID] = new StageData.LineData()
        //    {
        //        AttachedRegionID = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].AttachedRegionID,
        //        //StartOffsetFrame = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].StartOffsetFrame,
        //        //EndOffsetFrame = endOffsetFrame,
        //        CurvedLinePoints = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].CurvedLinePoints,
        //        LineWidth = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].LineWidth,
        //        CurLineSmoothType = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].CurLineSmoothType
        //    };
        //    Render();
        //}
        //else
        //{
        //    m_startAndEndOffsetFrameUI.m_inputField_EndOffsetFrame.text = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].EndOffsetFrame.ToString();
        //}
    }

    // LineWidthUI
    public void AddLinePointItem()
    {
        if (m_selectedLineID == Guid.Empty)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "There's no currently selected line item");
            return;
        }

        if (m_selectedMode != LineEditScreenSelectModeType.None)
        {
            return;
        }

        if (StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].CurvedLinePoints.Count > 0 &&
            StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].CurvedLinePoints.Last().Y == GetCurLineMaxFrameCount() - 1)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Warning, "The last line point is already at the end of the line");
            return;
        }
        if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[RegionEditScreenManager.Instance.SelectedRegionID].CurColorType == StageData.RegionData.ColorType.Blue)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "You can't add more line point in blue region");
        }

        int realSelectedPointIndex = GetLineItem(m_selectedLineID).SelectedPointIndex;
        if(StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].AttachedRegionID].CurColorType == StageData.RegionData.ColorType.Green)
        {
            realSelectedPointIndex--;
        }

        if (realSelectedPointIndex == StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].CurvedLinePoints.Count - 1)
        {
            Debug.Log("1");
            return;
        }
        else if (realSelectedPointIndex + 1 < StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].CurvedLinePoints.Count &&
            StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].CurvedLinePoints[realSelectedPointIndex].Y + 1 ==
            StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].CurvedLinePoints[realSelectedPointIndex + 1].Y)
        {
            Debug.Log("2");
            return;
        }
        int defailtYPos = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].CurvedLinePoints[realSelectedPointIndex].Y + 1;

        HalfFloatVector2 defaultLinePointPos = new HalfFloatVector2()
        {
            X = 0,
            Y = defailtYPos
        };

        StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].CurvedLinePoints.Insert(realSelectedPointIndex + 1, defaultLinePointPos);
        StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].MinorOffsetTimes.Insert(realSelectedPointIndex + 1, 0.0f);

        Render();
    }
    public void RemoveLinePointItem()
    {
        if (m_selectedLineID == Guid.Empty)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "There's no currently selected line item");
            return;
        }

        if (m_selectedMode != LineEditScreenSelectModeType.None)
        {
            return;
        }
        if(GetCurLinePointCount() - 1 < 2)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "You can't remove more line point in that line");
        }

        int realSelectedPointIndex = GetLineItem(m_selectedLineID).SelectedPointIndex;
        if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].AttachedRegionID].CurColorType == StageData.RegionData.ColorType.Green)
        {
            realSelectedPointIndex--;
        }

        StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].CurvedLinePoints.RemoveAt(realSelectedPointIndex);
        StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].MinorOffsetTimes.RemoveAt(realSelectedPointIndex);

        Render();
    }

    // LineWidthUI
    public void SetLineWidth()
    {
        if (m_selectedLineID == Guid.Empty)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "There's no currently selected line item");
            return;
        }

        bool bisSetable = true;
        if (m_selectedMode != LineEditScreenSelectModeType.None)
        {
            bisSetable = false;
        }

        float lineWidth = float.MinValue;
        float.TryParse(m_lineWidthUI.m_inputField_LineWidth.text, out lineWidth);
        if (lineWidth <= 0)
        {
            lineWidth = 0.1f;
        }

        if (bisSetable)
        {
            StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID] = new StageData.LineData()
            {
                AttachedRegionID = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].AttachedRegionID,
                //StartOffsetFrame = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].StartOffsetFrame,
                //EndOffsetFrame = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].EndOffsetFrame,
                CurvedLinePoints = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].CurvedLinePoints,
                MinorOffsetTimes = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].MinorOffsetTimes,
                LineWidth = lineWidth,
                CurLineSmoothType = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].CurLineSmoothType
            };
            Render();
        }
        else
        {
            m_lineWidthUI.m_inputField_LineWidth.text = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].LineWidth.ToString();
        }
    }
    #endregion

    #region Utils
    internal void Render()
    {
        while (true)
        {
            if (m_lineItems.Count == 0)
            {
                break;
            }

            Editor_LineItem lineItem = m_lineItems.Pop();
            lineItem.Dispose();
            Destroy(lineItem.gameObject);
        }

        //m_slider_SelectLineItem.maxValue = StageDataBuffer.Instance.CurStageData.Value.LineDataTable.Count - 1;

        foreach (Guid lineID in StageDataBuffer.Instance.CurStageData.Value.LineDataTable.Keys)
        {
            Editor_LineItem newLineItem = Instantiate(m_prefab_LineItem, m_transform_LineItemParent).GetComponent<Editor_LineItem>();
            newLineItem.RenderLineItem(lineID);
            m_lineItems.Push(newLineItem);
        }

        if (m_selectedLineID != Guid.Empty)
        {
            m_startAndEndOffsetFrameUI.m_text_TotalFrameCount.text = "Cur Line Total Frame Count : " + (GetCurLineMaxFrameCount() - 1).ToString();

            //m_startAndEndOffsetFrameUI.m_inputField_StartOffsetFrame.text = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].StartOffsetFrame.ToString();
            //m_startAndEndOffsetFrameUI.m_inputField_EndOffsetFrame.text = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].EndOffsetFrame.ToString();

            m_lineWidthUI.m_inputField_LineWidth.text = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].LineWidth.ToString();
        }
        else
        {
            m_startAndEndOffsetFrameUI.m_text_TotalFrameCount.text = "Cur Line Total Frame Count : 0";

            m_startAndEndOffsetFrameUI.m_inputField_StartOffsetFrame.text = string.Empty;
            m_startAndEndOffsetFrameUI.m_inputField_EndOffsetFrame.text = string.Empty;

            m_lineWidthUI.m_inputField_LineWidth.text = string.Empty;
        }
    }

    internal void ClearSelectMode()
    {
        m_selectedMode = LineEditScreenSelectModeType.None;

        m_text_LineSelectItem.text = "Cur Line Select Mode : False [Q]";
        m_text_LineSelectItem.color = Color.white;

        m_text_LinePointSelectItem.text = "Cur Line Point Select Mode : False [W]";
        m_text_LinePointSelectItem.color = Color.white;
    }

    internal StageData.LineData.LineSmoothType GetLineSmoothTypeBasedRegionColor(in StageData.RegionData.ColorType colorType)
    {
        switch (colorType)
        {
            case StageData.RegionData.ColorType.Red:
                return StageData.LineData.LineSmoothType.Linear;

            case StageData.RegionData.ColorType.Green:
                return StageData.LineData.LineSmoothType.Curved;

            case StageData.RegionData.ColorType.Blue:
                return StageData.LineData.LineSmoothType.Linear;

            default:
                throw new Exception("Invalid Color Type");
        }
    }

    internal int GetCurLineMaxFrameCount()
    {
        return GetLineMaxFrameCount(m_selectedLineID);
    }
    internal int GetLineMaxFrameCount(in Guid targetLineID)
    {
        int attachedRegionIndex = RegionEditScreenManager.Instance.GetRegionIndex(StageDataBuffer.Instance.CurStageData.Value.LineDataTable[targetLineID].AttachedRegionID);
        int curLineTotalFrameCount = int.MinValue;
        if (attachedRegionIndex + 1 < StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Count)
        {
            curLineTotalFrameCount = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Values.ElementAt(attachedRegionIndex + 1).StartOffsetFrame - StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Values.ElementAt(attachedRegionIndex).StartOffsetFrame;
        }
        else
        {
            curLineTotalFrameCount = GridRenderManager.Instance.GetTotalBitCount() - StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Values.ElementAt(attachedRegionIndex).StartOffsetFrame;
        }

        return curLineTotalFrameCount;
    }

    internal Editor_LineItem GetLineItem(in Guid lineID)
    {
        foreach (Editor_LineItem lineItem in m_lineItems)
        {
            if (lineItem.LineID == lineID)
            {
                return lineItem;
            }
        }

        return null;
    }

    private void SelectLineitem()
    {
        if (m_selectedMode != LineEditScreenSelectModeType.LineItem)
        {
            m_selectedMode = LineEditScreenSelectModeType.LineItem;
            m_text_LineSelectItem.text = "Cur Line Select Mode : True [Q]";
            m_text_LineSelectItem.color = new Color(199 / 255.0f, 125 / 255.0f, 72 / 255.0f);

            //m_slider_SelectLineItem.interactable = true;

            m_text_LinePointSelectItem.text = "Cur Line Point Select Mode : False [W]";
            m_text_LinePointSelectItem.color = Color.white;
        }
        else
        {
            m_selectedMode = LineEditScreenSelectModeType.None;
            m_text_LineSelectItem.text = "Cur Line Select Mode : False [Q]";
            m_text_LineSelectItem.color = Color.white;

            //m_slider_SelectLineItem.interactable = false;
        }
    }
    private void SelectLinePointItem()
    {
        if (m_selectedMode != LineEditScreenSelectModeType.LinePointItem)
        {
            m_selectedMode = LineEditScreenSelectModeType.LinePointItem;
            m_text_LinePointSelectItem.text = "Cur Line Point Select Mode : True [W]";
            m_text_LinePointSelectItem.color = new Color(199 / 255.0f, 125 / 255.0f, 72 / 255.0f);

            m_text_LineSelectItem.text = "Cur Line Select Mode : False [Q]";
            m_text_LineSelectItem.color = Color.white;

            //m_slider_SelectLineItem.interactable = false;
        }
        else
        {
            m_selectedMode = LineEditScreenSelectModeType.None;
            m_text_LinePointSelectItem.text = "Cur Line Point Select Mode : False [W]";
            m_text_LinePointSelectItem.color = Color.white;

            //m_slider_SelectLineItem.interactable = false;
        }
    }

    private int GetCurRegionLineCount()
    {
        int curRegionLineCount = 0;
        foreach (var lineItem in StageDataBuffer.Instance.CurStageData.Value.LineDataTable.Values)
        {
            if (lineItem.AttachedRegionID == RegionEditScreenManager.Instance.SelectedRegionID)
            {
                curRegionLineCount++;
            }
        }

        return curRegionLineCount;
    }
    private int GetCurLinePointCount()
    {
        return StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_selectedLineID].CurvedLinePoints.Count;
    }

    private int GetSelectedRegionMaxFrameCount()
    {
        int selectedRegionIndex = RegionEditScreenManager.Instance.GetRegionIndex(RegionEditScreenManager.Instance.SelectedRegionID);
        int curLineTotalFrameCount = int.MinValue;
        if (selectedRegionIndex + 1 < StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Count)
        {
            curLineTotalFrameCount = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Values.ElementAt(selectedRegionIndex + 1).StartOffsetFrame - StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Values.ElementAt(selectedRegionIndex).StartOffsetFrame;
        }
        else
        {
            curLineTotalFrameCount = GridRenderManager.Instance.GetTotalBitCount() - StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Values.ElementAt(selectedRegionIndex).StartOffsetFrame;
        }

        return curLineTotalFrameCount;
    }
    #endregion
}