using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Editor_Note_GreenNote : MonoBehaviour
{
    private LineRenderer m_lineRenderer;
    private MeshCollider m_meshCollider;

    private Editor_NoteItem m_noteItem;

    [SerializeField] private Transform m_transform_StartJudgeBox;
    [SerializeField] private Transform m_transform_EndJudgeBox;

    [SerializeField] private Material m_material_SelectedColor;
    [SerializeField] private Material m_material_UnselectedColor;


    public void Awake()
    {
        m_lineRenderer = GetComponent<LineRenderer>();
        m_meshCollider = GetComponent<MeshCollider>();
    }
    public void FixedUpdate()
    {
        Mesh mesh = new Mesh();
        m_lineRenderer.BakeMesh(mesh, true);
        m_meshCollider.sharedMesh = mesh;
    }
    public void OnMouseDown()
    {
        if (NoteEditScreenManager.Instance.BIsNoteSelectMode &&
            RegionEditScreenManager.Instance.SelectedRegionID != Guid.Empty &&
            LineEditScreenManager.Instance.SelectedLineID != Guid.Empty)
        {
            NoteEditScreenManager.Instance.SelectedNoteID = m_noteItem.NoteID;
        }
    }

    internal Guid NoteID
    {
        get
        { 
            return m_noteItem.NoteID;
        }
    }

    internal float CurveStartYPos
    {
        get
        {
            return m_lineRenderer.GetPosition(0).y;
        }
    }

    #region Utils
    internal void RenderGreenNoteItem(in Editor_NoteItem noteItem)
    {
        m_noteItem = noteItem;

        Invoke("DrawLine", 0.5f);   
    }

    internal void HitNote()
    {
        m_noteItem.HitNote();
    }

    private void DrawLine()
    {
        List<Vector3> linePoses = new List<Vector3>();
        Guid attachedLineID = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].AttachedLineID;
        Vector3 startPos = new Vector3()
        {
            x = m_noteItem.GetNoteXPos(StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].StartOffsetFrame, attachedLineID),
            y = GridRenderManager.Instance.GetFramePosition(StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].AttachedLineID].AttachedRegionID].StartOffsetFrame) +
                GridRenderManager.Instance.GetFramePosition(StageDataBuffer.Instance.CurStageData.Value.LineDataTable[StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].AttachedLineID].CurvedLinePoints[0].Y) +
                GridRenderManager.Instance.GetFramePosition(StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].StartOffsetFrame),
            z = 5.0f
        };
        Vector3 endPos = new Vector3()
        {
            x = m_noteItem.GetNoteXPos(StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].StartOffsetFrame + StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].NoteLength, attachedLineID),
            y = GridRenderManager.Instance.GetFramePosition(StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].AttachedLineID].AttachedRegionID].StartOffsetFrame) +
                GridRenderManager.Instance.GetFramePosition(StageDataBuffer.Instance.CurStageData.Value.LineDataTable[StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].AttachedLineID].CurvedLinePoints[0].Y) +
                GridRenderManager.Instance.GetFramePosition(StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].StartOffsetFrame + StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].NoteLength),
            z = 5.0f
        };

        LineRenderer attachedLineRenderer = LineEditScreenManager.Instance.GetLineItem(StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].AttachedLineID).LineRenderer;
        //linePoses.Add(new Vector3()
        //{
        //    x = startPos.x,
        //    y = startPos.y,
        //    z = startPos.z + 5.0f
        //});
        linePoses.Add(startPos);
        for (int linePosIndex = 0; linePosIndex < attachedLineRenderer.positionCount; linePosIndex++)
        {
            if (startPos.y < attachedLineRenderer.GetPosition(linePosIndex).y && attachedLineRenderer.GetPosition(linePosIndex).y < endPos.y)
            {
                linePoses.Add(attachedLineRenderer.GetPosition(linePosIndex));
            }
        }
        linePoses.Add(endPos);
        //linePoses.Add(new Vector3()
        //{
        //    x = endPos.x,
        //    y = endPos.y,
        //    z = endPos.z + 5.0f
        //});

        //string log = "Line Poses: ";
        //for (int index = 0; index < linePoses.Count; index++)
        //{
        //    log += linePoses[index].ToString();
        //    log += " ";
        //}
        //Debug.Log(log);

        m_lineRenderer.positionCount = linePoses.Count;
        m_lineRenderer.SetPositions(linePoses.ToArray());

        if(NoteEditScreenManager.Instance.SelectedNoteID == m_noteItem.NoteID)
        {
            m_lineRenderer.material = m_material_SelectedColor;
        }
        else
        {
            m_lineRenderer.material = m_material_UnselectedColor;
        }

        m_lineRenderer.numCapVertices = 90;
        m_lineRenderer.alignment = LineAlignment.TransformZ;

        m_transform_StartJudgeBox.position = startPos;
        m_transform_EndJudgeBox.position = endPos;

        float velocity = (GridRenderManager.Instance.GetTotalBitCount() * (StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit / StageDataBuffer.Instance.CurStageData.Value.StageConfig.BitSubDivision)) / ChiefGameManager.Instance.CurMusicClip.length;
        m_transform_StartJudgeBox.GetComponent<BoxCollider2D>().size = new Vector2()
        {
            x = 1.0f,
            y = velocity * (GameConfigDataBuffer.Instance.GameConfigData.Value.noteHitJudgingStrandard.HitJudgingRanges[2] / 1000.0f) * 4.0f
        };
        m_transform_EndJudgeBox.GetComponent<BoxCollider2D>().size = new Vector2()
        {
            x = 1.0f,
            y = velocity * (GameConfigDataBuffer.Instance.GameConfigData.Value.noteHitJudgingStrandard.HitJudgingRanges[2] / 1000.0f) * 4.0f
        };

        m_transform_StartJudgeBox.GetComponent<Editor_Note_GreenHitJudgeBox>().Init(NoteID);
        m_transform_EndJudgeBox.GetComponent<Editor_Note_GreenHitJudgeBox>().Init(NoteID);

        try
        {
            Mesh mesh = new Mesh();
            m_lineRenderer.BakeMesh(mesh, true);
            m_meshCollider.sharedMesh = mesh;
        }
        catch
        {
            Debug.Log("BakeMesh Error");
        }
    }
    #endregion
}