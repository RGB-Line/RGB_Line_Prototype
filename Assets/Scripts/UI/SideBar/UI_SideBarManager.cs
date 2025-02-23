using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using EasyTechToolUI.ItemViewList;


public sealed class UI_SideBarManager : ItemViewList
{
    [Header("Page Type Data")]
    [SerializeField] private UIPageType m_uiPageType;


    public override void InitializeModule(in object moduleInitData, in Guid attahcedCanvasTransitionManagerGuid)
    {
        base.InitializeModule(moduleInitData, attahcedCanvasTransitionManagerGuid);

        for (int index = 0; index < m_uiPageType.UIPages.Count; index++)
        {
            AddItem(new UI_SideBar_Item.ItemInitData()
            {
                PageName = m_uiPageType.UIPages[index]
            });
        }
    }
}