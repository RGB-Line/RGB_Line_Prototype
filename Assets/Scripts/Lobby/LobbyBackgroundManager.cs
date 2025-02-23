using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using System;


public class LobbyBackgroundManager : MonoBehaviour
{
    private static LobbyBackgroundManager m_instance;

    [SerializeField] private List<Material> m_mats;
    private int m_randomMatSelectIndex;

    private MeshRenderer m_meshRenderer;


    public void Awake()
    {
        m_instance = this;

        m_meshRenderer = GetComponent<MeshRenderer>();

        m_randomMatSelectIndex = UnityEngine.Random.Range(0, m_mats.Count);
    }
    public void FixedUpdate()
    {
        m_meshRenderer.material = m_mats[m_randomMatSelectIndex];
        //if (Input.GetMouseButton(0))
        //{
        //    Vector3 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //    Vector3 pos = new Vector3()
        //    {
        //        x = (worldPos.x + transform.localScale.x / 2.0f) / transform.localScale.x,
        //        y = (worldPos.y + transform.localScale.y / 2.0f) / transform.localScale.y,
        //        z = 0.0f
        //    };

        //    m_meshRenderer.material.SetColor("_MousePos", new Color(pos.x, pos.y, pos.z));
        //    m_meshRenderer.material.SetInt("_MouseClicked", 1);
        //}
        //else
        //{
        //    m_meshRenderer.material.SetInt("_MouseClicked", 0);
        //}
    }

    internal static LobbyBackgroundManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    internal int RandomMatSelectIndex
    {
        get
        {
            return m_randomMatSelectIndex;
        }
        set
        {
            m_randomMatSelectIndex = value;
        }
    }
}