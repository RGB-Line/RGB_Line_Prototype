using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;


[CreateAssetMenu(fileName = "DataPathTable", menuName = "RGBLine/DataPathTable", order = 1)]
internal class DataPathTable : ScriptableObject
{
    [Header("Stage")]
    public string StagesBasePath;

    public string StageDataFilePathTemplate;
    public string StageDataFileExtension;

    public string StageMetaDataFilePathTemplate;
    public string StageMetaDataFileExtension;

    [Header("Config")]
    public string ConfigBasePath;

    public string ConfigFileName;
    public string ConfigFileExtension;
}