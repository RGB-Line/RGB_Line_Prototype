using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Editor_DetermineLine : MonoBehaviour
{
    private static Editor_DetermineLine m_instance;


    public void Awake()
    {
        m_instance = this;
    }
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(!ChiefGameManager.Instance.BIsMusicPlaying)
        {
            return;
        }

        if(collision.GetComponent<Editor_Note_RedAndBlueHitJudgeBox>() != null)
        {
            ScoreManager.Instance.PushNoteCandidate(collision.GetComponent<Editor_Note_RedAndBlueHitJudgeBox>().AttachedNoteID);
            //collision.GetComponent<Editor_Note_RedAndBlueHitJudgeBox>().HitNote();
        }
        if(collision.GetComponent<Editor_Note_GreenHitJudgeBox>() != null)
        {
            ScoreManager.Instance.PushNoteCandidate(collision.GetComponent<Editor_Note_GreenHitJudgeBox>().AttachedNoteID);
            //collision.GetComponent<Editor_Note_RedAndBlueHitJudgeBox>().HitNote();
        }
        if(collision.GetComponent<Editor_Note_RedLineCurveNote>() != null)
        {
            ScoreManager.Instance.PushNoteCandidate(collision.GetComponent<Editor_Note_RedLineCurveNote>().NoteID);
        }

        if(collision.GetComponent<Editor_RegionItem>() != null && StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[collision.GetComponent<Editor_RegionItem>().RegionID].CurColorType == StageData.RegionData.ColorType.Green)
        {
            ScoreManager.Instance.BIsGreenRegion = true;
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (!ChiefGameManager.Instance.BIsMusicPlaying)
        {
            return;
        }

        if (collision.GetComponent<Editor_Note_RedAndBlueHitJudgeBox>() != null)
        {
            ScoreManager.Instance.RemoveNoteCandidate(collision.GetComponent<Editor_Note_RedAndBlueHitJudgeBox>().AttachedNoteID);
        }
        if (collision.GetComponent<Editor_Note_GreenHitJudgeBox>() != null)
        {
            ScoreManager.Instance.RemoveNoteCandidate(collision.GetComponent<Editor_Note_GreenHitJudgeBox>().AttachedNoteID);
        }
        if (collision.GetComponent<Editor_Note_RedLineCurveNote>() != null)
        {
            ScoreManager.Instance.RemoveNoteCandidate(collision.GetComponent<Editor_Note_RedLineCurveNote>().NoteID);
        }

        if (collision.GetComponent<Editor_RegionItem>() != null && StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[collision.GetComponent<Editor_RegionItem>().RegionID].CurColorType == StageData.RegionData.ColorType.Green)
        {
            ScoreManager.Instance.BIsGreenRegion = false;
        }
    }

    public static Editor_DetermineLine Instance
    {
        get
        {
            return m_instance;
        }
    }
}