using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using TMPro;


public class LogMessage : MonoBehaviour
{
    private TMP_Text m_text_LogMessage;

    [SerializeField] private float m_logExpirationTime = 15.0f;
    private float m_logFadeTimer = 0.0f;


    public void Awake()
    {
        m_text_LogMessage = GetComponent<TMP_Text>();
    }
    public void FixedUpdate()
    {
        m_logExpirationTime -= Time.deltaTime;
        if (m_logExpirationTime <= m_logFadeTimer)
        {
            Color currentColor = m_text_LogMessage.color;
            currentColor.a = m_logExpirationTime / m_logFadeTimer;
            m_text_LogMessage.color = currentColor;
        }

        if (m_logExpirationTime <= 0.0f)
        {
            Destroy(gameObject);
        }
    }

    internal void Initialize(in string message)
    {
        m_text_LogMessage.text += message;
    }
}