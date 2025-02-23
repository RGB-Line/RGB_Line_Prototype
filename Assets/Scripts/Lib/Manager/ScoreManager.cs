using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using TMPro;


public sealed class ScoreManager : MonoBehaviour
{
    private static ScoreManager m_instance;

    private Stack<Guid> m_noteCandidates = new Stack<Guid>();
    private Dictionary<Guid, Tuple<float, float>> m_longNoteTable = new Dictionary<Guid, Tuple<float, float>>();

    [SerializeField] private List<KeyCode> m_bannedBasicNoteKeyCodes;

    [SerializeField] private TMP_Text m_text_Score;

    private bool m_bisGreenRegion;
    private bool m_bisMouseOnGreenLine;
    private bool m_bisGreenRegionClear = false;

    private float m_curScore = 0.0f;


    public void Awake()
    {
        m_instance = this;
    }
    public void FixedUpdate()
    {
        if(!ChiefGameManager.Instance.BIsMusicPlaying)
        {
            m_text_Score.gameObject.SetActive(false);
            return;
        }

        // For Green Region - Mouse Tracking
        if(m_bisGreenRegion)
        {
            if(!m_bisMouseOnGreenLine)
            {
                //Debug.Log("!!!!!");

                m_bisGreenRegionClear = false;
                m_bisGreenRegion = false;
            }
        }

        if (!m_bisGreenRegion && m_bisGreenRegionClear)
        {
            //Debug.Log("Hit");

            int greenRegionCount = 0;
            foreach (StageData.RegionData regionItem in StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Values)
            {
                if (regionItem.CurColorType == StageData.RegionData.ColorType.Green)
                {
                    greenRegionCount++;
                }
            }

            float resultScore = 1000000.0f / (StageDataBuffer.Instance.CurStageData.Value.NoteDataTable.Count + NoteEditScreenManager.Instance.RedLineCurveNotes.Count + greenRegionCount);

            m_curScore += resultScore;

            m_bisGreenRegionClear = false;
        }

        // For Long Notes
        int curPressedBasicNoteKeyCount = GetCurPressedBasicNoteKeyCount();
        if(curPressedBasicNoteKeyCount < m_longNoteTable.Count)
        {
            int unmatchedPressedKeyCount = m_longNoteTable.Count - curPressedBasicNoteKeyCount;
            for(int count = 0; count < unmatchedPressedKeyCount; count++)
            {
                float minTime = float.MaxValue;
                int minItemIndex = -1;
                for (int index = 0; index < m_longNoteTable.Values.Count; index++)
                {
                    if (m_longNoteTable.Values.ElementAt(index).Item1 < minTime)
                    {
                        minTime = m_longNoteTable.Values.ElementAt(index).Item1;
                        minItemIndex = index;
                    }
                }

                m_longNoteTable.Remove(m_longNoteTable.Keys.ElementAt(minItemIndex));
            }
        }

        // For New Notes(Include Long Note)
        int freshPressedBasicNoteKeyCount = GetFreshPressedBasicNoteKeyCount();
        for(int count = 0; count < freshPressedBasicNoteKeyCount; count++)
        {
            Guid targetNoteID = Guid.Empty;
            if(!m_noteCandidates.TryPop(out targetNoteID))
            {
                break;
            }

            // Red Line Curved Note
            if(NoteEditScreenManager.Instance.GetRedLineCurvedNote(targetNoteID) != null && Input.GetKeyDown(KeyCode.Space))
            {
                //Debug.Log("Test");
                Editor_Note_RedLineCurveNote targetNoteItem = NoteEditScreenManager.Instance.GetRedLineCurvedNote(targetNoteID);
                m_curScore += GetSingleNoteScore(GetYPos2Time(targetNoteItem.transform.position.y), ChiefGameManager.Instance.CurAudioSource.time);

                targetNoteItem.gameObject.SetActive(false);
            }
            // Red, Green, Blue Note
            else
            {
                StageData.NoteData.NoteType curNoteType = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[targetNoteID].CurNoteType;
                switch (curNoteType)
                {
                    case StageData.NoteData.NoteType.Common:
                        {
                            Editor_NoteItem targetNoteItem = NoteEditScreenManager.Instance.GetNoteItem(targetNoteID);
                            m_curScore += GetSingleNoteScore(GetYPos2Time(targetNoteItem.RedAndBlueNoteItem.transform.position.y), ChiefGameManager.Instance.CurAudioSource.time);

                            targetNoteItem.gameObject.SetActive(false);
                        }
                        break;

                    case StageData.NoteData.NoteType.Flip:
                        {
                            bool bisHit = false;
                            if (StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[targetNoteID].flipNoteDirection == StageData.NoteData.FlipNoteDirection.Left &&
                                Input.GetKeyDown(KeyCode.Mouse0))
                            {
                                bisHit = true;
                            }
                            else if (StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[targetNoteID].flipNoteDirection == StageData.NoteData.FlipNoteDirection.Right &&
                                Input.GetKeyDown(KeyCode.Mouse1))
                            {
                                bisHit = true;
                            }

                            if (bisHit)
                            {
                                Editor_NoteItem targetNoteItem = NoteEditScreenManager.Instance.GetNoteItem(targetNoteID);
                                m_curScore += GetSingleNoteScore(GetYPos2Time(targetNoteItem.RedAndBlueNoteItem.transform.position.y), ChiefGameManager.Instance.CurAudioSource.time);

                                targetNoteItem.gameObject.SetActive(false);
                            }
                        }
                        break;

                    case StageData.NoteData.NoteType.Long:
                        {
                            Editor_NoteItem targetNoteItem = NoteEditScreenManager.Instance.GetNoteItem(targetNoteID);
                            m_longNoteTable.Add(targetNoteID, new Tuple<float, float>(GetYPos2Time(targetNoteItem.GreenNoteItem.CurveStartYPos), ChiefGameManager.Instance.CurAudioSource.time));
                        }
                        break;
                }
            }
        }

        m_text_Score.gameObject.SetActive(true);
        m_text_Score.text = ((int)m_curScore).ToString();
    }

    internal static ScoreManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    internal bool BIsGreenRegion
    {
        set
        {
            //if(value)
            //{
            //    Debug.Log("Enter");
            //}
            //else
            //{
            //    Debug.Log("Exit");
            //}

            m_bisGreenRegion = value;
            if(value)
            {
                m_bisGreenRegionClear = value;
            }
        }
    }
    internal bool BIsMouseOnGreenLine
    {
        set
        {
            m_bisMouseOnGreenLine = value;
        }
    }

    internal void PushNoteCandidate(in Guid noteID)
    {
        m_noteCandidates.Push(noteID);

        SortNoteCandidates();
    }
    internal void RemoveNoteCandidate(in Guid noteID)
    {
        if(m_longNoteTable.ContainsKey(noteID))
        {
            m_curScore += GetSingleNoteScore(m_longNoteTable[noteID].Item1, m_longNoteTable[noteID].Item2);
            m_longNoteTable.Remove(noteID);
        }
        else
        {
            List<Guid> noteCandidateBuffer = m_noteCandidates.ToList();
            if(noteCandidateBuffer.Contains(noteID))
            {
                noteCandidateBuffer.Remove(noteID);

                Stack<Guid> resultNoteCandidates = new Stack<Guid>();
                foreach (Guid curNoteID in noteCandidateBuffer)
                {
                    resultNoteCandidates.Push(curNoteID);
                }

                m_noteCandidates = resultNoteCandidates;

                SortNoteCandidates();
            }
        }
    }

    private int GetCurPressedBasicNoteKeyCount()
    {
        IEnumerable<KeyCode> keyCodes = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>();
        
        List<KeyCode> keyCodeBuffer = keyCodes.ToList();
        foreach(KeyCode keyCode in m_bannedBasicNoteKeyCodes)
        {
            if(keyCodeBuffer.Contains(keyCode))
            {
                keyCodeBuffer.Remove(keyCode);
            }
        }

        keyCodes = keyCodeBuffer.Cast<KeyCode>();

        int curPressedKeyCount = keyCodes.Count(keyCode => Input.GetKey(keyCode));

        return curPressedKeyCount;
    }
    private int GetFreshPressedBasicNoteKeyCount()
    {
        IEnumerable<KeyCode> keyCodes = Enum.GetValues(typeof(KeyCode)).Cast<KeyCode>();

        List<KeyCode> keyCodeBuffer = keyCodes.ToList();
        foreach (KeyCode keyCode in m_bannedBasicNoteKeyCodes)
        {
            if (keyCodeBuffer.Contains(keyCode))
            {
                keyCodeBuffer.Remove(keyCode);
            }
        }

        keyCodes = keyCodeBuffer.Cast<KeyCode>();

        int curPressedKeyCount = keyCodes.Count(keyCode => Input.GetKeyDown(keyCode));

        return curPressedKeyCount;
    }

    private float GetSingleNoteScore(in float noteAppearTime, in float keyPressedTime)
    {
        //Debug.Log("noteAppearTime : " + noteAppearTime + " / keyPressedTime : " + keyPressedTime);

        int greenRegionCount = 0;
        foreach(StageData.RegionData regionItem in StageDataBuffer.Instance.CurStageData.Value.RegionDataTable.Values)
        {
            if(regionItem.CurColorType == StageData.RegionData.ColorType.Green)
            {
                greenRegionCount++;
            }
        }

        float resultScore = 1000000.0f / (StageDataBuffer.Instance.CurStageData.Value.NoteDataTable.Count + NoteEditScreenManager.Instance.RedLineCurveNotes.Count + greenRegionCount);

        float timeDistance = MathF.Abs(noteAppearTime - keyPressedTime);
        if(0.1f <= timeDistance && timeDistance < 0.2f)
        {
            resultScore *= 0.7f;
        }
        else if(0.2f <= timeDistance)
        {
            resultScore = 0.0f;
        }

        return resultScore;
    }

    private float GetYPos2Time(in float yPos)
    {
        float curNoteRatio = ((yPos - Editor_DetermineLine.Instance.transform.localPosition.y)
                            / (StageDataBuffer.Instance.CurStageData.Value.StageConfig.LengthPerBit / StageDataBuffer.Instance.CurStageData.Value.StageConfig.BitSubDivision)) / GridRenderManager.Instance.GetTotalBitCount();

        float curTime = ChiefGameManager.Instance.CurMusicClip.length * curNoteRatio;
        curTime -= StageDataBuffer.Instance.CurStageData.Value.StageConfig.MusicStartOffsetTime;

        return curTime;
    }

    private void SortNoteCandidates()
    {
        Dictionary<float, List<Guid>> noteByAppearYPos = new Dictionary<float, List<Guid>>();
        foreach(Guid noteID in m_noteCandidates)
        {
            //int curNoteStartOffsetFrame = StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[noteID].StartOffsetFrame;
            float curNoteAppearYPos = float.MinValue;
            if(StageDataBuffer.Instance.CurStageData.Value.NoteDataTable.ContainsKey(noteID))
            {
                curNoteAppearYPos = NoteEditScreenManager.Instance.GetNoteItem(noteID).transform.position.y;
            }
            else if(NoteEditScreenManager.Instance.GetRedLineCurvedNote(noteID) != null)
            {
                curNoteAppearYPos = NoteEditScreenManager.Instance.GetRedLineCurvedNote(noteID).transform.position.y;
            }

            if (!noteByAppearYPos.ContainsKey(curNoteAppearYPos))
            {
                noteByAppearYPos.Add(curNoteAppearYPos, new List<Guid>());
            }
            noteByAppearYPos[curNoteAppearYPos].Add(noteID);
        }

        foreach (float key in noteByAppearYPos.Keys)
        {
            noteByAppearYPos[key].Sort((noteID_0, noteID_1) =>
            {
                int result = 0;

                float appearYPos_0 = float.MinValue;
                if (StageDataBuffer.Instance.CurStageData.Value.NoteDataTable.ContainsKey(noteID_0))
                {
                    appearYPos_0 = NoteEditScreenManager.Instance.GetNoteItem(noteID_0).transform.position.y;
                }
                else if (NoteEditScreenManager.Instance.GetRedLineCurvedNote(noteID_0) != null)
                {
                    appearYPos_0 = NoteEditScreenManager.Instance.GetRedLineCurvedNote(noteID_0).transform.position.y;
                }

                float appearYPos_1 = float.MinValue;
                if (StageDataBuffer.Instance.CurStageData.Value.NoteDataTable.ContainsKey(noteID_1))
                {
                    appearYPos_1 = NoteEditScreenManager.Instance.GetNoteItem(noteID_1).transform.position.y;
                }
                else if (NoteEditScreenManager.Instance.GetRedLineCurvedNote(noteID_1) != null)
                {
                    appearYPos_1 = NoteEditScreenManager.Instance.GetRedLineCurvedNote(noteID_1).transform.position.y;
                }

                if (appearYPos_0 < appearYPos_1)
                {
                    result = -1;
                }
                else if (appearYPos_0 > appearYPos_1)
                {
                    result = 1;
                }

                return result;
            });
        }

        var sortedKeys = noteByAppearYPos.Keys.ToList();
        sortedKeys.Sort();

        Stack<Guid> resultNoteCandidates = new Stack<Guid>();
        foreach(float key in sortedKeys)
        {
            foreach(Guid noteID in noteByAppearYPos[key])
            {
                resultNoteCandidates.Push(noteID);
            }
        }

        m_noteCandidates = resultNoteCandidates;
    }
}