using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public struct StageData
{
    public struct RegionData
    {
        public enum ColorType
        {
            Red, Green, Blue
        }


        public int StartOffsetFrame;
        public float MinorOffsetTime;
        public ColorType CurColorType;
    }
    public struct LineData
    {
        public enum LineSmoothType
        {
            Linear, Curved
        }


        public Guid AttachedRegionID;

        //public int StartOffsetFrame;
        //public int EndOffsetFrame;

        /// <summary>
        /// 기본 가정으로 CurvedLinePoints는 Y 좌표 기준으로 정렬되어 있음
        /// </summary>
        public List<HalfFloatVector2> CurvedLinePoints;
        public List<float> MinorOffsetTimes;
        public float LineWidth;

        public LineSmoothType CurLineSmoothType;
    }
    public struct NoteData
    {
        public enum NoteType
        {
            Common,
            Flip,
            Long
        }
        public enum FlipNoteDirection
        {
            Left, Right
        }


        public Guid AttachedLineID;

        public int StartOffsetFrame;
        public float MinorOffsetTime;
        public float NoteLength;
        public FlipNoteDirection flipNoteDirection;

        public NoteType CurNoteType;
    }
    public struct StageConfigData
    {
        public int BPM;
        public int BitSubDivision;
        public float LengthPerBit;
        public float MusicStartOffsetTime;
    }


    /// <summary>
    /// 기본 가정으로 RegionDataTable는 StartOffsetFrame 기준으로 정렬되어 있음
    /// </summary>
    public Dictionary<Guid, RegionData> RegionDataTable;
    public Dictionary<Guid, LineData> LineDataTable;
    public Dictionary<Guid, NoteData> NoteDataTable;

    public StageConfigData StageConfig;
}

public struct StageMetadata
{
    [Obsolete]
    [Flags]
    public enum MusicGenre : byte
    {
        None = 0b0000_0000,
        EDM = 0b0000_0001,
        Pop = 0b0000_0010,
        Rock = 0b0000_0100,
        Jaze = 0b0000_1000,
        Classic = 0b0001_0000,
        HipHop = 0b0010_0000,
    }
    public enum DifficultyLevel : byte
    {
        Easy, Normal, Hard
    }


    public byte[] ProfileImageBytes;
    public string Title;
    public string Artist;
    //public string Description;
    //public MusicGenre Genre;
    public DifficultyLevel Difficulty;
    public float Length;
    public int BestScore;
}

public struct HalfFloatVector2
{
    public float X;
    public int Y;
}