using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class RegionFadeoutEffect : MonoBehaviour
{
    private MeshRenderer m_meshRenderer;

    private bool m_bisStartEffect;

    private float m_timeout;
    private float m_curTime;

    private Color m_prevBaseColor;
    private Color m_targetBaseColor;

    private Vector2 m_effectStartPos;

    //private bool m_bisInited = false;


    public void Awake()
    {
        m_meshRenderer = GetComponent<MeshRenderer>();
    }
    public void Update()
    {
        if (m_bisStartEffect)
        {
            m_curTime += Time.deltaTime;

            m_meshRenderer.material.SetFloat("_Radius", Mathf.Lerp(10000.0f, 0.0f, (m_curTime / m_timeout)));
            m_meshRenderer.material.SetColor("_BaseColor", m_targetBaseColor);
            m_meshRenderer.material.SetColor("_PrevBaseColor", m_prevBaseColor);
            m_meshRenderer.material.SetFloat("_EffectStartPos_X", m_effectStartPos.x);
            m_meshRenderer.material.SetFloat("_EffectStartPos_Y", m_effectStartPos.y);

            if (m_curTime >= m_timeout)
            {
                m_bisStartEffect = false;
            }
        }
    }

    internal float Timeout
    {
        set
        {
            m_timeout = value;
        }
    }

    public void CalculateFadeout(in Color targetBaseColor, in Vector2 effectStartPos)
    {
        m_bisStartEffect = true;
        m_curTime = 0.0f;

        m_prevBaseColor = m_targetBaseColor;
        m_targetBaseColor = targetBaseColor;

        //if(!m_bisInited)
        //{
        //    m_prevBaseColor = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        //    m_bisInited = true;
        //}

        m_effectStartPos = effectStartPos;
    }
}