using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class DetailEditScreenManager : MonoBehaviour
{
    public enum DetailEditScreenType
    {
        None,
        Region,
        Line,
        Note
    }


    private static DetailEditScreenManager m_instance;

    [SerializeField] private GameObject[] m_gameObject_DetailEditScreens;


    public void Awake()
    {
        m_instance = this;
    }

    public static DetailEditScreenManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    internal DetailEditScreenType CurDetailEditScreenType { get; private set; } = DetailEditScreenType.None;

    public void OpenScreen(int targetScreenIndex)
    {
        for (int index = 0; index < m_gameObject_DetailEditScreens.Length; index++)
        {
            if(index == targetScreenIndex)
            {
                continue;
            }

            m_gameObject_DetailEditScreens[index].SetActive(false);
            ClearSelectModePerDetailScreen(index);
        }

        if (m_gameObject_DetailEditScreens[targetScreenIndex].activeSelf)
        {
            m_gameObject_DetailEditScreens[targetScreenIndex].SetActive(false);
            ClearSelectModePerDetailScreen(targetScreenIndex);

            CurDetailEditScreenType = DetailEditScreenType.None;
        }
        else
        {
            m_gameObject_DetailEditScreens[targetScreenIndex].SetActive(true);

            switch(targetScreenIndex)
            {
                case 0:
                    CurDetailEditScreenType = DetailEditScreenType.Region;
                    break;

                case 1:
                    CurDetailEditScreenType = DetailEditScreenType.Line;
                    break;

                case 2:
                    CurDetailEditScreenType = DetailEditScreenType.Note;
                    break;
            }
        }
    }

    private void ClearSelectModePerDetailScreen(in int targetScreenIndex)
    {
        switch (targetScreenIndex)
        {
            case 0:
                RegionEditScreenManager.Instance.ClearSelectMode();
                break;

            case 1:
                LineEditScreenManager.Instance.ClearSelectMode();
                break;

            case 2:
                NoteEditScreenManager.Instance.ClearSelectMode();
                break;
        }
    }
}