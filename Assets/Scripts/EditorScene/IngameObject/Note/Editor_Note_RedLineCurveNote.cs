using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Editor_Note_RedLineCurveNote : MonoBehaviour
{
    [SerializeField] private BoxCollider2D m_MissHitJudgyBox;

    private Guid m_curNoteID;


    internal void Init()
    {
        m_curNoteID = Guid.NewGuid();

        NoteEditScreenManager.Instance.RedLineCurveNotes.Add(this);

        float velocity = (GridRenderManager.Instance.GetTotalBitCount() * (StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit / StageDataBuffer.Instance.CurStageData.Value.StageConfig.BitSubDivision)) / ChiefGameManager.Instance.CurMusicClip.length;
        m_MissHitJudgyBox.size = new Vector2()
        {
            x = 1.0f,
            y = velocity * (GameConfigDataBuffer.Instance.GameConfigData.Value.noteHitJudgingStrandard.HitJudgingRanges[2] / 1000.0f) * 4.0f
        };
    }

    internal Guid NoteID
    {
        get
        {
            return m_curNoteID;
        }
    }
}