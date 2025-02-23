using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class LoggingManager : MonoBehaviour
{
    internal enum LogType
    {
        Common,
        Error,
        Warning
    }


    private static LoggingManager m_instance;

    [SerializeField] private Transform m_transform_LoggingParent;
    [SerializeField] private GameObject m_prefab_CommonLog;
    [SerializeField] private GameObject m_prefab_ErrorLog;
    [SerializeField] private GameObject m_prefab_WarningLog;


    public void Awake()
    {
        m_instance = this;

        if(m_transform_LoggingParent.childCount > 0)
        {
            for (int i = 0; i < m_transform_LoggingParent.childCount; i++)
            {
                Destroy(m_transform_LoggingParent.GetChild(i).gameObject);
            }
        }
    }

    public static LoggingManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    internal void AssertLogMessage(in LogType logType, in string message)
    {
        switch (logType)
        {
            case LogType.Common:
                {
                    GameObject commonGameObject = Instantiate(m_prefab_CommonLog, m_transform_LoggingParent);
                    commonGameObject.GetComponent<LogMessage>().Initialize(message);
                }
                break;

            case LogType.Error:
                {
                    GameObject errorGameObject = Instantiate(m_prefab_ErrorLog, m_transform_LoggingParent);
                    errorGameObject.GetComponent<LogMessage>().Initialize(message);
                }
                break;

            case LogType.Warning:
                {
                    GameObject warningGameObject = Instantiate(m_prefab_WarningLog, m_transform_LoggingParent);
                    warningGameObject.GetComponent<LogMessage>().Initialize(message);
                }
                break;
        }
    }
}