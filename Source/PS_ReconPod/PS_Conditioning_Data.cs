using RimWorld;
using Verse;

namespace PS_ReconPod;

public class PS_Conditioning_Data : IExposable
{
    public int AddedDegree;
    public string AddedTraitDefName;
    public TraitAlterType AlterType = TraitAlterType.UNSET;
    public int OriginalDegree;
    public string OriginalTraitDefName;
    public string PawnId;

    private string AddedTraitLabel =>
        new Trait(DefDatabase<TraitDef>.GetNamed(AddedTraitDefName), AddedDegree).Label;

    private string OriginalTraitLabel =>
        new Trait(DefDatabase<TraitDef>.GetNamed(OriginalTraitDefName), OriginalDegree).Label;

    public void ExposeData()
    {
        Scribe_Values.Look(ref PawnId, "PawnId");
        Scribe_Values.Look(ref AddedTraitDefName, "AddedTraitDefName");
        Scribe_Values.Look(ref OriginalTraitDefName, "OrigonalTraitDefName");
        Scribe_Values.Look(ref AddedDegree, "AddedDegree");
        Scribe_Values.Look(ref OriginalDegree, "OrigonalDegree");
        Scribe_Values.Look(ref AlterType, "AlterType");
    }

    public override string ToString()
    {
        return
            $"[PawnId: {PawnId}, Change: {AlterType.ToString()} OrgTrait: {OriginalTraitDefName}:{OriginalDegree} AddTrait: {AddedTraitDefName}:{AddedDegree}]";
    }

    public string ToPrettyString()
    {
        switch (AlterType)
        {
            case TraitAlterType.Added:
                return "PS_HediffReconditionedAddedLab".Translate() + " " + AddedTraitLabel;

            case TraitAlterType.Removed:
                return "PS_HediffReconditionedRemovedLab".Translate() + " " + OriginalTraitLabel;

            case TraitAlterType.Altered:
                return "PS_HediffReconditionedAlteredLab".Translate() + " " + OriginalTraitLabel;

            case TraitAlterType.UNSET:
                Log.Error("PS_Hediff_Reconditioned: Tried to get label of hediff with ChangeType = UNSET.");
                return "ERROR";

            default:
                Log.Error("PS_Hediff_Reconditioned: Tried to get label of hediff with unknown ChangeType.");
                return "ERROR";
        }
    }

    public string ToShortPrettyString()
    {
        switch (AlterType)
        {
            case TraitAlterType.Added:
                return "PS_Added".Translate() + " " + AddedTraitLabel;

            case TraitAlterType.Removed:
                return "PS_Removed".Translate() + " " + OriginalTraitLabel;

            case TraitAlterType.Altered:
                return "PS_Changed".Translate() + " " + OriginalTraitLabel;

            case TraitAlterType.UNSET:
                Log.Error("PS_Hediff_Reconditioned: Tried to get label of hediff with ChangeType = UNSET.");
                return "ERROR";

            default:
                Log.Error("PS_Hediff_Reconditioned: Tried to get label of hediff with unknown ChangeType.");
                return "ERROR";
        }
    }

    public bool IsValid()
    {
        switch (AlterType)
        {
            case TraitAlterType.UNSET:
                return false;
            case TraitAlterType.Added:
                return !string.IsNullOrEmpty(AddedTraitDefName);
            case TraitAlterType.Removed:
                return !string.IsNullOrEmpty(OriginalTraitDefName);
            case TraitAlterType.Altered:
                return !string.IsNullOrEmpty(AddedTraitDefName) && !string.IsNullOrEmpty(OriginalTraitDefName);
            default:
                return false;
        }
        //return (this.AlterType != TraitAlterType.UNSET) && (!string.IsNullOrEmpty(this.OrigonalTraitDefName) || !string.IsNullOrEmpty(this.AddedTraitDefName));
    }

    public bool IsSame(PS_Conditioning_Data conData)
    {
        return AlterType == conData.AlterType && OriginalTraitDefName == conData.OriginalTraitDefName &&
               OriginalDegree == conData.OriginalDegree && AddedTraitDefName == conData.AddedTraitDefName &&
               AddedDegree == conData.AddedDegree;
    }
}