using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CommonUtilLib.ThreadSafe;


internal sealed class StageDataBuffer : SingleTon<StageDataBuffer>
{
    private StageData? m_curStageData;


    internal StageData? CurStageData
    {
        get
        {
            return m_curStageData;
        }
        set
        {
            m_curStageData = value;
        }
    }
}