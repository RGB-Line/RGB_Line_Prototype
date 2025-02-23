using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Editor_RegionItem : MonoBehaviour
{
    private SpriteRenderer m_spriteRenderer;

    private Guid m_regionID;


    public void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnMouseDown()
    {
        if(RegionEditScreenManager.Instance.BIsRegionSelectMode)
        {
            RegionEditScreenManager.Instance.SelectedRegionID = m_regionID;

            //Debug.Log($"Selected Region ID: {m_regionID}");
        }
    }

    internal Guid RegionID
    {
        get
        {
            return m_regionID;
        }
    }

    #region Utils
    internal void RenderRegionItem(in Guid regionID)
    {
        m_regionID = regionID;

        int startFrame = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[regionID].StartOffsetFrame;
        float startMinorOffsetTime = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[regionID].MinorOffsetTime;

        int endFrame = GridRenderManager.Instance.GetTotalBitCount();
        float endMinorOffsetTime = 0.0f;
        if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Count > 1)
        {
            foreach (StageData.RegionData regionData in StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Values)
            {
                if (regionData.StartOffsetFrame > startFrame && regionData.StartOffsetFrame < endFrame)
                {
                    endFrame = regionData.StartOffsetFrame;
                    endMinorOffsetTime = regionData.MinorOffsetTime;
                }
            }
        }

        SetRegionFramePosition(startFrame, endFrame, startMinorOffsetTime, endMinorOffsetTime);
        DrawRegionColor(StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[regionID].CurColorType);
    }

    private void SetRegionFramePosition(in int startFrame, in int endFrame, in float startMinorOffsetTime, in float endMinorOffsetTime)
    {
        float realStartPos = startFrame + startMinorOffsetTime;
        float realEndPos = endFrame + endMinorOffsetTime;

        float bitSubDivision = StageDataBuffer.Instance.CurStageData.Value.StageConfig.BitSubDivision;
        bitSubDivision /= 4.0f;
        if (startFrame == 0)
        {
            transform.localScale = new Vector3(200.0f, (realEndPos - realStartPos) * StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit / 4.0f * 2.0f / bitSubDivision, 1);
            transform.position = new Vector3(0, (realStartPos + realEndPos) * StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit / (2.0f * 4.0f) / bitSubDivision - transform.localScale.y / 4.0f, 0);
        }
        else if (endFrame == GridRenderManager.Instance.GetTotalBitCount())
        {
            transform.localScale = new Vector3(200.0f, (realEndPos - realStartPos) * StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit / 4.0f * 2.0f / bitSubDivision, 1);
            transform.position = new Vector3(0, (realStartPos + realEndPos) * StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit / (2.0f * 4.0f) / bitSubDivision + transform.localScale.y / 4.0f, 0);
        }
        else
        {
            transform.localScale = new Vector3(200.0f, (realEndPos - realStartPos) * StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit / 4.0f / bitSubDivision, 1);
            transform.position = new Vector3(0, (realStartPos + realEndPos) * StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit / (2.0f * 4.0f) / bitSubDivision, 0);
        }

        if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[m_regionID].CurColorType == StageData.RegionData.ColorType.Green)
        {
            transform.position = new Vector3()
            {
                x = transform.position.x,
                y = transform.position.y,
                z = 10.0f
            };
        }
    }
    private void DrawRegionColor(in StageData.RegionData.ColorType colorType)
    {
        switch(colorType)
        {
            case StageData.RegionData.ColorType.Red:
                m_spriteRenderer.color = new Color(223 / 255.0f, 84 / 255.0f, 82 / 255.0f);
                break;

            case StageData.RegionData.ColorType.Green:
                m_spriteRenderer.color = new Color(82 / 255.0f, 158 / 255.0f, 114 / 255.0f);
                break;

            case StageData.RegionData.ColorType.Blue:
                m_spriteRenderer.color = new Color(94 / 255.0f, 135 / 255.0f, 201 / 255.0f);
                break;
        }

        if(m_regionID == RegionEditScreenManager.Instance.SelectedRegionID)
        {
            m_spriteRenderer.color += new Color(0.2f, 0.2f, 0.2f);
        }
    }
    #endregion
}