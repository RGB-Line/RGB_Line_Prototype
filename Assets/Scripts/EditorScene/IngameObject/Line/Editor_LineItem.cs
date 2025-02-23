using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using static StageData;


public class Editor_LineItem : MonoBehaviour, IDisposable
{
    [Serializable] public struct LineMaterial
    {
        [Header("Line Material")]
        public Material m_material_RedLine;
        public Material m_material_GreenLine;
        public Material m_material_BlueLine;

        [Header("Line Material Editor")]
        public Material m_material_RedLine_Editor;
        public Material m_material_GreenLine_Editor;
        public Material m_material_BlueLine_Editor;
    }


    [SerializeField] private GameObject m_prefab_LinePoint;

    [SerializeField] private LineMaterial m_materials;

    private LineRenderer m_lineRenderer;
    private MeshCollider m_meshCollider;
    private CurvedLineRenderer m_curvedLineRenderer;

    private Guid m_lineID;
    private int m_selectedPointIndex = -1;

    private Stack<Editor_LinePointItem> m_linePointItems = new Stack<Editor_LinePointItem>();


    public void Awake()
    {
        m_lineRenderer = GetComponent<LineRenderer>();
        m_meshCollider = GetComponent<MeshCollider>();
        m_curvedLineRenderer = GetComponent<CurvedLineRenderer>();
    }
    public void FixedUpdate()
    {
        if (ChiefGameManager.Instance.BIsEditorMode ||
            StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineID].AttachedRegionID].CurColorType == StageData.RegionData.ColorType.Green)
        {
            UpdateMeshCollider();
        }
    }
    //public void Update()
    //{
    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

    //        if (hit.collider != null)
    //        {
    //            if (LineEditScreenManager.Instance.SelectedMode == LineEditScreenManager.LineEditScreenSelectModeType.LineItem)
    //            {
    //                LineEditScreenManager.Instance.SelectedLineID = m_lineID;
    //            }
    //        }
    //    }
    //}
    public void OnMouseDown()
    {
        if (LineEditScreenManager.Instance.SelectedMode == LineEditScreenManager.LineEditScreenSelectModeType.LineItem &&
            StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineID].AttachedRegionID == RegionEditScreenManager.Instance.SelectedRegionID)
        {
            LineEditScreenManager.Instance.SelectedLineID = m_lineID;
        }
    }
    public void OnMouseEnter()
    {
        if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineID].AttachedRegionID].CurColorType == StageData.RegionData.ColorType.Green)
        {
            ScoreManager.Instance.BIsMouseOnGreenLine = true;
        }
        else
        {
            ScoreManager.Instance.BIsMouseOnGreenLine = false;
        }
    }
    public void OnMouseExit()
    {
        //if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineID].AttachedRegionID].CurColorType == StageData.RegionData.ColorType.Green)
        //{
            ScoreManager.Instance.BIsMouseOnGreenLine = false;
        //}
    }

    internal int SelectedPointIndex
    {
        get
        {
            return m_selectedPointIndex;
        }
        set
        {
            m_selectedPointIndex = value;

            RenderLineItem(m_lineID);
        }
    }
    internal Guid LineID
    {
        get
        {
            return m_lineID;
        }
    }

    internal LineRenderer LineRenderer
    {
        get
        {
            return m_lineRenderer;
        }
    }

    #region Utils
    public void Dispose()
    {
        while (true)
        {
            if (m_linePointItems.Count == 0)
            {
                break;
            }

            Editor_LinePointItem linePointItem = m_linePointItems.Pop();
            Destroy(linePointItem.gameObject);
        }
    }

    internal void RenderLineItem(in Guid lineID)
    {
        m_lineID = lineID;

        Dispose();

        List<HalfFloatVector2> points = (StageDataBuffer.Instance.CurStageData.Value.LineDataTable[lineID].CurvedLinePoints.ToArray().Clone() as HalfFloatVector2[]).ToList();
        List<float> minorOffsetTimes = (StageDataBuffer.Instance.CurStageData.Value.LineDataTable[lineID].MinorOffsetTimes.ToArray().Clone() as float[]).ToList();
        if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineID].AttachedRegionID].CurColorType == StageData.RegionData.ColorType.Green)
        {
            HalfFloatVector2 startPos = new HalfFloatVector2
            {
                X = points.First().X,
                Y = points.First().Y - 1
            };
            points.Insert(0, startPos);
            minorOffsetTimes.Insert(0, 0.0f);

            HalfFloatVector2 endPos = new HalfFloatVector2
            {
                X = points.Last().X,
                Y = points.Last().Y + 1
            };
            points.Add(endPos);
            minorOffsetTimes.Add(0.0f);
        }

        for(int index = 0; index < points.Count; index++)
        {
            Editor_LinePointItem linePointItem = Instantiate(m_prefab_LinePoint, transform).GetComponent<Editor_LinePointItem>();

            Vector2 pointPos = new Vector2(points[index].X, points[index].Y);
            if(StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineID].AttachedRegionID].CurColorType == StageData.RegionData.ColorType.Green)
            {
                if (index == 0 || index == 1)
                {
                    pointPos.y += StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineID].AttachedRegionID].MinorOffsetTime;
                }
                else if (index == points.Count - 2 || index == points.Count - 1)
                {
                    Guid nextRegionID = Guid.Empty;
                    foreach (Guid regionID in StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys)
                    {
                        if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[regionID].StartOffsetFrame > StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineID].AttachedRegionID].StartOffsetFrame)
                        {
                            nextRegionID = regionID;
                            break;
                        }
                    }

                    if (nextRegionID != Guid.Empty)
                    {
                        pointPos.y += StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[nextRegionID].MinorOffsetTime;
                    }
                }
            }
            else
            {
                if(index == 0)
                {
                    pointPos.y += StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineID].AttachedRegionID].MinorOffsetTime;
                }
                else if(index == points.Count - 1)
                {
                    Guid nextRegionID = Guid.Empty;
                    foreach(Guid regionID in StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys)
                    {
                        if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[regionID].StartOffsetFrame > StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineID].AttachedRegionID].StartOffsetFrame)
                        {
                            nextRegionID = regionID;
                            break;
                        }
                    }

                    if (nextRegionID != Guid.Empty)
                    {
                        pointPos.y += StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[nextRegionID].MinorOffsetTime;
                    }
                }
            }

            linePointItem.RenderLinePointItem(this, pointPos, minorOffsetTimes[index], index);
            m_linePointItems.Push(linePointItem);
        }

        m_lineRenderer.numCapVertices = 90;
        m_lineRenderer.alignment = LineAlignment.TransformZ;

        DrawLineItem();

        Invoke("RenderNoteScreen", 1.0f);
    }

    internal void UpdateMeshCollider()
    {
        Mesh mesh = new Mesh();
        m_lineRenderer.BakeMesh(mesh, true);
        m_meshCollider.sharedMesh = mesh;
    }

    private void DrawLineItem()
    {
        switch(StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineID].CurLineSmoothType)
        {
            case StageData.LineData.LineSmoothType.Linear:
                m_curvedLineRenderer.lineSegmentSize = 100.0f;
                break;

            case StageData.LineData.LineSmoothType.Curved:
                m_curvedLineRenderer.lineSegmentSize = 0.15f;
                break;
        }

        if (LineEditScreenManager.Instance.SelectedLineID == m_lineID)
        {
            switch (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineID].AttachedRegionID].CurColorType)
            {
                case StageData.RegionData.ColorType.Red:
                    m_lineRenderer.material = m_materials.m_material_RedLine_Editor;
                    break;

                case StageData.RegionData.ColorType.Green:
                    m_lineRenderer.material = m_materials.m_material_GreenLine_Editor;
                    break;

                case StageData.RegionData.ColorType.Blue:
                    m_lineRenderer.material = m_materials.m_material_BlueLine_Editor;
                    break;
            }
        }
        else
        {
            switch (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineID].AttachedRegionID].CurColorType)
            {
                case StageData.RegionData.ColorType.Red:
                    m_lineRenderer.material = m_materials.m_material_RedLine;
                    break;

                case StageData.RegionData.ColorType.Green:
                    m_lineRenderer.material = m_materials.m_material_GreenLine;
                    break;

                case StageData.RegionData.ColorType.Blue:
                    m_lineRenderer.material = m_materials.m_material_BlueLine;
                    break;
            }
        }

        m_curvedLineRenderer.lineWidth = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineID].LineWidth;
    }

    private void RenderNoteScreen()
    {
        NoteEditScreenManager.Instance.Render();
    }
    #endregion
}