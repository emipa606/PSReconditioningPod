using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace PS_ReconPod 
{
    public class PS_Hediff_Reconditioned : Hediff
    {
        //public TraitAlterType ChangeType;
        //public TraitDef OrigonalTraitDef;
        //public TraitDef AddedTraitDef;

        // Old: to support pre-multi-conditioning
        public PS_Conditioning_Data ConditioningData;
        public List<PS_Conditioning_Data> ConditioningDataList;
        private bool CheckedConData;
        
        public PS_Hediff_Reconditioned()
        {
            def = DefDatabase<HediffDef>.GetNamed("PS_Hediff_Reconditioned");
            CheckedConData = false;
            ConditioningDataList = new List<PS_Conditioning_Data>();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look<PS_Conditioning_Data>(ref ConditioningData, "ConditionData");
            Scribe_Collections.Look<PS_Conditioning_Data>(ref ConditioningDataList, "ConditionDataList");
            //Scribe_Values.Look<TraitAlterType>(ref this.ChangeType, "ChangeType", TraitAlterType.UNSET, false);
            //Scribe_Defs.Look<TraitDef>(ref this.OrigonalTraitDef, "OrigonalTraitDef");
            //Scribe_Defs.Look<TraitDef>(ref this.AddedTraitDef, "AddedTraitDef");
        }

        public override string LabelBase => "Reconditioned";//switch (this.ConditioningData.AlterType)//{//    case TraitAlterType.Added://        return "PS_HediffReconditionedAddedLab".Translate() + " " + this.ConditioningData.AddedTraitLabel;//    case TraitAlterType.Removed://        return "PS_HediffReconditionedRemovedLab".Translate() + " " + this.ConditioningData.OrigonalTraitLabel;//    case TraitAlterType.Altered://        return "PS_HediffReconditionedAlteredLab".Translate() + " " + this.ConditioningData.OrigonalTraitLabel;//    case TraitAlterType.UNSET://        Log.Error("PS_Hediff_Reconditioned: Tried to get label of hediff with ChangeType = UNSET.");//        return "ERROR";//    default://        Log.Error("PS_Hediff_Reconditioned: Tried to get label of hediff with unknown ChangeType.");//        return "ERROR";//}

        private void CheckForOldConData()
        {
            if (ConditioningData != null)
            {
                if (ConditioningDataList == null || !ConditioningDataList.Any())
                {
                    Log.Message(string.Format("PS_Hediff_Reconditoning: Condition data for {0} found as single and not list. This is due to multi-condition update. Building list now.", pawn.LabelShort));
                    ConditioningDataList = new List<PS_Conditioning_Data>
                    {
                        new PS_Conditioning_Data
                        {
                            PawnId = ConditioningData.PawnId,
                            AddedTraitDefName = ConditioningData.AddedTraitDefName,
                            OriginalTraitDefName = ConditioningData.OriginalTraitDefName,
                            AddedDegree = ConditioningData.AddedDegree,
                            OriginalDegree = ConditioningData.OriginalDegree,
                            AlterType = ConditioningData.AlterType
                        }
                    };
                    ConditioningData = null;
                    return;
                }

                else if (!ConditioningDataList.Where(x => x.IsSame(ConditioningData)).Any())
                {
                    Log.Message(string.Format("PS_Hediff_Reconditoning: Condition data for {0} found as single and not list due to update. But list was already occupied.", pawn.LabelShort));
                    ConditioningDataList.Add(new PS_Conditioning_Data
                    {
                        PawnId = ConditioningData.PawnId,
                        AddedTraitDefName = ConditioningData.AddedTraitDefName,
                        OriginalTraitDefName = ConditioningData.OriginalTraitDefName,
                        AddedDegree = ConditioningData.AddedDegree,
                        OriginalDegree = ConditioningData.OriginalDegree,
                        AlterType = ConditioningData.AlterType
                    });
                    ConditioningData = null;
                }

                var tempList = new List<PS_Conditioning_Data>();
                tempList = ConditioningDataList.Where(x => x.IsValid()).ToList();
                ConditioningDataList.Clear();
                ConditioningDataList = tempList;
            }
        }

        public override void Tick()
        {
            base.Tick();
            if(!CheckedConData)
            {
                CheckForOldConData();
                CheckedConData = true;
            }
        }
    }
}
