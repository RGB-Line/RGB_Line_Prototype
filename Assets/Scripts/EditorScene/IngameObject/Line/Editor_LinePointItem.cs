using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;


public class Editor_LinePointItem : MonoBehaviour
{
    private SpriteRenderer m_spriteRenderer;
    private CircleCollider2D m_circleCollider;

    private Editor_LineItem m_lineItem;
    private int m_pointIndex;

    [SerializeField] private GameObject m_prefab_RedLineCurvedNote;


    public void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_circleCollider = GetComponent<CircleCollider2D>();
    }
    public void Update()
    {
        if (LineEditScreenManager.Instance.SelectedMode != LineEditScreenManager.LineEditScreenSelectModeType.LinePointItem ||
            LineEditScreenManager.Instance.SelectedLineID != m_lineItem.LineID ||
            m_lineItem.SelectedPointIndex != m_pointIndex)
        {
            return;
        }

        int realPointIndex = m_pointIndex;
        if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].AttachedRegionID].CurColorType == StageData.RegionData.ColorType.Green)
        {
            realPointIndex -= 1;
        }

        Debug.Log("Selected Line Point Index: " + realPointIndex);

        if (Input.GetKeyDown(KeyCode.UpArrow) &&
            StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].Y + 1 <= LineEditScreenManager.Instance.GetCurLineMaxFrameCount() &&
            ((realPointIndex + 1 == StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints.Count) ||
            realPointIndex + 1 < StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints.Count &&
            StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].Y + 1 < StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex + 1].Y))
        {
            if(StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[RegionEditScreenManager.Instance.SelectedRegionID].CurColorType != StageData.RegionData.ColorType.Blue &&
                realPointIndex == 0)
            {
                return;
            }

            StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex] = new HalfFloatVector2
            {
                X = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].X,
                Y = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].Y + 1
            };

            m_lineItem.RenderLineItem(m_lineItem.LineID);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow) &&
            StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].Y - 1 >= 0 &&
            ((realPointIndex == 0) ||
            realPointIndex - 1 < StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints.Count &&
            StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].Y - 1 > StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex - 1].Y))
        {
            if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[RegionEditScreenManager.Instance.SelectedRegionID].CurColorType != StageData.RegionData.ColorType.Blue &&
                realPointIndex == StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints.Count - 1)
            {
                return;
            }

            StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex] = new HalfFloatVector2
            {
                X = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].X,
                Y = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].Y - 1
            };

            m_lineItem.RenderLineItem(m_lineItem.LineID);
        }
        else if (Input.GetKey(KeyCode.LeftArrow) &&
            Mathf.Abs(StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].X - 0.05f) <= 7.0f)
        {
            if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[RegionEditScreenManager.Instance.SelectedRegionID].CurColorType == StageData.RegionData.ColorType.Blue)
            {
                for(int lineIndex = 0; lineIndex < StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints.Count; lineIndex++)
                {
                    StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[lineIndex] = new HalfFloatVector2
                    {
                        X = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[lineIndex].X - 0.05f,
                        Y = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[lineIndex].Y
                    };
                }
            }
            else
            {
                StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex] = new HalfFloatVector2
                {
                    X = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].X - 0.05f,
                    Y = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].Y
                };

                if(realPointIndex == 0 && StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.ToList().IndexOf(StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].AttachedRegionID) - 1 >= 0)
                {
                    int prevRegionIndex = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.ToList().IndexOf(StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].AttachedRegionID) - 1;
                    Guid prevRegionID = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.ElementAt(prevRegionIndex);

                    Guid prevRegionAttachedLineID = Guid.Empty;
                    foreach (var lineItem in StageDataBuffer.Instance.CurStageData.Value.LineDataTable)
                    {
                        if (lineItem.Value.AttachedRegionID == prevRegionID)
                        {
                            prevRegionAttachedLineID = lineItem.Key;
                            break;
                        }
                    }

                    if(prevRegionAttachedLineID != Guid.Empty)
                    {
                        StageDataBuffer.Instance.CurStageData.Value.LineDataTable[prevRegionAttachedLineID].CurvedLinePoints[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[prevRegionAttachedLineID].CurvedLinePoints.Count - 1] = new HalfFloatVector2
                        {
                            X = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].X,
                            Y = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[prevRegionAttachedLineID].CurvedLinePoints[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[prevRegionAttachedLineID].CurvedLinePoints.Count - 1].Y
                        };

                        LineEditScreenManager.Instance.GetLineItem(prevRegionAttachedLineID).RenderLineItem(prevRegionAttachedLineID);
                    }
                }
                if(realPointIndex == StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints.Count - 1 &&
                    StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.ToList().IndexOf(StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].AttachedRegionID) + 1 < StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Count)
                {
                    int nextRegionIndex = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.ToList().IndexOf(StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].AttachedRegionID) + 1;
                    Guid nextRegionID = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.ElementAt(nextRegionIndex);

                    Guid nextRegionAttachedLineID = Guid.Empty;
                    foreach (var lineItem in StageDataBuffer.Instance.CurStageData.Value.LineDataTable)
                    {
                        if (lineItem.Value.AttachedRegionID == nextRegionID)
                        {
                            nextRegionAttachedLineID = lineItem.Key;
                            break;
                        }
                    }

                    if(nextRegionAttachedLineID != Guid.Empty)
                    {
                        StageDataBuffer.Instance.CurStageData.Value.LineDataTable[nextRegionAttachedLineID].CurvedLinePoints[0] = new HalfFloatVector2
                        {
                            X = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].X,
                            Y = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[nextRegionAttachedLineID].CurvedLinePoints[0].Y
                        };

                        LineEditScreenManager.Instance.GetLineItem(nextRegionAttachedLineID).RenderLineItem(nextRegionAttachedLineID);
                    }
                }
            }

            m_lineItem.RenderLineItem(m_lineItem.LineID);
        }
        else if (Input.GetKey(KeyCode.RightArrow) &&
                 Mathf.Abs(StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].X + 0.05f) <= 7.0f)
        {
            if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[RegionEditScreenManager.Instance.SelectedRegionID].CurColorType == StageData.RegionData.ColorType.Blue)
            {
                for (int lineIndex = 0; lineIndex < StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints.Count; lineIndex++)
                {
                    StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[lineIndex] = new HalfFloatVector2
                    {
                        X = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[lineIndex].X + 0.05f,
                        Y = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[lineIndex].Y
                    };
                }
            }
            else
            {
                StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex] = new HalfFloatVector2
                {
                    X = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].X + 0.05f,
                    Y = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].Y
                };

                if (realPointIndex == 0 && StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.ToList().IndexOf(StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].AttachedRegionID) - 1 >= 0)
                {
                    int prevRegionIndex = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.ToList().IndexOf(StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].AttachedRegionID) - 1;
                    Guid prevRegionID = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.ElementAt(prevRegionIndex);

                    Guid prevRegionAttachedLineID = Guid.Empty;
                    foreach (var lineItem in StageDataBuffer.Instance.CurStageData.Value.LineDataTable)
                    {
                        if (lineItem.Value.AttachedRegionID == prevRegionID)
                        {
                            prevRegionAttachedLineID = lineItem.Key;
                            break;
                        }
                    }

                    if(prevRegionAttachedLineID != Guid.Empty)
                    {
                        StageDataBuffer.Instance.CurStageData.Value.LineDataTable[prevRegionAttachedLineID].CurvedLinePoints[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[prevRegionAttachedLineID].CurvedLinePoints.Count - 1] = new HalfFloatVector2
                        {
                            X = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].X,
                            Y = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[prevRegionAttachedLineID].CurvedLinePoints[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[prevRegionAttachedLineID].CurvedLinePoints.Count - 1].Y
                        };

                        LineEditScreenManager.Instance.GetLineItem(prevRegionAttachedLineID).RenderLineItem(prevRegionAttachedLineID);
                    }
                }
                if (realPointIndex == StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints.Count - 1 &&
                    StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.ToList().IndexOf(StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].AttachedRegionID) + 1 < StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Count)
                {
                    int nextRegionIndex = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.ToList().IndexOf(StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].AttachedRegionID) + 1;
                    Guid nextRegionID = StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Keys.ElementAt(nextRegionIndex);

                    Guid nextRegionAttachedLineID = Guid.Empty;
                    foreach (var lineItem in StageDataBuffer.Instance.CurStageData.Value.LineDataTable)
                    {
                        if (lineItem.Value.AttachedRegionID == nextRegionID)
                        {
                            nextRegionAttachedLineID = lineItem.Key;
                            break;
                        }
                    }

                    if(nextRegionAttachedLineID != Guid.Empty)
                    {
                        StageDataBuffer.Instance.CurStageData.Value.LineDataTable[nextRegionAttachedLineID].CurvedLinePoints[0] = new HalfFloatVector2
                        {
                            X = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].X,
                            Y = StageDataBuffer.Instance.CurStageData.Value.LineDataTable[nextRegionAttachedLineID].CurvedLinePoints[0].Y
                        };

                        LineEditScreenManager.Instance.GetLineItem(nextRegionAttachedLineID).RenderLineItem(nextRegionAttachedLineID);
                    }
                }
            }

            m_lineItem.RenderLineItem(m_lineItem.LineID);
        }

        float minorSetStep = GridRenderManager.Instance.GetUnitFramePosition() * MenuPopup_MusicDetail.Instance.MinorSetStep;
        if (Input.GetKeyDown(KeyCode.KeypadPlus) &&
            StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].Y + 1 <= LineEditScreenManager.Instance.GetCurLineMaxFrameCount() &&
            StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].MinorOffsetTimes[realPointIndex] + minorSetStep <= 0.5f)
        {
            if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[RegionEditScreenManager.Instance.SelectedRegionID].CurColorType != StageData.RegionData.ColorType.Blue &&
                realPointIndex == 0)
            {
                return;
            }

            StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].MinorOffsetTimes[realPointIndex] += minorSetStep;

            m_lineItem.RenderLineItem(m_lineItem.LineID);
        }
        else if(Input.GetKeyDown(KeyCode.KeypadMinus) &&
            StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints[realPointIndex].Y - 1 >= 0 &&
            StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].MinorOffsetTimes[realPointIndex] - minorSetStep >= -0.5f)
        {
            if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[RegionEditScreenManager.Instance.SelectedRegionID].CurColorType != StageData.RegionData.ColorType.Blue &&
                realPointIndex == StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints.Count - 1)
            {
                return;
            }

            StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].MinorOffsetTimes[realPointIndex] -= minorSetStep;

            m_lineItem.RenderLineItem(m_lineItem.LineID);
        }
    }
    public void OnMouseDown()
    {
        if(m_lineItem.LineID != LineEditScreenManager.Instance.SelectedLineID)
        {
            return;
        }

        if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].AttachedRegionID].CurColorType == StageData.RegionData.ColorType.Green)
        {
            if (m_pointIndex == 0 || m_pointIndex == (StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints.Count + 2) - 1)
            {
                return;
            }
        }

        //int realPointIndex = m_pointIndex;
        //if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].AttachedRegionID].CurColorType == StageData.RegionData.ColorType.Green)
        //{
        //    realPointIndex -= 1;
        //}

        if (LineEditScreenManager.Instance.SelectedMode == LineEditScreenManager.LineEditScreenSelectModeType.LinePointItem &&
            RegionEditScreenManager.Instance.SelectedRegionID != Guid.Empty)
        {
            m_lineItem.SelectedPointIndex = m_pointIndex;
        }
    }

    #region Utils
    internal void RenderLinePointItem(in Editor_LineItem lineItem, in Vector2 pos, in float minorOffsetTime, in int pointIndex)
    {
        m_lineItem = lineItem;
        m_pointIndex = pointIndex;

        transform.position = new Vector3()
        {
            x = pos.x,
            y = GridRenderManager.Instance.GetFramePosition(pos.y + minorOffsetTime)
                + GridRenderManager.Instance.GetFramePosition(StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].AttachedRegionID].StartOffsetFrame),
            z = -5.0f
        };

        if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].AttachedRegionID].CurColorType == StageData.RegionData.ColorType.Green)
        {
            transform.position = new Vector3()
            {
                x = transform.position.x,
                y = transform.position.y,
                z = 5.0f
            };
        }

        if (StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].AttachedRegionID].CurColorType == StageData.RegionData.ColorType.Red &&
            0 < m_pointIndex && m_pointIndex < StageDataBuffer.Instance.CurStageData.Value.LineDataTable[m_lineItem.LineID].CurvedLinePoints.Count - 1)
        {
            Editor_Note_RedLineCurveNote targetNoteItem = Instantiate(m_prefab_RedLineCurvedNote, transform).GetComponent<Editor_Note_RedLineCurveNote>();
            targetNoteItem.Init();
            targetNoteItem.transform.position = transform.position;
        }

        if (LineEditScreenManager.Instance.SelectedLineID == m_lineItem.LineID)
        {
            m_circleCollider.enabled = true;

            if (m_lineItem.SelectedPointIndex == m_pointIndex)
            {
                m_spriteRenderer.color = new Color(141 / 255.0f, 91 / 255.0f, 193 / 255.0f, 1.0f);
            }
            else
            {
                m_spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
        }
        else
        {
            m_circleCollider.enabled = false;

            m_spriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
        }
    }
    #endregion
}