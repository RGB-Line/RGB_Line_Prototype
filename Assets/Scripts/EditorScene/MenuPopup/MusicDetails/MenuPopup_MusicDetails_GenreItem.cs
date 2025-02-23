using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class MenuPopup_MusicDetails_GenreItem : MonoBehaviour
{
    [SerializeField] private TMP_Text m_text_Genre;
    private Button m_button_Genre;
    private Image m_image_BackgroundColor;

    [SerializeField] private StageMetadata.MusicGenre m_curGenre;


    public void Awake()
    {
        m_button_Genre = GetComponent<Button>();
        m_image_BackgroundColor = GetComponent<Image>();
    }

    public void SetGenre()
    {
        //if(StageMetaDataBuffer.Instance.CurStageMetadata.Value.Genre.HasFlag(m_curGenre))
        //{
        //    StageMetaDataBuffer.Instance.CurStageMetadata = new StageMetadata()
        //    {
        //        ProfileImageBytes = StageMetaDataBuffer.Instance.CurStageMetadata.Value.ProfileImageBytes,
        //        Title = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Title,
        //        Length = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Length,
        //        Artist = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Artist,
        //        Description = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Description,
        //        Genre = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Genre & ~m_curGenre,
        //        Difficulty = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Difficulty
        //    };

        //    SetSelectedState(false);
        //}
        //else
        //{
        //    StageMetaDataBuffer.Instance.CurStageMetadata = new StageMetadata()
        //    {
        //        ProfileImageBytes = StageMetaDataBuffer.Instance.CurStageMetadata.Value.ProfileImageBytes,
        //        Title = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Title,
        //        Length = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Length,
        //        Artist = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Artist,
        //        Description = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Description,
        //        Genre = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Genre | m_curGenre,
        //        Difficulty = StageMetaDataBuffer.Instance.CurStageMetadata.Value.Difficulty
        //    };

        //    SetSelectedState(true);
        //}
    }

    internal void SetInteractable(in bool bIsInteractable)
    {
        m_button_Genre.interactable = bIsInteractable;
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