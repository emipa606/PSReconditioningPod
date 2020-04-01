using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace PS_ReconPod
{
    // Token: 0x020006C8 RID: 1736
    public class PS_Buildings_ReconPod : Building_CryptosleepCasket
    {

        private class NeedValuePair : IExposable
        {
            public string DefName;
            public float Value;

            public void ExposeData()
            {
                Scribe_Values.Look<float>(ref this.Value, "Value");
                Scribe_Values.Look<string>(ref this.DefName, "DefName");
            }
        }

        private CompProperties_Power powerComp;
        //private Pawn _Owner;
        //private string PodOwnerId;
        private PS_Conditioning_JobState JobState;
        private float CurrentTicksLeft;
        private float CurrentMaxTicks;
        private float TotalTicksPerAction = 2500;
        private float TicksPerDay = 60000;
        private PS_Conditioning_Data ConditioningData;
        private Effecter ProgressBarEffector;
        private bool CheckedHediffPart;

        private List<NeedValuePair> StartingNeedLevels;
        
        public bool HasOwner { get { return this.PodOwner != null; } }
        public bool IsUseable(Pawn pawn)
        {
            return this.TryGetComp<CompPowerTrader>().PowerOn && !this.IsForbidden(pawn);
        }

        public bool CheatMod;

        public Pawn PodOwner;
        //{
        //    get
        //    {
        //        if (this._Owner == null)
        //        {
        //            if (!string.IsNullOrEmpty(this.PodOwnerId))
        //            {
        //                //var pawn = Find.World.worldPawns.AllPawnsAlive.Where(x => x.ThingID == this.PodOwnerId).FirstOrDefault();
        //                var pawn = PawnsFinder.All_AliveOrDead.Where(x => x.ThingID == this.PodOwnerId).FirstOrDefault();
        //                if (pawn == null)
        //                    Log.Error("PS_ReconPod: Failed to find pod owner withing ThingId " + this.PodOwnerId);
        //                else
        //                    this._Owner = pawn;
        //            }
        //        }
        //        return this._Owner;
        //    }
        //}

        public override void ExposeData()
        {
            base.ExposeData();
            //Scribe_Values.Look<string>(ref this.PodOwnerId, "PodOwnerId");
            Scribe_Deep.Look<PS_Conditioning_Data>(ref this.ConditioningData, "ConditionData");
            Scribe_References.Look<Pawn>(ref this.PodOwner, "PodOwner");
            Scribe_Values.Look<PS_Conditioning_JobState>(ref this.JobState, "JobState");
            Scribe_Values.Look<float>(ref this.CurrentTicksLeft, "CurrentTicksLeft");
            Scribe_Values.Look<float>(ref this.CurrentMaxTicks, "CurrentMaxTicks");
            Scribe_Collections.Look<NeedValuePair>(ref this.StartingNeedLevels, "StartingNeedLevels");
        }

        // Token: 0x06002513 RID: 9491 RVA: 0x00116F67 File Offset: 0x00115367
        public override bool TryAcceptThing(Thing thing, bool allowSpecialEffects = true)
        {
            if (base.TryAcceptThing(thing, allowSpecialEffects))
            {
                if (allowSpecialEffects)
                {
                    SoundDefOf.CryptosleepCasket_Accept.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
                }
                return true;
            }
            return false;
        }

        // Token: 0x06002514 RID: 9492 RVA: 0x00116FA0 File Offset: 0x001153A0
        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn pawn)
        {
            foreach (FloatMenuOption o in base.GetFloatMenuOptions(pawn))
            {
                if(o.Label != "EnterCryptosleepCasket".Translate())
                yield return o;
            }
            if(this.CheatMod)
            {
                JobDef jobDef = PS_ReconPodDefsOf.PS_DoConditioning;
                string jobStr = "PS_PodOption_CheatMode".Translate();
                Action jobAction = delegate ()
                {
                    Job job = new Job(jobDef, this);
                    pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                };
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction, MenuOptionPriority.Default, null, null, 0f, null, null), pawn, this, "ReservedBy");
                yield break;
            }

            if (!this.TryGetComp<CompPowerTrader>().PowerOn)
            {
                yield return new FloatMenuOption("CannotUseNoPower".Translate(), null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else if (PS_ConditioningHelper.IsCemented(pawn))
            {
                yield return new FloatMenuOption(string.Format("PS_CementCantUsePod".Translate(), pawn.LabelShort), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                yield break;
            }
            else if (this.PodOwner != null && pawn != this.PodOwner)
            {
                yield return new FloatMenuOption("PS_OwnedBy".Translate() + this.PodOwner.LabelShort, null, MenuOptionPriority.Default, null, null, 0f, null, null);
            }
            else if (this.innerContainer.Count == 0)
            {
                if(PS_ConditioningHelper.IsReconditioned(pawn) && PS_PodFinder.FindMyPod(pawn) != this && PS_PodFinder.FindMyPod(pawn) != null)
                    yield return new FloatMenuOption(string.Format("PS_NotOwnedBy".Translate(), pawn.LabelShort), null, MenuOptionPriority.Default, null, null, 0f, null, null);
                else if (PS_ConditioningHelper.IsReconditioned(pawn) && PS_PodFinder.FindMyPod(pawn) == null && this.PodOwner == null)
                {
                    JobDef jobDef = PS_ReconPodDefsOf.PS_RefreshConditioning;
                    string jobStr = string.Format("PS_PodOption_ClaimPod".Translate(), pawn.LabelShort);
                    Action jobAction = delegate ()
                    {
                        this.TryAssignPawn(pawn);
                    };
                    yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction, MenuOptionPriority.Default, null, null, 0f, null, null), pawn, this, "ReservedBy");
                }
                else if (pawn.CanReach(this, PathEndMode.InteractionCell, Danger.Deadly, false, TraverseMode.ByPawn))
                {
                    if (PS_ConditioningHelper.IsReconditioned(pawn))
                    {
                        JobDef jobDef = PS_ReconPodDefsOf.PS_RefreshConditioning;
                        string jobStr = "PS_PodOption_RefreshConditioning".Translate();
                        Action jobAction = delegate ()
                        {
                            Job job = new Job(jobDef, this);
                            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        };
                        yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction, MenuOptionPriority.Default, null, null, 0f, null, null), pawn, this, "ReservedBy");

                        JobDef jobDef2 = PS_ReconPodDefsOf.PS_ManageConditioning;
                        string jobStr2 = "PS_PodOption_Decondition".Translate();
                        Action jobAction2 = delegate ()
                        {
                            Job job2 = new Job(jobDef2, this);
                            pawn.jobs.TryTakeOrderedJob(job2, JobTag.Misc);
                        };
                        yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr2, jobAction2, MenuOptionPriority.Default, null, null, 0f, null, null), pawn, this, "ReservedBy");
                    }
                    else
                    {
                        JobDef jobDef = PS_ReconPodDefsOf.PS_DoConditioning;
                        string jobStr = "PS_PodOption_Condition".Translate();
                        Action jobAction = delegate ()
                        {
                            Job job = new Job(jobDef, this);
                            pawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                        };
                        yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(jobStr, jobAction, MenuOptionPriority.Default, null, null, 0f, null, null), pawn, this, "ReservedBy");
                    }
                }
            }
            yield break;
        }

        public void TryAssignPawn(Pawn pawn)
        {
            //if (string.IsNullOrEmpty(this.PodOwnerId))
            //{
            //    this._Owner = null;
            //    this.PodOwnerId = pawn.ThingID;
            //}
            if (this.PodOwner == null)
                this.PodOwner = pawn;
        }

        //public bool TryStartConditioning(Pawn pawn, PS_Conditioning_JobState ConditionType)
        //{
        //    if(this.JobState != PS_Conditioning_JobState.Waiting || !this.IsUseable(pawn))
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        if(this.TryAcceptThing(pawn))
        //        {
        //            this.JobState = ConditionType;
        //            this.CurrentTicksLeft = this.TotalTicksPerAction;
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        // Token: 0x060024DC RID: 9436 RVA: 0x00118724 File Offset: 0x00116B24
        public override string GetInspectString()
        {
            bool flag = base.ParentHolder != null && !(base.ParentHolder is Map);
            string result;
            if (flag)
            {
                result = base.GetInspectString();
            }
            else
            {
                StringBuilder stringBuilder = new StringBuilder(base.GetInspectString());
                stringBuilder.AppendLine();
                if (this.PodOwner == null) //string.IsNullOrEmpty(this.PodOwnerId))
                {
                    stringBuilder.AppendLine("Owner".Translate() + ": " + "Nobody".Translate());
                }
                else
                {
                    stringBuilder.AppendLine("Owner".Translate() + ": " + this.PodOwner.Label);
                }

                if(this.JobState == PS_Conditioning_JobState.Waiting)
                    stringBuilder.AppendLine("PS_Status".Translate() + ": " + GetStatusString());
                else
                {
                    var completion = (1f - this.CurrentTicksLeft / this.CurrentMaxTicks) * 100f;
                    stringBuilder.AppendLine("PS_Status".Translate() + ": " + GetStatusString() + " (" + completion.ToString("0.00") + "%)");
                }

                if (Prefs.DevMode && this.PodOwner != null)
                    stringBuilder.AppendLine("(Dev) Pawn need: " + PS_ConditioningHelper.GetCurrentNeedLevel(this.PodOwner));
                result = stringBuilder.ToString().TrimEndNewlines();
            }
            return result;
        }

        private string GetStatusString()
        {
            switch(this.JobState)
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
            if(this.PodOwner == pawn)
            {
                this.PodOwner = null;
            }
            //if (pawn.ThingID == this.PodOwnerId)
            //{
            //    this._Owner = null;
            //    this.PodOwnerId = null;
            //}
        }

        public void ForceUnassignPawn(Pawn pawn)
        {
            if (this.PodOwner == pawn)
            {
                this.PodOwner = null;
                if (this.JobState != PS_Conditioning_JobState.Waiting)
                    this.ForceEndJobState();
                this.EjectContents();
            }
            //if (pawn.ThingID == this.PodOwnerId)
            //{
            //    this._Owner = null;
            //    this.PodOwnerId = null;
            //}
        }

        public bool AssignedAnything(Pawn pawn)
        {
            return pawn.ownership.OwnedBed != null;
        }

        public override void Open()
        {
            this.ForceEndJobState();
            base.Open();
        }

        public override void EjectContents()
        {
            this.ForceEndJobState();
            foreach (Thing thing in ((IEnumerable<Thing>)this.innerContainer))
            {
                Pawn pawn = thing as Pawn;
                if (pawn != null)
                {
                    PawnComponentsUtility.AddComponentsForSpawn(pawn);
                }
            }
            if (!base.Destroyed && this.innerContainer.Any())
            {
                SoundDefOf.CryptosleepCasket_Eject.PlayOneShot(SoundInfo.InMap(new TargetInfo(base.Position, base.Map, false), MaintenanceType.None));
            }

            this.innerContainer.TryDropAll(this.InteractionCell, base.Map, ThingPlaceMode.Near, null, null);
            this.contentsKnown = true;
        }

        private void ForceEndJobState()
        {
            this.JobState = PS_Conditioning_JobState.Waiting;
            this.CurrentTicksLeft = 0f;
            if (ProgressBarEffector != null)
            {
                ProgressBarEffector.Cleanup();
                ProgressBarEffector = null;
            }
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            //if (this.def.building.bed_humanlike && base.Faction == Faction.OfPlayer)
            //{
            //    Command_Toggle pris = new Command_Toggle();

            //    if (this.innerContainer != null)
            //    {
            //        Command_Action eject = new Command_Action();
            //        eject.action = new Action(this.EjectContents);
            //        eject.defaultLabel = "CommandPodEject".Translate();
            //        eject.defaultDesc = "CommandPodEjectDesc".Translate();
            //        if (this.innerContainer.Count == 0)
            //        {
            //            eject.Disable("CommandPodEjectFailEmpty".Translate());
            //        }
            //        eject.hotKey = KeyBindingDefOf.Misc1;
            //        eject.icon = ContentFinder<Texture2D>.Get("UI/Commands/PodEject", true);
            //        yield return eject;
            //    }
            //}
            if (Prefs.DevMode)
            {
                var toggleCheat = new Command_Toggle();
                toggleCheat.defaultLabel = "Cheat Mode";
                toggleCheat.defaultDesc = "PS_CheatModeString".Translate();
                toggleCheat.icon = ContentFinder<Texture2D>.Get("UI/CheatMode", true);
                toggleCheat.isActive = (() => this.CheatMod);
                toggleCheat.toggleAction = delegate (){ this.CheatMod = !this.CheatMod; };
                yield return toggleCheat;

                if(this.JobState != PS_Conditioning_JobState.Waiting)
                    yield return new Command_Action
                    {
                        defaultLabel = "Dev: Insta Complete",
                        action = delegate ()
                        {
                            if (this.JobState != PS_Conditioning_JobState.Waiting)
                                this.CurrentTicksLeft = 0;
                        },
                        icon = ContentFinder<Texture2D>.Get("UI/DevInstComplete", true)
                    };
                if (this.PodOwner != null)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Dev: Down Pawn Need",
                        action = delegate ()
                        {
                            PS_ConditioningHelper.SetCurrentNeedLevel(this.PodOwner, this.PodOwner.ConditioningLevel() - 0.1f);
                        },
                        icon = ContentFinder<Texture2D>.Get("UI/DevDown", true)
                    };
                    yield return new Command_Action
                    {
                        defaultLabel = "Dev: Fill Pawn Need",
                        action = delegate ()
                        {
                            PS_ConditioningHelper.SetCurrentNeedLevel(this.PodOwner, 1f);
                        },
                        icon = ContentFinder<Texture2D>.Get("UI/DevFull", true)
                    };
                    yield return new Command_Action
                    {
                        defaultLabel = "Dev: Half Pawn Need",
                        action = delegate ()
                        {
                            PS_ConditioningHelper.SetCurrentNeedLevel(this.PodOwner, 0.5f);
                        },
                        icon = ContentFinder<Texture2D>.Get("UI/DevHalf", true)
                    };
                }
            }
            yield break;
        }

        public override void DrawGUIOverlay()
        {
            if (this.CheatMod)
                GenMapUI.DrawThingLabel(this, "Cheat Mode", Color.red);
            else if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
            {
                Color defaultThingLabelColor = GenMapUI.DefaultThingLabelColor;
                if (this.PodOwner == null)
                {
                    GenMapUI.DrawThingLabel(this, "Unowned".Translate(), defaultThingLabelColor);
                }
                else if(this.JobState != PS_Conditioning_JobState.Waiting)
                {
                    // Draw nothing because the progress bar will be there
                }
                else
                {
                    GenMapUI.DrawThingLabel(this, this.PodOwner.LabelShort, defaultThingLabelColor);
                }
            }
        }
        
        public void StartReconditioning(Pawn pawn, PS_Conditioning_Data conData)
        {
            if (this.PodOwner != null && this.PodOwner != pawn)//!string.IsNullOrEmpty(this.PodOwnerId) && pawn.ThingID != this.PodOwnerId)
            {
                Log.Error("PS_Buildings_ReconPod: Tried to start conditioning on a pawn that was not the owner");
                return;
            }
            this.SaveNeeds(pawn.needs.AllNeeds);
            pawn.DeSpawn(DestroyMode.Vanish);
            if (this.TryAcceptThing(pawn))
            {
                this.ConditioningData = conData;
                this.CurrentMaxTicks = PS_ConditioningHelper.DaysToCondition(pawn) * this.TicksPerDay;
                this.CurrentTicksLeft = CurrentMaxTicks;
                this.JobState = PS_Conditioning_JobState.Reconditioning;
                this.TryAssignPawn(pawn);

                EffecterDef progressBar = EffecterDefOf.ProgressBar;
                ProgressBarEffector = progressBar.Spawn();
                var target = new TargetInfo(this);
                ProgressBarEffector.EffectTick(target, TargetInfo.Invalid);
            }

        }

        public void StartRefreshing(Pawn pawn, LocalTargetInfo targetInfo)
        {
            //if (pawn.ThingID != this.PodOwnerId)
            if(this.PodOwner != pawn)
                return;
            this.SaveNeeds(pawn.needs.AllNeeds);
            pawn.DeSpawn(DestroyMode.Vanish);
            if(this.TryAcceptThing(pawn))
            {
                this.CurrentTicksLeft = this.CurrentMaxTicks = this.TotalTicksPerAction;
                this.JobState = PS_Conditioning_JobState.Refreshing;


                EffecterDef progressBar = EffecterDefOf.ProgressBar;
                ProgressBarEffector = progressBar.Spawn();
                ProgressBarEffector.EffectTick(targetInfo.ToTargetInfo(this.Map), TargetInfo.Invalid);
            }

        }

        public void StartFixingBotched(Pawn pawn)
        {
            //if (pawn.ThingID != this.PodOwnerId)
            if (this.PodOwner != null && this.PodOwner != pawn)
                return;

            this.SaveNeeds(pawn.needs.AllNeeds);
            pawn.DeSpawn(DestroyMode.Vanish);
            if (this.TryAcceptThing(pawn))
            {
                this.CurrentTicksLeft = this.CurrentMaxTicks = this.TicksPerDay;
                this.JobState = PS_Conditioning_JobState.FixingBotch;


                EffecterDef progressBar = EffecterDefOf.ProgressBar;
                ProgressBarEffector = progressBar.Spawn();
                var target = new TargetInfo(this);
                ProgressBarEffector.EffectTick(target, TargetInfo.Invalid);
            }

        }

        public void StartDeconditioning(Pawn pawn, PS_Conditioning_Data conData)
        {
            //if (pawn.ThingID != this.PodOwnerId)
            if (this.PodOwner != pawn)
                return;
            this.SaveNeeds(pawn.needs.AllNeeds);
            pawn.DeSpawn(DestroyMode.Vanish);
            if (this.TryAcceptThing(pawn))
            {
                this.CurrentMaxTicks = this.TicksPerDay;
                this.CurrentTicksLeft = CurrentMaxTicks;
                this.JobState = PS_Conditioning_JobState.Deconditioning;
                this.ConditioningData = conData;

                EffecterDef progressBar = EffecterDefOf.ProgressBar;
                ProgressBarEffector = progressBar.Spawn();
                var target = new TargetInfo(this);
                ProgressBarEffector.EffectTick(target, TargetInfo.Invalid);
            }

        }

        public override void Tick()
        {
            base.Tick();

            if (this.PodOwner != null && this.PodOwner.Dead)
            {
                ForceEndJobState();
                this.PodOwner = null;
            }

            if(this.PodOwner != null && this.JobState == PS_Conditioning_JobState.Waiting && !PS_ConditioningHelper.IsReconditioned(this.PodOwner))
            {
                ForceEndJobState();
                this.PodOwner = null;
            }

            if (this.JobState == PS_Conditioning_JobState.Waiting) // Do nothing if waiting
                return;

            if(!this.TryGetComp<CompPowerTrader>().PowerOn && this.HasAnyContents) // Boot pawn if lose power
            {
                this.ForceEndJobState();
                this.EjectContents();
                return;
            }

            var pawn = (Pawn)innerContainer.FirstOrDefault();
            if(pawn != null)
            {
                this.ResetNeeds(pawn);
            }

            
            if (ProgressBarEffector != null)
            {
                MoteProgressBar mote = ((SubEffecter_ProgressBar)this.ProgressBarEffector?.children[0]).mote;
                if (mote != null)
                {
                    mote.progress = Mathf.Clamp01(1f - (this.CurrentTicksLeft / this.CurrentMaxTicks));
                    mote.offsetZ = -0.5f;
                }
            }

            if (this.JobState != PS_Conditioning_JobState.Waiting)
            {
                this.CurrentTicksLeft--;
                if (this.CurrentTicksLeft <= 0)
                    this.FinishConditioning();
            }
        }

        private void FinishConditioning()
        {
            var pawn = (Pawn)this.innerContainer.FirstOrDefault();
            if(pawn == null)
            {
                Log.Error("PS_Bulding_ReconPod: Finsihed conditioning put held pawn is null");
                this.EjectContents();
                this.ForceEndJobState();
                return;
            }

            switch (this.JobState)
            {
                case PS_Conditioning_JobState.Waiting:
                    return;

                case PS_Conditioning_JobState.Reconditioning:
                    if (RoleForSucessOrHandleFail(pawn))
                    {
                        PS_ConditioningHelper.DoConditioning(pawn, this, this.ConditioningData);
                        this.TryAssignPawn(pawn);
                        pawn.needs.mood.thoughts.memories.TryGainMemory(DefDatabase<ThoughtDef>.GetNamed("PS_Thoughts_Memory_Reconditioned"));
                        PS_ConditioningHelper.DirtyNeedFall(pawn);
                        PS_ConditioningHelper.SetCurrentNeedLevel(pawn, PS_ConditioningHelper.GetCurrentNeedLevel(pawn) + 1f);
                        Messages.Message(string.Format("PS_Messages_CompletedReconditioning".Translate(), pawn.LabelShort), new LookTargets(pawn), MessageTypeDefOf.TaskCompletion);
                    }
                    this.Open();
                    break;

                case PS_Conditioning_JobState.Refreshing:
                    PS_ConditioningHelper.SetCurrentNeedLevel(pawn, PS_ConditioningHelper.GetCurrentNeedLevel(pawn) +  0.6f);
                    PS_ConditioningHelper.DirtyNeedFall(pawn);
                    this.Open();
                    break;

                case PS_Conditioning_JobState.Deconditioning:
                    var lastConditioning = (PS_ConditioningHelper.GetConditioningDataFromHediff(pawn)?.Count() ?? 0) <= 1;
                    PS_ConditioningHelper.UndoConditioning(pawn, this, this.ConditioningData);
                    if (lastConditioning)
                    {
                        pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(DefDatabase<ThoughtDef>.GetNamed("PS_Thoughts_Memory_Reconditioned"));
                        pawn.needs.mood.thoughts.memories.TryGainMemory(DefDatabase<ThoughtDef>.GetNamed("PS_Thoughts_Memory_Deconditioned"));

                        this.TryUnassignPawn(pawn);
                    }
                    else
                    {
                        PS_ConditioningHelper.DirtyNeedFall(pawn);
                        PS_ConditioningHelper.SetCurrentNeedLevel(pawn, PS_ConditioningHelper.GetCurrentNeedLevel(pawn) + 1f);
                    }
                    this.Open();
                    Messages.Message(string.Format("PS_Messages_CompletedDeconditioning".Translate(), pawn.LabelShort), new LookTargets(pawn), MessageTypeDefOf.TaskCompletion);
                    break;

                case PS_Conditioning_JobState.FixingBotch:
                    PS_ConditioningHelper.RemoveBotch(pawn);
                    pawn.needs.mood.thoughts.memories.RemoveMemoriesOfDef(DefDatabase<ThoughtDef>.GetNamed("PS_Thoughts_Memory_BotchedConditioned"));
                    if (PS_ConditioningHelper.IsReconditioned(pawn))
                    {
                        PS_ConditioningHelper.DirtyNeedFall(pawn);
                        PS_ConditioningHelper.SetCurrentNeedLevel(pawn, PS_ConditioningHelper.GetCurrentNeedLevel(pawn) + 1f);
                    }
                    this.Open();
                    Messages.Message(string.Format("PS_Messages_CompletedFixingBotched".Translate(), pawn.LabelShort), new LookTargets(pawn), MessageTypeDefOf.TaskCompletion);
                    break;
            }

            if (ProgressBarEffector != null)
            {
                ProgressBarEffector.Cleanup();
                ProgressBarEffector = null;
            }
            this.JobState = PS_Conditioning_JobState.Waiting;

            // Move hediff to brain if it's not, fix for old version
            if (!this.CheckedHediffPart)
            {
                var hediff = pawn.health.hediffSet.GetHediffs<PS_Hediff_Reconditioned>().FirstOrDefault();
                if (hediff != null)
                {
                    var currentPart = hediff.Part;
                    var brain = pawn.RaceProps.body.AllParts.Where(x => x.def.defName == "Brain").FirstOrDefault();
                    if (brain != null && currentPart != brain)
                        hediff.Part = brain;
                }
                this.CheckedHediffPart = true;
            }
        }

        private void SaveNeeds(List<Need> needs)
        {
            if (this.StartingNeedLevels == null)
                this.StartingNeedLevels = new List<NeedValuePair>();

            this.StartingNeedLevels.Clear();

            foreach (var need in needs)
                this.StartingNeedLevels.Add(new NeedValuePair { DefName = need.def.defName, Value = need.CurLevel });
        }

        private void ResetNeeds(Pawn pawn)
        {
            if(this.StartingNeedLevels == null)
            {
                Log.Message(string.Format("PS_Buildings_ReconPod: Tried to reset needs for {0}, but starting needs list is null.", pawn.LabelShort));
            }
            foreach (var need in this.StartingNeedLevels)
            {
                var pawnNeed = pawn.needs.TryGetNeed(DefDatabase<NeedDef>.GetNamed(need.DefName));
                if (pawnNeed != null)
                    pawnNeed.CurLevel = need.Value;
            }
        }

        private bool RoleForSucessOrHandleFail(Pawn pawn)
        {

            var conCount = PS_ConditioningHelper.GetConditioningDataFromHediff(pawn, false)?.Count() ?? 0;
            var successChance = PS_ConditioningHelper.GetSucessChance(conCount );
            var roll = Rand.Range(0f, 1f);
            Log.Message(string.Format("PS_Buildings_ReconPod: Rolled for sucess Chance: {0}, Roll: {1}", successChance, roll));

            //roll = 1.00f;

            // Sucess
            if (roll < successChance)
                return true;

            roll = Rand.Range(0f, 1f);
            // Fail with no coniquence
            if (roll > 0.3f)
            {
                pawn.needs.mood.thoughts.memories.TryGainMemory(DefDatabase<ThoughtDef>.GetNamed("PS_Thoughts_Memory_FailedConditioned"));
                Messages.Message(string.Format("PS_Messages_FailConditioning".Translate(), pawn.LabelShort), new LookTargets(pawn), MessageTypeDefOf.NegativeEvent);
            }
            // Botched
            else if(roll > 0.01f)
            {
                var bothedcondata = new PS_Conditioning_Data
                {
                    AlterType = TraitAlterType.Added,
                    AddedTraitDefName = "PS_Trait_BotchedConditioning",
                    AddedDegree = -1
                };
                pawn.needs.mood.thoughts.memories.TryGainMemory(DefDatabase<ThoughtDef>.GetNamed("PS_Thoughts_Memory_BotchedConditioned"));
                Messages.Message(string.Format("PS_Messages_BotchedConditioning".Translate(), pawn.LabelShort), new LookTargets(pawn), MessageTypeDefOf.ThreatBig);
                PS_ConditioningHelper.DoTraitChange(pawn, bothedcondata);
            }
            // Lucky Botched
            else 
            {
                var condata = new PS_Conditioning_Data
                {
                    AlterType = TraitAlterType.Added,
                    AddedTraitDefName = "PS_Trait_BotchedConditioning",
                    AddedDegree = 1
                };
                pawn.needs.mood.thoughts.memories.TryGainMemory(DefDatabase<ThoughtDef>.GetNamed("PS_Thoughts_Memory_LuckyConditioned"));
                Messages.Message(string.Format("PS_Messages_LuckyConditioning".Translate(), pawn.LabelShort), new LookTargets(pawn), MessageTypeDefOf.ThreatBig);
                PS_ConditioningHelper.DoTraitChange(pawn, condata);
            }
            return false;
        }
    }
}
