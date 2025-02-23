using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Editor_NoteItem : MonoBehaviour
{
    private Guid m_noteID;

    private StageData.NoteData.NoteType m_curNoteType;

    [SerializeField] private Editor_Note_RedAndBlueNote m_redAndBlueNote;
    [SerializeField] private Editor_Note_GreenNote m_greenNote;


    public void Awake()
    {

    }

    internal Guid NoteID
    {
        get
        {
            return m_noteID;
        }
    }

    internal Editor_Note_RedAndBlueNote RedAndBlueNoteItem
    {
        get
        {
            return m_redAndBlueNote;
        }
    }
    internal Editor_Note_GreenNote GreenNoteItem
    {
        get
        {
            return m_greenNote;
        }
    }

    #region Utils
    internal void RenderNoteItem(in Guid noteID)
    {
        m_noteID = noteID;

        switch (StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[noteID].CurNoteType)
        {
            case StageData.NoteData.NoteType.Common:
                m_redAndBlueNote.RenderRedAndBlueNoteItem(this, StageData.NoteData.NoteType.Common);

                m_greenNote.gameObject.SetActive(false);
                break;

            case StageData.NoteData.NoteType.Flip:
                m_redAndBlueNote.RenderRedAndBlueNoteItem(this, StageData.NoteData.NoteType.Flip);

                m_greenNote.gameObject.SetActive(false);
                break;

            case StageData.NoteData.NoteType.Long:
                m_greenNote.RenderGreenNoteItem(this);

                m_redAndBlueNote.gameObject.SetActive(false);
                break;
        }
    }

    internal void HitNote()
    {
        ScoreManager.Instance.PushNoteCandidate(m_noteID);
    }

    internal float GetNoteXPos(in float targetFrame, in Guid attachedLineID)
    {
        float NoteYPos = GridRenderManager.Instance.GetFramePosition(targetFrame) +
                         GridRenderManager.Instance.GetFramePosition(StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[attachedLineID].AttachedRegionID].StartOffsetFrame);
        //Debug.Log("NoteYPos : " + NoteYPos);

        List<int> nearestLinePosIndexes = new List<int>(2);
        LineRenderer attachedLineRenderer = LineEditScreenManager.Instance.GetLineItem(StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteID].AttachedLineID).LineRenderer;

        // For Blue Line
        if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[attachedLineID].AttachedRegionID].CurColorType == StageData.RegionData.ColorType.Blue)
        {
            return attachedLineRenderer.GetPosition(0).x;
        }

        for (int index = 0; index < attachedLineRenderer.positionCount; index++)
        {
            if(attachedLineRenderer.GetPosition(index).y == NoteYPos)
            {
                return attachedLineRenderer.GetPosition(index).x;
            }
        }

        // Most nearest line pos
        int curNearestLinePosIndex = -1;
        float curNearestLinePos = float.MaxValue;
        for (int linePosIndex = 0; linePosIndex < attachedLineRenderer.positionCount; linePosIndex++)
        {
            float linePos = attachedLineRenderer.GetPosition(linePosIndex).y;
            if(linePos > NoteYPos && Mathf.Abs(linePos - NoteYPos) < Mathf.Abs(curNearestLinePos - NoteYPos))
            {
                curNearestLinePosIndex = linePosIndex;
                curNearestLinePos = linePos;
            }
        }
        nearestLinePosIndexes.Add(curNearestLinePosIndex);

        if(Mathf.Abs(attachedLineRenderer.GetPosition(nearestLinePosIndexes[0]).y - NoteYPos) == 0)
        {
            return attachedLineRenderer.GetPosition(curNearestLinePosIndex).x;
        }

        // Second nearest line pos
        curNearestLinePosIndex = -1;
        curNearestLinePos = float.MaxValue;
        for (int linePosIndex = 0; linePosIndex < attachedLineRenderer.positionCount; linePosIndex++)
        {
            if(nearestLinePosIndexes.Contains(linePosIndex))
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