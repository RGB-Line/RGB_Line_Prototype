using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using EasyTechToolUI.ItemViewList;
using TMPro;
using UnityEngine.UI;


public sealed class UI_SideBar_Item : ItemViewList.Item
{
    internal sealed class ItemInitData
    {
        public string PageName { get; set; }
    }


    [SerializeField] private TMP_Text m_text_PageName;
    [SerializeField] private Image m_image_AcctiveLine;


    public override void InitializeItem(in ItemViewList itemViewList, in object itemInitData)
    {
        base.InitializeItem(itemViewList, itemInitData);

        m_text_PageName.text = (itemInitData as ItemInitData).PageName;
    }

    public override void OnItemSelected(in object itemData, in bool bisSelected)
    {
        if (bisSelected)
        {
            m_image_AcctiveLine.color = new Color(0.9686275f, 0.5058824f, 0.4f, 1.0f);
        }
        else
        {
            m_image_AcctiveLine.color = Color.clear;
        }
    }
}