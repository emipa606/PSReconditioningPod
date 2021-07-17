using Verse;

namespace PS_ReconPod
{
    public static class PS_Extensions
    {
        public static float ConditioningLevel(this Pawn pawn)
        {
            if (!PS_ConditioningHelper.IsReconditioned(pawn))
            {
                return -1f;
            }

            var need = pawn.needs.TryGetNeed<PS_Needs_Reconditioning>();
            if (need != null)
            {
                return need.CurLevel;
            }

            Log.Error("PS_Extensions: Tried to GetCurrentConditioningLevel but failed to find need");
            return -1f;
        }
    }
}