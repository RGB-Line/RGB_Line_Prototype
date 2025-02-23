using System.Collections;
using System.Collections.Generic;
using System.IO;

using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MenuPopup_MusicDetails_DifficultyItem : MonoBehaviour
{
    private static StageMetadata.DifficultyLevel m_curDifficulty;

    private Image m_image_BackgroundColor;

    [SerializeField] private StageMetadata.DifficultyLevel m_difficulty;
    [SerializeField] private TMP_InputField m_inputField_StageName;


    public void Awake()
    {
        m_image_BackgroundColor = GetComponent<Image>();

        m_curDifficulty = StageMetadata.DifficultyLevel.Easy;
    }
    public void FixedUpdate()
    {
        if(StageMetaDataBuffer.Instance.CurStageMetadata.HasValue)
        {
            m_curDifficulty = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Difficulty;
        }

        if (m_curDifficulty == m_difficulty)
        {
            SetSelectedState(true);
        }
        else
        {
            SetSelectedState(false);
        }
    }

    internal static StageMetadata.DifficultyLevel CurDifficulty
    {
        get
        {
            return m_curDifficulty;
        }
    }

    public void SetDifficulty()
    {
        m_curDifficulty = m_difficulty;

        if (string.IsNullOrEmpty(m_inputField_StageName.text))
        {
            return;
        }
        else if(File.Exists(DataPathInterface.GetStageDataPath(m_inputField_StageName.text, m_difficulty)))
        {
            return;
        }

        StageMetaDataBuffer.Instance.CurStageMetadata = new StageMetadata()
        {
            ProfileImageBytes = StageMetaDataBuffer.Instance.CurStageMetadata.Value.ProfileImageBytes,
            Title = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Title,
            Length = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Length,
            Artist = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Artist,
            //Description = m_musicDetailUI.m_inputField_MusicDescription.text,
            //Genre = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Genre,
            Difficulty = m_difficulty,
            BestScore = StageMetaDataBuffer.Instance.CurStageMetadata.Value.BestScore
        };
    }

    internal void SetSelectedState(in bool bisSelected)
    {
        if (bisSelected)
        {
            m_image_BackgroundColor.color = new Color(199 / 255.0f, 125 / 255.0f, 72 / 255.0f);
        }
        else
        {
            m_image_BackgroundColor.color = new Color(33 / 255.0f, 33 / 255.0f, 33 / 255.0f);
        }
    }
}