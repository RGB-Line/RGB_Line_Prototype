using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;    

using CommonUtilLib.ThreadSafe;

using Newtonsoft.Json;


internal sealed class GameConfigDataBuffer : SingleTon<GameConfigDataBuffer>
{
    private GameConfigData? m_gameConfigData;


    internal GameConfigData? GameConfigData
    {
        get
        {
            return m_gameConfigData;
        }
        set
        {
            m_gameConfigData = value;
        }
    }

    internal void LoadGameConfigData()
    {
        string path = DataPathInterface.GetConfigPath();
        if (!File.Exists(path))
        {
            m_gameConfigData = new GameConfigData()
            {
                noteHitJudgingStrandard = new GameConfigData.NoteHitJudgingStrandard()
                {
                    HitJudgingRanges = new List<float>() { 50.0f, 100.0f, 200.0f }
                },
                MusicVolume = 0.8f,
                MaxFrameRate = ChiefLobbyManager.MaxFrameRate.Frame_60,
                VSyncCount = 0
            };
            SaveGameConfigData();
            return;
        }

        string jsonData = File.ReadAllText(path);
        m_gameConfigData = JsonConvert.DeserializeObject<GameConfigData>(jsonData);
    }
    internal void SaveGameConfigData()
    {
        string path = DataPathInterface.GetConfigPath();
        string directoryName = Path.GetDirectoryName(path);
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        string jsonData = JsonConvert.SerializeObject(m_gameConfigData);
        File.WriteAllText(path, jsonData);
    }
}