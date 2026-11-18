using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace PS_ReconPod;

public class PS_Buildings_ReconPod : Building_CryptosleepCasket
{
    private readonly float TicksPerDay = 60000;
    private readonly float TotalTicksPerAction = 2500;
    private readonly CompProperties_Power powerComp;

    public bool CheatMod;
    private bool CheckedHediffPart;
    private PS_Conditioning_Data ConditioningData;
    private float CurrentMaxTicks;

    private float CurrentTicksLeft;

    private PS_Conditioning_JobState JobState;

    public Pawn PodOwner;
    private Effecter ProgressBarEffector;

    private List<NeedValuePair> StartingNeedLevels;

    public bool HasOwner => PodOwner != null;

    public bool IsUseable(Pawn pawn)
    {
        return this.TryGetComp<CompPowerTrader>().PowerOn && !this.IsForbidden(pawn);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Deep.Look(ref ConditioningData, "ConditionData");
        Scribe_References.Look(ref PodOwner, "PodOwner");
        Scribe_Values.Look(ref JobState, "JobState");
        Scribe_Values.Look(ref CurrentTicksLeft, "CurrentTicksLeft");
        Scribe_Values.Look(ref CurrentMaxTicks, "CurrentMaxTicks");
        Scribe_Collections.Look(ref StartingNeedLevels, "StartingNeedLevels");
    }

    public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
    {
        if (!base.TryAcceptThing(thing, allowSpecialEffects))
        {
            return false;
        }

        if (allowSpecialEffects)
        {
            SoundDefOf.CryptosleepCasket_Accept.PlayOneShot(new TargetInfo(Position, Map));
        }

        return true;
    }

    public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn myPawn)
    {
        foreach (var o in base.GetFloatMenuOptions(myPawn))
        {
            if (o.Label != "EnterCryptosleepCasket".Translate())
            {
                yield return o;
            }
        }

        if (CheatMod)
        {
            var jobDef = PS_ReconPodDefsOf.PS_DoConditioning;
            string jobStr = "PS_PodOption_CheatMode".Translate();

            yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction), myPawn,
                this);
            yield break;

            void jobAction()
            {
                var job = new Job(jobDef, this);
                myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
            }
        }

        if (!this.TryGetComp<CompPowerTrader>().PowerOn)
        {
            yield return new FloatMenuOption("CannotUseNoPower".Translate(), null);
        }
        else if (PS_ConditioningHelper.IsCemented(myPawn))
        {
            yield return new FloatMenuOption(string.Format("PS_CementCantUsePod".Translate(), myPawn.LabelShort),
                null);
        }
        else if (PodOwner != null && myPawn != PodOwner)
        {
            yield return new FloatMenuOption("PS_OwnedBy".Translate() + PodOwner.LabelShort, null);
        }
        else if (innerContainer.Count == 0)
        {
            if (PS_ConditioningHelper.IsReconditioned(myPawn) && PS_PodFinder.FindMyPod(myPawn) != this &&
                PS_PodFinder.FindMyPod(myPawn) != null)
            {
                yield return new FloatMenuOption(string.Format("PS_NotOwnedBy".Translate(), myPawn.LabelShort), null);
            }
            else if (PS_ConditioningHelper.IsReconditioned(myPawn) && PS_PodFinder.FindMyPod(myPawn) == null &&
                     PodOwner == null)
            {
                _ = PS_ReconPodDefsOf.PS_RefreshConditioning;
                var jobStr = string.Format("PS_PodOption_ClaimPod".Translate(), myPawn.LabelShort);

                void jobAction()
                {
                    TryAssignPawn(myPawn);
                }

                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction), myPawn,
                    this);
            }
            else if (myPawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly))
            {
                if (PS_ConditioningHelper.IsReconditioned(myPawn))
                {
                    var jobDef = PS_ReconPodDefsOf.PS_RefreshConditioning;
                    string jobStr = "PS_PodOption_RefreshConditioning".Translate();

                    void jobAction()
                    {
                        var job = new Job(jobDef, this);
                        myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }

                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction),
                        myPawn, this);

                    var jobDef2 = PS_ReconPodDefsOf.PS_ManageConditioning;
                    string jobStr2 = "PS_PodOption_Decondition".Translate();

                    void jobAction2()
                    {
                        var job2 = new Job(jobDef2, this);
                        myPawn.jobs.TryTakeOrderedJob(job2, JobTag.Misc);
                    }

                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr2, jobAction2),
                        myPawn, this);
                }
                else
                {
                    var jobDef = PS_ReconPodDefsOf.PS_DoConditioning;
                    string jobStr = "PS_PodOption_Condition".Translate();

                    void jobAction()
                    {
                        var job = new Job(jobDef, this);
                        myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    }

                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction),
                        myPawn, this);
                }
            }
        }
    }

    private void TryAssignPawn(Pawn pawn)
    {
        PodOwner ??= pawn;
    }

    public override string GetInspectString()
    {
        string result;
        if (ParentHolder != null && ParentHolder is not Verse.Map)
        {
            result = base.GetInspectString();
        }
        else
        {
            var stringBuilder = new StringBuilder(base.GetInspectString());
            stringBuilder.AppendLine();
            if (PodOwner == null)
            {
                stringBuilder.AppendLine("Owner".Translate() + ": " + "Nobody".Translate());
            }
            else
            {
                stringBuilder.AppendLine("Owner".Translate() + ": " + PodOwner.Label);
            }

            if (JobState == PS_Conditioning_JobState.Waiting)
            {
                stringBuilder.AppendLine("PS_Status".Translate() + ": " + GetStatusString());
            }
            else
            {
                var completion = (1f - (CurrentTicksLeft / CurrentMaxTicks)) * 100f;
                stringBuilder.AppendLine("PS_Status".Translate() + ": " + GetStatusString() + " (" +
                                         completion.ToString("0.00") + "%)");
            }

            if (Prefs.DevMode && PodOwner != null)
            {
                stringBuilder.AppendLine($"(Dev) Pawn need: {PS_ConditioningHelper.GetCurrentNeedLevel(PodOwner)}");
            }

            result = stringBuilder.ToString().TrimEndNewlines();
        }

        return result;
    }

    private string GetStatusString()
    {
        switch (JobState)
        {
            case PS_Conditioning_JobState.Waiting:
                return "PS_PodState_StandBy".Translate();
            case PS_Conditioning_JobState.Reconditioning:
                return "PS_PodState_Reconditioning".Translate();
            case PS_Conditioning_JobState.Refreshing:
                return "PS_PodState_ReinforceingCondtioning".Translate();
            case PS_Conditioning_JobState.Deconditioning:
                return "PS_PodState_Deconditioning".Translate();
            default:
                return "ERROR";
        }
    }

    public void TryUnassignPawn(Pawn pawn)
    {
        if (PodOwner == pawn)
        {
            PodOwner = null;
        }
    }

    public void ForceUnassignPawn(Pawn pawn)
    {
        if (PodOwner != pawn)
        {
            return;
        }

        PodOwner = null;
        if (JobState != PS_Conditioning_JobState.Waiting)
        {
            ForceEndJobState();
        }

        EjectContents();
    }

    public bool AssignedAnything(Pawn pawn)
    {
        return pawn.ownership.OwnedBed != null;
    }

    public override void Open()
    {
        ForceEndJobState();
        base.Open();
    }

    public override void EjectContents()
    {
        ForceEndJobState();
        foreach (var thing in innerContainer)
        {
            if (thing is Pawn pawn)
            {
                PawnComponentsUtility.AddComponentsForSpawn(pawn);
            }
        }

        if (!Destroyed && innerContainer.Any())
        {
            SoundDefOf.CryptosleepCasket_Eject.PlayOneShot(SoundInfo.InMap(new TargetInfo(Position, Map)));
        }

        innerContainer.TryDropAll(InteractionCell, Map, ThingPlaceMode.Near);
        contentsKnown = true;
    }

    private void ForceEndJobState()
    {
        JobState = PS_Conditioning_JobState.Waiting;
        CurrentTicksLeft = 0f;
        if (ProgressBarEffector == null)
        {
            return;
        }

        ProgressBarEffector.Cleanup();
        ProgressBarEffector = null;
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos())
        {
            yield return g;
        }

        if (!Prefs.DevMode)
        {
            yield break;
        }

        var toggleCheat = new Command_Toggle
        {
            defaultLabel = "Cheat Mode",
            defaultDesc = "PS_CheatModeString".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/CheatMode"),
            isActive = () => CheatMod,
            toggleAction = delegate { CheatMod = !CheatMod; }
        };
        yield return toggleCheat;

        if (JobState != PS_Conditioning_JobState.Waiting)
        {
            yield return new Command_Action
            {
                defaultLabel = "Dev: Insta Complete",
                action = delegate
                {
                    if (JobState != PS_Conditioning_JobState.Waiting)
                    {
                        CurrentTicksLeft = 0;
                    }
                },
                icon = ContentFinder<Texture2D>.Get("UI/DevInstComplete")
            };
        }

        if (PodOwner == null)
        {
            yield break;
        }

        yield return new Command_Action
        {
            defaultLabel = "Dev: Down Pawn Need",
            action = delegate
            {
                PS_ConditioningHelper.SetCurrentNeedLevel(PodOwner,
                    PodOwner.ConditioningLevel() - 0.1f);
            },
            icon = ContentFinder<Texture2D>.Get("UI/DevDown")
        };
        yield return new Command_Action
        {
            defaultLabel = "Dev: Fill Pawn Need",
            action = delegate { PS_ConditioningHelper.SetCurrentNeedLevel(PodOwner, 1f); },
            icon = ContentFinder<Texture2D>.Get("UI/DevFull")
        };
        yield return new Command_Action
        {
            defaultLabel = "Dev: Half Pawn Need",
            action = delegate { PS_ConditioningHelper.SetCurrentNeedLevel(PodOwner, 0.5f); },
            icon = ContentFinder<Texture2D>.Get("UI/DevHalf")
        };
    }

    public override void DrawGUIOverlay()
    {
        if (CheatMod)
        {
            GenMapUI.DrawThingLabel(this, "Cheat Mode", Color.red);
        }
        else if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
        {
            var defaultThingLabelColor = GenMapUI.DefaultThingLabelColor;
            if (PodOwner == null)
            {
                GenMapUI.DrawThingLabel(this, "Unowned".Translate(), defaultThingLabelColor);
            }
            else if (JobState != PS_Conditioning_JobState.Waiting)
            {
                // Draw nothing because the progress bar will be there
            }
            else
            {
                GenMapUI.DrawThingLabel(this, PodOwner.LabelShort, defaultThingLabelColor);
            }
        }
    }

    public void StartReconditioning(Pawn pawn, PS_Conditioning_Data conData)
    {
        if (PodOwner != null &&
            PodOwner != pawn)
        {
            Log.Error("PS_Buildings_ReconPod: Tried to start conditioning on a pawn that was not the owner");
            return;
        }

        SaveNeeds(pawn.needs.AllNeeds);
        pawn.DeSpawn();
        if (!TryAcceptThing(pawn))
        {
            return;
        }

        ConditioningData = conData;
        CurrentMaxTicks = PS_ConditioningHelper.DaysToCondition(pawn) * TicksPerDay;
        CurrentTicksLeft = CurrentMaxTicks;
        JobState = PS_Conditioning_JobState.Reconditioning;
        TryAssignPawn(pawn);

        var progressBar = EffecterDefOf.ProgressBar;
        ProgressBarEffector = progressBar.Spawn();
        var target = new TargetInfo(this);
        ProgressBarEffector.EffectTick(target, TargetInfo.Invalid);
    }

    public void StartRefreshing(Pawn pawn, LocalTargetInfo targetInfo)
    {
        if (PodOwner != pawn)
        {
            return;
        }

        SaveNeeds(pawn.needs.AllNeeds);
        pawn.DeSpawn();
        if (!TryAcceptThing(pawn))
        {
            return;
        }

        CurrentTicksLeft = CurrentMaxTicks = TotalTicksPerAction;
        JobState = PS_Conditioning_JobState.Refreshing;


        var progressBar = EffecterDefOf.ProgressBar;
        ProgressBarEffector = progressBar.Spawn();
        ProgressBarEffector.EffectTick(targetInfo.ToTargetInfo(Map), TargetInfo.Invalid);
    }

    public void StartFixingBotched(Pawn pawn)
    {
        if (PodOwner != null && PodOwner != pawn)
        {
            return;
        }

        SaveNeeds(pawn.needs.AllNeeds);
        pawn.DeSpawn();
        if (!TryAcceptThing(pawn))
        {
            return;
        }

        CurrentTicksLeft = CurrentMaxTicks = TicksPerDay;
        JobState = PS_Conditioning_JobState.FixingBotch;


        var progressBar = EffecterDefOf.ProgressBar;
        ProgressBarEffector = progressBar.Spawn();
        var target = new TargetInfo(this);
        ProgressBarEffector.EffectTick(target, TargetInfo.Invalid);
    }

    public void StartDeconditioning(Pawn pawn, PS_Conditioning_Data conData)
    {
        if (PodOwner != pawn)
        {
            return;
        }

        SaveNeeds(pawn.needs.AllNeeds);
        pawn.DeSpawn();
        if (!TryAcceptThing(pawn))
        {
            return;
        }

        CurrentMaxTicks = TicksPerDay;
        CurrentTicksLeft = CurrentMaxTicks;
        JobState = PS_Conditioning_JobState.Deconditioning;
        ConditioningData = conData;

        var progressBar = EffecterDefOf.ProgressBar;
        ProgressBarEffector = progressBar.Spawn();
        var target = new TargetInfo(this);
        ProgressBarEffector.EffectTick(target, TargetInfo.Invalid);
    }

    protected override void Tick()
    {
        base.Tick();

        if (PodOwner is { Dead: true })
        {
            ForceEndJobState();
            PodOwner = null;
        }

        if (PodOwner != null && JobState == PS_Conditioning_JobState.Waiting &&
            !PS_ConditioningHelper.IsReconditioned(PodOwner))
        {
            ForceEndJobState();
            PodOwner = null;
        }

        if (JobState == PS_Conditioning_JobState.Waiting) // Do nothing if waiting
        {
            return;
        }

        if (!this.TryGetComp<CompPowerTrader>().PowerOn && HasAnyContents) // Boot pawn if lose power
        {
            ForceEndJobState();
            EjectContents();
            return;
        }

        var pawn = (Pawn)innerContainer.FirstOrDefault();
        if (pawn != null)
        {
            ResetNeeds(pawn);
        }


        var mote = ((SubEffecter_ProgressBar)ProgressBarEffector?.children[0])?.mote;
        if (mote != null)
        {
            mote.progress = Mathf.Clamp01(1f - (CurrentTicksLeft / CurrentMaxTicks));
            mote.offsetZ = -0.5f;
        }

        if (JobState == PS_Conditioning_JobState.Waiting)
        {
            return;
        }

        CurrentTicksLeft--;
        if (CurrentTicksLeft <= 0)
        {
            FinishConditioning();
        }
    }

    private void FinishConditioning()
    {
        var pawn = (Pawn)innerContainer.FirstOrDefault();
        if (pawn == null)
        {
            Log.Error("PS_Bulding_ReconPod: Finsihed conditioning put held pawn is null");
            EjectContents();
            ForceEndJobState();
            return;
        }

        switch (JobState)
        {
            case PS_Conditioning_JobState.Waiting:
                return;

            case PS_Conditioning_JobState.Reconditioning:
                if (RoleForSucessOrHandleFail(pawn))
                {
                    PS_ConditioningHelper.DoConditioning(pawn, this, ConditioningData);
                    TryAssignPawn(pawn);
                    pawn.needs.mood.thoughts.memories.TryGainMemory(
                        DefDatabase<ThoughtDef>.GetNamed("PS_Thoughts_Memory_Reconditioned"));
                    PS_ConditioningHelper.DirtyNeedFall(pawn);
                    PS_ConditioningHelper.SetCurrentNeedLevel(pawn,
                        PS_ConditioningHelper.GetCurrentNeedLevel(pawn) + 1f);
                    Messages.Message(
                        string.Format("PS_Messages_CompletedReconditioning".Translate(), pawn.LabelShort),
                        new LookTargets(pawn), MessageTypeDefOf.TaskCompletion);
                }

                Open();
                break;

            case PS_Conditioning_JobState.Refreshing:
                PS_ConditioningHelper.SetCurrentNeedLevel(pawn,
                    PS_ConditioningHelper.GetCurrentNeedLevel(pawn) + 0.6f);
                PS_ConditioningHelper.DirtyNeedFall(pawn);
                Open();
                break;

            case PS_Conditioning_JobState.Deconditioning:
                var lastConditioning =
                    (PS_ConditioningHelper.GetConditioningDataFromHediff(pawn)?.Count ?? 0) <= 1;
                PS_ConditioningHelper.UndoConditioning(pawn, this, ConditioningData);
                if (lastConditioning)
                {
                    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(
                        DefDatabase<ThoughtDef>.GetNamed("PS_Thoughts_Memory_Reconditioned"));
                    pawn.needs.mood.thoughts.memories.TryGainMemory(
                        DefDatabase<ThoughtDef>.GetNamed("PS_Thoughts_Memory_Deconditioned"));

                    TryUnassignPawn(pawn);
                }
                else
                {
                    PS_ConditioningHelper.DirtyNeedFall(pawn);
                    PS_ConditioningHelper.SetCurrentNeedLevel(pawn,
                        PS_ConditioningHelper.GetCurrentNeedLevel(pawn) + 1f);
                }

                Open();
                Messages.Message(string.Format("PS_Messages_CompletedDeconditioning".Translate(), pawn.LabelShort),
                    new LookTargets(pawn), MessageTypeDefOf.TaskCompletion);
                break;

            case PS_Conditioning_JobState.FixingBotch:
                PS_ConditioningHelper.RemoveBotch(pawn);
                pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(
                    DefDatabase<ThoughtDef>.GetNamed("PS_Thoughts_Memory_BotchedConditioned"));
                if (PS_ConditioningHelper.IsReconditioned(pawn))
                {
                    PS_ConditioningHelper.DirtyNeedFall(pawn);
                    PS_ConditioningHelper.SetCurrentNeedLevel(pawn,
                        PS_ConditioningHelper.GetCurrentNeedLevel(pawn) + 1f);
                }

                Open();
                Messages.Message(string.Format("PS_Messages_CompletedFixingBotched".Translate(), pawn.LabelShort),
                    new LookTargets(pawn), MessageTypeDefOf.TaskCompletion);
                break;
        }

        if (ProgressBarEffector != null)
        {
            ProgressBarEffector.Cleanup();
            ProgressBarEffector = null;
        }

        JobState = PS_Conditioning_JobState.Waiting;

        // Move hediff to brain if it's not, fix for old version
        if (CheckedHediffPart)
        {
            return;
        }

        var hediff = pawn.health.hediffSet.GetFirstHediff<PS_Hediff_Reconditioned>();
        if (hediff != null)
        {
            var currentPart = hediff.Part;
            var brain = pawn.RaceProps.body.AllParts.FirstOrDefault(x => x.def.defName == "Brain");
            if (brain != null && currentPart != brain)
            {
                hediff.Part = brain;
            }
        }

        CheckedHediffPart = true;
    }

    private void SaveNeeds(List<Need> needs)
    {
        StartingNeedLevels ??= [];

        StartingNeedLevels.Clear();

        foreach (var need in needs)
        {
            StartingNeedLevels.Add(new NeedValuePair { DefName = need.def.defName, Value = need.CurLevel });
        }
    }

    private void ResetNeeds(Pawn pawn)
    {
        if (StartingNeedLevels == null)
        {
            Log.Message(
                $"PS_Buildings_ReconPod: Tried to reset needs for {pawn.LabelShort}, but starting needs list is null.");
        }

        if (StartingNeedLevels == null)
        {
            return;
        }

        foreach (var need in StartingNeedLevels)
        {
            var pawnNeed = pawn.needs.TryGetNeed(DefDatabase<NeedDef>.GetNamed(need.DefName));
            pawnNeed?.CurLevel = need.Value;
        }
    }

    private static bool RoleForSucessOrHandleFail(Pawn pawn)
    {
        var conCount = PS_ConditioningHelper.GetConditioningDataFromHediff(pawn, false)?.Count ?? 0;
        var successChance = PS_ConditioningHelper.GetSucessChance(conCount);
        var roll = Rand.Range(0f, 1f);

        // Sucess
        if (roll < successChance)
        {
            return true;
        }

        roll = Rand.Range(0f, 1f);
        switch (roll)
        {
            // Fail with no coniquence
            case > 0.3f:
                pawn.needs.mood.thoughts.memories.TryGainMemory(
                    DefDatabase<ThoughtDef>.GetNamed("PS_Thoughts_Memory_FailedConditioned"));
                Messages.Message(string.Format("PS_Messages_FailConditioning".Translate(), pawn.LabelShort),
                    new LookTargets(pawn), MessageTypeDefOf.NegativeEvent);
                break;
            // Botched
            case > 0.01f:
            {
                var bothedcondata = new PS_Conditioning_Data
                {
                    AlterType = TraitAlterType.Added,
                    AddedTraitDefName = "PS_Trait_BotchedConditioning",
                    AddedDegree = -1
                };
                pawn.needs.mood.thoughts.memories.TryGainMemory(
                    DefDatabase<ThoughtDef>.GetNamed("PS_Thoughts_Memory_BotchedConditioned"));
                Messages.Message(string.Format("PS_Messages_BotchedConditioning".Translate(), pawn.LabelShort),
                    new LookTargets(pawn), MessageTypeDefOf.ThreatBig);
                PS_ConditioningHelper.DoTraitChange(pawn, bothedcondata);
                break;
            }
            // Lucky Botched
            default:
            {
                var condata = new PS_Conditioning_Data
                {
                    AlterType = TraitAlterType.Added,
                    AddedTraitDefName = "PS_Trait_BotchedConditioning",
                    AddedDegree = 1
                };
                pawn.needs.mood.thoughts.memories.TryGainMemory(
                    DefDatabase<ThoughtDef>.GetNamed("PS_Thoughts_Memory_LuckyConditioned"));
                Messages.Message(string.Format("PS_Messages_LuckyConditioning".Translate(), pawn.LabelShort),
                    new LookTargets(pawn), MessageTypeDefOf.ThreatBig);
                PS_ConditioningHelper.DoTraitChange(pawn, condata);
                break;
            }
        }

        return false;
    }

    private sealed class NeedValuePair : IExposable
    {
        public string DefName;
        public float Value;

        public void ExposeData()
        {
            Scribe_Values.Look(ref Value, "Value");
            Scribe_Values.Look(ref DefName, "DefName");
        }
    }
}