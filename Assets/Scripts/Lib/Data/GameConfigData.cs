using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;


//[CreateAssetMenu(fileName = "GameConfigData", menuName = "RGBLine/GameConfigData")]
public struct GameConfigData
{
    [Serializable] public struct NoteHitJudgingStrandard
    {
        public enum HitJudgingType
        {
            Perfect,
            Good,
            Miss
        }


        public List<float> HitJudgingRanges;
    }


    public NoteHitJudgingStrandard noteHitJudgingStrandard;

    public float MusicVolume;
    public ChiefLobbyManager.MaxFrameRate MaxFrameRate;
    public int VSyncCount;
}