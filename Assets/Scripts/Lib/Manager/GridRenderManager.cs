using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;


public sealed class GridRenderManager : MonoBehaviour
{
    private static GridRenderManager m_instance;

    [SerializeField] private GameObject m_prefab_MainHorizontalLine;
    [SerializeField] private GameObject m_prefab_SubHorizontalLine;

    private Stack<GameObject> m_horizontalLines = new Stack<GameObject>();


    public void Awake()
    {
        m_instance = this;
    }

    public static GridRenderManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    public void RenderGrid()
    {
        if (StageDataBuffer.Instance.CurStageData == null)
        {
            LoggingManager.Instance.AssertLogMessage(LoggingManager.LogType.Error, "StageData is null");
            return;
        }

        while (m_horizontalLines.Count > 0)
        {
            GameObject horizontalLine = m_horizontalLines.Pop();
            Destroy(horizontalLine);
        }

        int totalBitCount = GetTotalBitCount();

        //Debug.Log($"Total Bit Count: {totalBitCount}");

        for (int index = 0; index < totalBitCount; index++)
        {
            GameObject horizontalLine = null;
            if (BIsMainBitLine(index))
            {
                horizontalLine = Instantiate(m_prefab_MainHorizontalLine, transform);
            }
            else
            {
                horizontalLine = Instantiate(m_prefab_SubHorizontalLine, transform);
            }
            m_horizontalLines.Push(horizontalLine);
            horizontalLine.transform.position = new Vector3(0.0f, GetFramePosition(index), 0.0f);
        }
    }

    internal int GetTotalBitCount()
    {
        //Debug.Log($"Length: {ChiefGameManager.Instance.CurMusicClip.length}");

        if (StageDataBuffer.Instance.CurStageData == null)
        {
            return 0;
        }

        float BPS = (StageDataBuffer.Instance.CurStageData.Value.StageConfig.BPM * StageDataBuffer.Instance.CurStageData.Value.StageConfig.BitSubDivision) / 60f;

        //Debug.Log($"BPS: {BPS}");

        return (int)(BPS * ChiefGameManager.Instance.CurMusicClip.length);
    }
    internal float GetFramePosition(in float index)
    {
        return index * GetUnitFramePosition();
    }
    internal float GetUnitFramePosition()
    {
        return StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit / StageDataBuffer.Instance.CurStageData.Value.StageConfig.BitSubDivision;
    }

    private bool BIsMainBitLine(in int index)
    {
        return index % StageDataBuffer.Instance.CurStageData.Value.StageConfig.BitSubDivision == 0;
    }
}