using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;


public sealed class CameraGuidingLine : MonoBehaviour
{
    [SerializeField] private GameObject m_prefab_CurvedLineParent;
    [SerializeField] private GameObject m_prefab_CurvedLinePoint;


    internal Vector3[] GetCameraGuidingPoses()
    {
        #region Spawn Guiding Line
        Dictionary<float, List<float>> objectXPosTable = new Dictionary<float, List<float>>();

        // Common, Flip Note
        foreach (Guid noteID in StageDataBuffer.Instance.CurStageData.Value.NoteDataTable.Keys)
        {
            if (StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[noteID].CurNoteType == StageData.NoteData.NoteType.Common || StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[noteID].CurNoteType == StageData.NoteData.NoteType.Flip)
            {
                float yPos = GridRenderManager.Instance.GetFramePosition(StageDataBuffer.Instance.CurStageData.Value.RegionDataTable[StageDataBuffer.Instance.CurStageData.Value.LineDataTable[StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[noteID].AttachedLineID].AttachedRegionID].StartOffsetFrame)
                             + GridRenderManager.Instance.GetFramePosition(StageDataBuffer.Instance.CurStageData.Value.LineDataTable[StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[noteID].AttachedLineID].CurvedLinePoints[0].Y)
                             + GridRenderManager.Instance.GetFramePosition(StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[noteID].StartOffsetFrame);

                if(!objectXPosTable.ContainsKey(yPos))
                {
                    objectXPosTable.Add(yPos, new List<float>());
                }

                //objectXPosTable[yPos].Add(Editor_NoteItem.GetNoteXPos(StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[noteID].StartOffsetFrame,
                //                                                      StageDataBuffer.Instance.CurStageData.Value.NoteDataTable[noteID].AttachedLineID));
            }
        }

        // Long

        // Red Line

        // Green Line

        // Spawn Guiding Line
        var sortedKeys = objectXPosTable.Keys.ToList();
        sortedKeys.Sort();


        #endregion

        throw new NotImplementedException();
    }
}