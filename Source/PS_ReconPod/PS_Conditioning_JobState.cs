using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PS_ReconPod
{
    public enum PS_Conditioning_JobState
    {
        UNSET = -1,
        Waiting = 0,
        Reconditioning = 2,
        Refreshing = 3,
        Deconditioning = 4,
        FixingBotch = 5
    }
}
