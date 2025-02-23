using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using UnityEngine;
using System.Security.Cryptography.X509Certificates;


internal static class DataPathInterface
{
    internal static string GetStageDataPath(in string stageName, in StageMetadata.DifficultyLevel difficulty)
    {
        string path = Path.Combine(Application.streamingAssetsPath, DataPathManager.Instance.DataPathTable.StagesBasePath);
        path = Path.Combine(path, stageName);
        switch (difficulty)
        {
            case StageMetadata.DifficultyLevel.Easy:
                path = Path.Combine(path, "Easy");
                break;

            case StageMetadata.DifficultyLevel.Normal:
                path = Path.Combine(path, "Normal");
                break;

            case StageMetadata.DifficultyLevel.Hard:
                path = Path.Combine(path, "Hard");
                break;
        }
        path = Path.Combine(path, DataPathManager.Instance.DataPathTable.StageDataFilePathTemplate + stageName);
        path = Path.ChangeExtension(path, DataPathManager.Instance.DataPathTable.StageDataFileExtension);

        return path;
    }
    internal static string GetStageMusicPath(in string stageName, in string musicExtention)
    {
        string path = Path.Combine(Application.streamingAssetsPath, DataPathManager.Instance.DataPathTable.StagesBasePath);
        path = Path.Combine(path, stageName);
        path = Path.Combine(path, stageName + musicExtention);

        return path;
    }
    internal static string GetStageMetaDataPath(in string stageName, in StageMetadata.DifficultyLevel difficulty)
    {
        string path = Path.Combine(Application.streamingAssetsPath, DataPathManager.Instance.DataPathTable.StagesBasePath);
        path = Path.Combine(path, stageName);
        switch (difficulty)
        {
            case StageMetadata.DifficultyLevel.Easy:
                path = Path.Combine(path, "Easy");
                break;

            case StageMetadata.DifficultyLevel.Normal:
                path = Path.Combine(path, "Normal");
                break;

            case StageMetadata.DifficultyLevel.Hard:
                path = Path.Combine(path, "Hard");
                break;
        }
        path = Path.Combine(path, DataPathManager.Instance.DataPathTable.StageMetaDataFilePathTemplate + stageName);
        path = Path.ChangeExtension(path, DataPathManager.Instance.DataPathTable.StageMetaDataFileExtension);

        return path;
    }

    internal static string[] GetStageNames()
    {
        string[] stageNames = Directory.GetDirectories(Path.Combine(Application.streamingAssetsPath, DataPathManager.Instance.DataPathTable.StagesBasePath));
        for(int index = 0; index < stageNames.Length; index++)
        {
            stageNames[index] = Path.GetFileName(stageNames[index]);
        }

        return stageNames;
    }

    internal static string GetConfigPath()
    {
        string path = Path.Combine(Application.streamingAssetsPath, DataPathManager.Instance.DataPathTable.ConfigBasePath);
        path = Path.Combine(path, DataPathManager.Instance.DataPathTable.ConfigFileName);
        path = Path.ChangeExtension(path, DataPathManager.Instance.DataPathTable.ConfigFileExtension);

        return path;
    }
}