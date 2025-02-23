using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


public class Editor_Note_RedAndBlueNote : MonoBehaviour
{
    private SpriteRenderer m_spriteRenderer;
    private BoxCollider2D m_boxCollider;

    private Editor_NoteItem m_noteItem;

    [SerializeField] private Sprite[] m_sprite_Notes;
    [SerializeField] private BoxCollider2D m_MissHitJudgyBox;


    public void Awake()
    {
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        m_boxCollider = GetComponent<BoxCollider2D>();
    }
    public void FixedUpdate()
    {
        m_boxCollider.enabled = false;
        m_boxCollider.enabled = true;
    }
    public void Update()
    {
        if(!NoteEditScreenManager.Instance.BIsNoteSelectMode ||
            NoteEditScreenManager.Instance.SelectedNoteID != m_noteItem.NoteID)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) &&
            StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].MinorOffsetTime + 0.05f <= 0.5f)
        {
            StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID] = new StageData.NoteData()
            {
                AttachedLineID = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].AttachedLineID,
                StartOffsetFrame = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].StartOffsetFrame,
                MinorOffsetTime = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].MinorOffsetTime + 0.05f,
                NoteLength = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].NoteLength,
                flipNoteDirection = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].flipNoteDirection,
                CurNoteType = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].CurNoteType
            };

            m_noteItem.RenderNoteItem(m_noteItem.NoteID);
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow) &&
            StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].MinorOffsetTime - 0.05f >= -0.5f)
        {
            StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID] = new StageData.NoteData()
            {
                AttachedLineID = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].AttachedLineID,
                StartOffsetFrame = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].StartOffsetFrame,
                MinorOffsetTime = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].MinorOffsetTime - 0.05f,
                NoteLength = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].NoteLength,
                flipNoteDirection = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].flipNoteDirection,
                CurNoteType = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].CurNoteType
            };

            m_noteItem.RenderNoteItem(m_noteItem.NoteID);
        }
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

    #region Utils
    internal void RenderRedAndBlueNoteItem(in Editor_NoteItem noteItem, in StageData.NoteData.NoteType noteType)
    {
        m_noteItem = noteItem;

        switch(noteType)
        {
            case StageData.NoteData.NoteType.Common:
                m_spriteRenderer.sprite = m_sprite_Notes[0];
                break;
            case StageData.NoteData.NoteType.Flip:
                m_spriteRenderer.sprite = m_sprite_Notes[1];

                switch(StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].flipNoteDirection)
                {
                    case StageData.NoteData.FlipNoteDirection.Left:
                        m_spriteRenderer.flipX = false;
                        break;

                    case StageData.NoteData.FlipNoteDirection.Right:
                        m_spriteRenderer.flipX = true;
                        break;
                }
                break;
        }

        if(NoteEditScreenManager.Instance.SelectedNoteID == m_noteItem.NoteID)
        {
            m_spriteRenderer.color = Color.white;
        }
        else
        {
            m_spriteRenderer.color = Color.gray;
        }

        transform.position = new Vector3()
        {
            x = m_noteItem.GetNoteXPos(StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].StartOffsetFrame + StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].MinorOffsetTime, StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].AttachedLineID),
            y = GridRenderManager.Instance.GetFramePosition(StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].AttachedLineID].AttachedRegionID].StartOffsetFrame) +
                GridRenderManager.Instance.GetFramePosition(StageDataBuffer.Instance.CurStageData.Value.LineDataTable[StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].AttachedLineID].CurvedLinePoints[0].Y) +
                GridRenderManager.Instance.GetFramePosition(StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].StartOffsetFrame) + 
                GridRenderManager.Instance.GetUnitFramePosition() * StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[m_noteItem.NoteID].MinorOffsetTime,
            z = -7.5f
        };

        float velocity = (GridRenderManager.Instance.GetTotalBitCount() * (StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit / StageDataBuffer.Instance.CurStageData.Value.StageConfig.BitSubDivision)) / ChiefGameManager.Instance.CurMusicClip.length;
        m_MissHitJudgyBox.size = new Vector2()
        {
            x = 1.0f,
            y = velocity * (GameConfigDataBuffer.Instance.GameConfigData.Value.noteHitJudgingStrandard.HitJudgingRanges[2] / 1000.0f) * 4.0f
        };

        m_MissHitJudgyBox.GetComponent<Editor_Note_RedAndBlueHitJudgeBox>().Init(NoteID);
    }

    internal void HitNote()
    {
        //float curNoteRatio = ((transform.position.y - Editor_DetermineLine.Instance.transform.localPosition.y)
        //                    / (StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit / StageDataBuffer.Instance.CurStageData.Value.StageConfig.BitSubDivision)) / GridRenderManager.Instance.GetTotalBitCount();

        //float curTime = ChiefGameManager.Instance.CurMusicClip.length * curNoteRatio;
        //curTime -= StageDataBuffer.Instance.CurStageData.Value.StageConfig.MusicStartOffsetTime;
        //Debug.Log("Camera : " + ChiefGameManager.Instance.MainCameraRigidbody2D.transform.position.y + " - " + ChiefGameManager.Instance.UI.m_slider_TimeMover.value
        //            + " / Note : " + (transform.position.y - Editor_DetermineLine.Instance.transform.localPosition.y) + " - " + curNoteRatio);

        //gameObject.SetActive(false);

        m_noteItem.HitNote();
    }
    #endregion
}