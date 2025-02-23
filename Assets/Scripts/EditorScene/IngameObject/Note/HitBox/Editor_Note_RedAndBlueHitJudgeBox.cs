using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class Editor_Note_RedAndBlueHitJudgeBox : MonoBehaviour
{
    private Guid m_attachedNoteID;


    internal void Init(in Guid attachedNoteID)
    {
        m_attachedNoteID = attachedNoteID;
    }

    internal Guid AttachedNoteID
    {
        get
        {
            return m_attachedNoteID;
        }
    }
}