using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;


public sealed class DataPathManager : MonoBehaviour
{
    private static DataPathManager m_instance;

    [SerializeField] private DataPathTable m_dataPathTable;


    public void Awake()
    {
        m_instance = this;
    }

    internal static DataPathManager Instance
    {
        get
        {
            return m_instance;
        }
    }

    internal DataPathTable DataPathTable
    {
        get
        {
            return m_dataPathTable;
        }
    }
}