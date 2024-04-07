using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace PS_ReconPod;

public class PS_TraitHelper
{
    private static List<TraitDef> _AllTraitDefs;

    private static List<Trait> _AllTraits;

    private static List<string> BlackList =>
    [
        "Beauty", "Immunity", "AnnoyingVoice", "PS_Trait_BotchedConditioning", "CreepyBreathing"
    ];

    public static List<TraitDef> AllTraitDefs
    {
        get
        {
            if (_AllTraitDefs == null)
            {
                _AllTraitDefs = DefDatabase<TraitDef>.AllDefs.ToList();
            }

            return _AllTraitDefs.ToList();
        }
    }

    public static List<Trait> AllTraits
    {
        get
        {
            if (_AllTraits == null)
            {
                LoadTraits();
            }

            return _AllTraits;
        }
    }

    public static bool IsBlacklisted(Trait t)
    {
        return BlackList?.Contains(t.def.defName) ?? false;
    }

    private static void LoadTraits()
    {
        _AllTraits = [];
        foreach (var traitDef in AllTraitDefs)
        {
            if (traitDef.degreeDatas.Count > 0)
            {
                foreach (var degree in traitDef.degreeDatas)
                {
                    _AllTraits.Add(new Trait(traitDef, degree.degree));
                }
            }
            else
            {
                _AllTraits.Add(new Trait(traitDef));
            }
        }

        _AllTraits = _AllTraits.OrderBy(trait => trait.LabelCap).ToList();
    }

    public static List<Trait> AllTraitsCompadable(List<Trait> CurrentTraits, bool IncludeBlack = false)
    {
        var conflicts = CurrentTraits.SelectMany(trait => trait.def.conflictingTraits).Select(def => def.defName)
            .ToList();
        var defNames = CurrentTraits.Select(trait => trait.def.defName);
        return AllTraits.Where(trait =>
            !defNames.Contains(trait.def.defName) && !conflicts.Contains(trait.def.defName) &&
            (IncludeBlack || !IsBlacklisted(trait))).OrderBy(trait => trait.LabelCap).ToList();
    }

    public static TraitDegreeData GetDegreeDate(Trait Trait)
    {
        var degreedata = Trait.def.degreeDatas?.Where(x => x.degree == Trait.Degree).SingleOrDefault();
        return degreedata;
    }
}