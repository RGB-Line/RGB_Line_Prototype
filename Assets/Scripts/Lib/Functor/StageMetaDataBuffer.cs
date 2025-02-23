using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommonUtilLib.ThreadSafe;


internal sealed class StageMetaDataBuffer : SingleTon<StageMetaDataBuffer>
{
    private StageMetadata? m_curStageMetadata;


    internal StageMetadata? CurStageMetadata
    {
        get
        {
            return m_curStageMetadata;
        }
        set
        {
            m_curStageMetadata = value;
        }
    }
}