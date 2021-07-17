using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PS_ReconPod
{
    // Token: 0x02000039 RID: 57
    public class PS_Needs_Reconditioning : Need
    {
        private readonly bool CheckedForBadSave;
        private float _fallPerDay;
        public bool FallPerDayIsDirty = true;

        // Token: 0x060000DC RID: 220 RVA: 0x00007B7B File Offset: 0x00005D7B
        public PS_Needs_Reconditioning(Pawn pawn)
        {
            this.pawn = pawn;
            FallPerDayIsDirty = true;
        }

        public float FallPerDay
        {
            get
            {
                if (!FallPerDayIsDirty)
                {
                    return _fallPerDay;
                }

                _fallPerDay = PS_ConditioningHelper.GetNeedFallPerDay(pawn);
                FallPerDayIsDirty = false;

                return _fallPerDay;
            }
        }

        private float FallPerTic => FallPerDay / 60000f;

        // Token: 0x17000016 RID: 22
        // (get) Token: 0x060000DD RID: 221 RVA: 0x00007B8C File Offset: 0x00005D8C
        public override float MaxLevel => 1f;

        // Token: 0x1700032F RID: 815
        // (get) Token: 0x0600177C RID: 6012 RVA: 0x000B8A59 File Offset: 0x000B6E59
        public ConditioningCategory CurCategory
        {
            get
            {
                if (base.CurLevel < 0.25f)
                {
                    return ConditioningCategory.Slipping;
                }

                if (base.CurLevel < 0.5f)
                {
                    return ConditioningCategory.Weakened;
                }

                if (base.CurLevel < 0.75f)
                {
                    return ConditioningCategory.Strong;
                }

                return ConditioningCategory.Fresh;
            }
        }

        // Token: 0x060000DE RID: 222 RVA: 0x00007B93 File Offset: 0x00005D93
        public override void SetInitialLevel()
        {
            CurLevel = 1f;
        }

        // Token: 0x060000DF RID: 223 RVA: 0x00007BA4 File Offset: 0x00005DA4
        public override void DrawOnGUI(Rect rect, int maxThresholdMarkers = 2147483647, float customMargin = -1f,
            bool drawArrows = true, bool doTooltip = true, Rect? rectForTooltip = null)
        {
            if (threshPercents == null)
            {
                threshPercents = new List<float>();
            }

            threshPercents.Clear();
            threshPercents.Add(0.25f);
            threshPercents.Add(0.5f);
            threshPercents.Add(0.75f);
            base.DrawOnGUI(rect, maxThresholdMarkers, customMargin, drawArrows, doTooltip, rectForTooltip);
        }

        // Token: 0x060000E0 RID: 224 RVA: 0x00007C08 File Offset: 0x00005E08
        public override void NeedInterval()
        {
            var inPod = pawn.ParentHolder != null && pawn.ParentHolder.GetType() == typeof(PS_Buildings_ReconPod);
            if (!inPod && !base.IsFrozen)
            {
                CurLevel -= FallPerTic * 150f;
            }

            var hediff = pawn.health.hediffSet.GetHediffs<PS_Hediff_Reconditioned>().FirstOrDefault();
            if (hediff == null)
            {
                Log.Error("PS_Needs_Reconditioning: failed to find PS_Hediff_Reconditined");
                return;
            }

            if (hediff.ConditioningDataList == null || !hediff.ConditioningDataList.Any())
            {
                Log.Error(
                    "PS_Needs_Reconditioning: Need interval found hediff but no data. Probably from bad save file. Removeing conditioning.");
                PS_ConditioningHelper.ClearBuggedConditioning(pawn);
                return;
            }


            var map = pawn.Map;
            if (!inPod && !pawn.IsCaravanMember() && map == null)
            {
                Log.Message("PS_Needs_Reconditioning: " + pawn.LabelShort +
                            " is not in a caravan or pod but map is null, not sure what this means but they can't find a pod");
                return;
            }

            // Try to take conditionall in in caravan
            if (CurLevel <= 0.5f && pawn.IsCaravanMember())
            {
                var caravan = pawn.GetCaravan();
                var stack = caravan.Goods.FirstOrDefault(x => x.def.defName == "PS_Drugs_Conditionall");
                if (stack != null)
                {
                    var pill = stack.SplitOff(1);
                    CurLevel += 0.25f;
                    pill.Destroy();
                }
            }


            if (CurLevel > 0)
            {
                hediff.Severity = CurLevel;
            }
            else if (pawn.InMentalState)
            {
                hediff.Severity = 0.0001f;
            }
            else // Undo Conditioning
            {
                var state = GetMentalState(pawn, hediff.ConditioningDataList);
                if (!pawn.mindState.mentalStateHandler.TryStartMentalState(state, "PS_ReconWoreOffMessage".Translate(),
                    true))
                {
                    Log.Error("PS_Need_Recon: Failed to give mental state " + state.defName);
                }

                PS_PodFinder.FindMyPod(pawn).TryUnassignPawn(pawn);
                PS_ConditioningHelper.UndoAllConditioning(pawn);
                Messages.Message(string.Format("PS_Messages_CompletedDeconditioning".Translate(), pawn.LabelShort),
                    new LookTargets(pawn), MessageTypeDefOf.TaskCompletion);
                hediff.Severity = 0;
            }
        }

        public static MentalStateDef GetMentalState(Pawn pawn, List<PS_Conditioning_Data> conData)
        {
            // Check if removed trait has break
            var breaksFromRemoved = new List<MentalStateDef>();
            foreach (var data in conData)
            {
                if (data.AlterType != TraitAlterType.Removed)
                {
                    continue;
                }

                var orgTrait = new Trait(DefDatabase<TraitDef>.GetNamed(data.OriginalTraitDefName));
                var degree = PS_TraitHelper.GetDegreeDate(orgTrait);
                if (degree == null)
                {
                    continue;
                }

                var count = degree.theOnlyAllowedMentalBreaks?.Count ?? 0;
                if (count > 0)
                {
                    breaksFromRemoved.AddRange(degree.theOnlyAllowedMentalBreaks!.Select(x => x.mentalState));
                }
            }

            if (breaksFromRemoved.Any())
            {
                return breaksFromRemoved[Rand.Range(0, breaksFromRemoved.Count - 1)];
            }


            // Check if added trait disallows break
            var breaksFromAdded = new List<MentalStateDef>();
            foreach (var data in conData)
            {
                if (data.AlterType != TraitAlterType.Added)
                {
                    continue;
                }

                var addTrait = new Trait(DefDatabase<TraitDef>.GetNamed(data.AddedTraitDefName));
                var degree = PS_TraitHelper.GetDegreeDate(addTrait);
                if (degree == null)
                {
                    continue;
                }

                var count = degree.disallowedMentalStates?.Count ?? 0;
                if (count > 0)
                {
                    breaksFromAdded.AddRange(degree.disallowedMentalStates!);
                }
            }

            if (breaksFromAdded.Any())
            {
                return breaksFromAdded[Rand.Range(0, breaksFromAdded.Count - 1)];
            }

            // Otherwise get all allowable mental traits
            var naturalTraits = PS_ConditioningHelper.GetNaturalTraits(pawn);
            var degrees = naturalTraits.SelectMany(x => x.def.degreeDatas).Distinct();
            var forbiden = degrees.Where(x => (x.disallowedMentalStates?.Count ?? 0) > 0)
                .SelectMany(x => x.disallowedMentalStates).ToList();
            var allowed = DefDatabase<MentalStateDef>.AllDefs
                .Where(x => !forbiden.Contains(x) && x.colonistsOnly && x.Worker.StateCanOccur(pawn)).ToList();

            return allowed[Rand.Range(0, allowed.Count - 1)];
        }
    }
}