using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;
using System.Reflection;
using System.Text;

namespace PS_ReconPod
{
    // Token: 0x02000095 RID: 149
    public class PS_Panel_Reconditioning : Window
    {
        private Pawn Pawn;
        private PS_Buildings_ReconPod Pod;
        private List<Trait> StartTraits;
        private List<Trait> CurrentTraits;
        private List<PS_Conditioning_Data> StartingConditioning;

        private bool Initalized;
        private Vector2 ScrollPosition;
        private PS_ScrollView<Trait> AddTraitScrollView;
        private PS_ScrollView<Trait> CurrentTraitScrollView;
        private PS_ScrollView<PS_Conditioning_Data> CurrentConditioningScrollView;

        private TraitAlterType ChangeType = TraitAlterType.UNSET;
        private Trait ToAdd;
        private Trait ToRemove;
        private PS_Conditioning_Data ConditioningToRemove;

        private bool RemoveingConditioning;
        private bool FixingBotch;
        public Func<string, string> ToolTipFunc;

        public bool CheatMode;

        public PS_Panel_Reconditioning()
        {
            this.forcePause = true;
            this.absorbInputAroundWindow = true;
            this.Initalized = false;
            this.draggable = true;
            this.focusWhenOpened = false;
        }

        private void DebugLog(string s)
        {
            Log.Message(string.Format("[PS] Reconditioning Pod Logging: {0}", s));
        }

        public void SetPawnAndPod(Pawn pawn, PS_Buildings_ReconPod Pod)
        {
            this.ToolTipFunc = (t => t);
            this.Pod = Pod;
            this.Pawn = pawn;

            this.StartTraits = pawn.story.traits.allTraits;
            this.CurrentTraits = new List<Trait>();
            foreach (var t in StartTraits)
                CurrentTraits.Add(t);

            this.StartingConditioning = new List<PS_Conditioning_Data>();
            if(PS_ConditioningHelper.IsReconditioned(pawn))
            {
                var condata = PS_ConditioningHelper.GetConditioningDataFromHediff(pawn);
                this.StartingConditioning = condata;
            }


            this.CheatMode = Pod.CheatMod;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (!this.Initalized)
            {
                this.Init(inRect);
            }

            Widgets.Label(new Rect(3f, CurrentTraitScrollView.DrawRect.y - 23f, this.windowRect.width, 20f), "PS_CurrentTraitsLab".Translate());
            Widgets.Label(new Rect((this.windowRect.width - (this.Margin * 2f)) * 0.5f + 3f, CurrentTraitScrollView.DrawRect.y - 23f, this.windowRect.width, 20f), "PS_OptionalTraitsLab".Translate());
            Widgets.Label(new Rect(3f, CurrentConditioningScrollView.DrawRect.y - 20f - 5f, this.windowRect.width, 20f), "PS_CurrentConditioningLab".Translate());

            GUI.color = Color.gray;
            var addtempRect = new Rect(AddTraitScrollView.DrawRect.x - 3f, AddTraitScrollView.DrawRect.y - 3f, AddTraitScrollView.DrawRect.width + 6f, AddTraitScrollView.DrawRect.height + 6f);
            Widgets.DrawBox(addtempRect);
            var curtempRect = new Rect(CurrentTraitScrollView.DrawRect.x - 3f, CurrentTraitScrollView.DrawRect.y - 3f, CurrentTraitScrollView.DrawRect.width + 6f, CurrentTraitScrollView.DrawRect.height + 6f);
            Widgets.DrawBox(curtempRect);
            var curContempRect = new Rect(CurrentConditioningScrollView.DrawRect.x - 3f, CurrentConditioningScrollView.DrawRect.y - 3f, CurrentConditioningScrollView.DrawRect.width + 6f, CurrentConditioningScrollView.DrawRect.height + 6f);
            Widgets.DrawBox(curContempRect);


            var labBox = new Rect(curContempRect.x, curContempRect.yMax + 3f, this.windowRect.width - this.Margin * 2f - 3f, 60f);
            Widgets.DrawBox(labBox);


            var refreshLabBox = new Rect(labBox.x, labBox.yMax + 3f, curContempRect.width, 26f);
            TooltipHandler.TipRegion(refreshLabBox, this.ToolTipFunc("PS_ToolTips_ConditioningFallRate".Translate()));
            Widgets.DrawBox(refreshLabBox);

            var daysBox = new Rect(addtempRect.x, labBox.yMax + 3f, addtempRect.width * 0.5f - 1.5f, 26f);
            Widgets.DrawBox(daysBox);
            TooltipHandler.TipRegion(daysBox, this.ToolTipFunc("PS_ToolTips_ConditioningTime".Translate()));
            var chanceBox = new Rect(daysBox.x + daysBox.width + 3f, daysBox.y, daysBox.width, daysBox.height);
            TooltipHandler.TipRegion(chanceBox, this.ToolTipFunc("PS_ToolTips_SuccessChance".Translate()));
            Widgets.DrawBox(chanceBox);

            GUI.color = Color.white;
            Widgets.Label(new Rect(refreshLabBox.x + 3f, refreshLabBox.y + 2f, refreshLabBox.width - 6f, refreshLabBox.height - 2f), string.Format("PS_UILabels_ConditioningFallRate".Translate(), this.GetRefreshRate()));
            Widgets.Label(new Rect(daysBox.x + 3f, daysBox.y + 2f, daysBox.width - 6f, daysBox.height -2f), string.Format("PS_UILabels_ConditioningTime".Translate(), this.GetDays()));
            Widgets.Label(new Rect(chanceBox.x + 3f, chanceBox.y + 2f, chanceBox.width - 6f, chanceBox.height - 2f), string.Format("PS_UILabels_SuccessChance".Translate(), this.GetFailChance()));
            
            Widgets.Label(new Rect(labBox.x + 3f, labBox.y + 2f, labBox.width - 6f, labBox.height - 2f), this.BuildInfoString());
            //Widgets.Label(labBox, (this.AddingTrait ? "Adding" : "Removeing") + ":\n " + (this.ToAlter != null ? this.ToAlter.LabelCap : "Unset"));

            AddTraitScrollView.Draw();
            CurrentTraitScrollView.Draw();
            CurrentConditioningScrollView.Draw();

            if (this.ChangeType == TraitAlterType.Added && this.StartTraits.Count() >= 3)
            {
                if (!PS_TextureLoader.Loaded)
                    PS_TextureLoader.Reset();
                var warnBox = new Rect(labBox.xMax - 20f, labBox.y + 2, 18f, 18f);
                Widgets.DrawAtlas(warnBox, PS_TextureLoader.Warning);

                TooltipHandler.TipRegion(warnBox, this.ToolTipFunc("PS_3OrMoreWarning".Translate()));
            }


            Text.Font = GameFont.Small;
            
            Text.Font = GameFont.Medium;
            // Cancel Button
            var cancelButtonRecGrid = GetRecForGridLocation(0, 5.5f, 1, 0.5f);// new Rect(inRect.width * 0.5f, inRect.height - inRect.height * 0.1f, inRect.width * 0.5f, inRect.height * 0.1f);
            var cancelButtonRectTrue = new Rect(cancelButtonRecGrid.x + 5, cancelButtonRecGrid.y, cancelButtonRecGrid.width - 10f, cancelButtonRecGrid.height);
            var cancelButton = Widgets.ButtonText(cancelButtonRectTrue, "PS_Cancel".Translate());
            if (cancelButton)
            {
                this.Close(true);
            }

            // Submit Button
            var submitButtonRecGrid = GetRecForGridLocation(1, 5.5f, 1, 0.5f);// new Rect(0, inRect.height - inRect.height * 0.1f, inRect.width * 0.5f, inRect.height * 0.1f);
            var submitButtonRectTrue = new Rect(submitButtonRecGrid.x + 5, submitButtonRecGrid.y, submitButtonRecGrid.width - 10f, submitButtonRecGrid.height);
            var button = Widgets.ButtonText(submitButtonRectTrue, "PS_Accept".Translate());
            if (button)
            {
                if (!this.CheatMode)
                    this.ApplyNewTraits(this.Pawn);
                this.Close(true);
            }
        }

        private string BuildInfoString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            if (CheatMode)
                return "PS_CheatModeString".Translate();

            if(this.RemoveingConditioning)
            {
                stringBuilder.AppendLine(string.Format("PS_SelectedTraitMessage".Translate(), this.ConditioningToRemove.ToPrettyString()));
                stringBuilder.AppendLine(string.Format("PS_UIPreviewMessage_RemoveConditioning".Translate(), Pawn.LabelShort, this.ConditioningToRemove.ToShortPrettyString().ToLower()));
                return stringBuilder.ToString().TrimEndNewlines();
            }
            else if(this.FixingBotch)
            {
                stringBuilder.AppendLine(string.Format("PS_SelectedTraitMessage".Translate(), "PS_SelectionString_FixBotched".Translate()));
                stringBuilder.AppendLine(string.Format("PS_UIPreviewMessage_FixBotch".Translate(), Pawn.LabelShort));
                return stringBuilder.ToString().TrimEndNewlines();
            }
            else if (this.ChangeType != TraitAlterType.UNSET && (this.ToAdd != null || this.ToRemove != null))
            {
                switch(this.ChangeType)
                {
                    case TraitAlterType.Added:
                        stringBuilder.AppendLine(string.Format("PS_SelectedTraitMessage".Translate(), this.ToAdd.LabelCap));
                        stringBuilder.AppendLine(string.Format("PS_UIPreviewMessage_Add".Translate(), Pawn.LabelShort, this.ToAdd?.Label ?? "PS_Unset".Translate()));
                        break;
                    case TraitAlterType.Removed:
                        stringBuilder.AppendLine(string.Format("PS_SelectedTraitMessage".Translate(), this.ToRemove.LabelCap));
                        stringBuilder.AppendLine(string.Format("PS_UIPreviewMessage_Remove".Translate(), Pawn.LabelShort, this.ToRemove?.Label ?? "PS_Unset".Translate()));
                        break;
                    case TraitAlterType.Altered:
                        stringBuilder.AppendLine(string.Format("PS_SelectedTraitMessage".Translate(), this.ToAdd.LabelCap));
                        stringBuilder.AppendLine(string.Format("PS_UIPreviewMessage_Change".Translate(), Pawn.LabelShort, this.ToRemove?.Label ?? "PS_Unset".Translate(), this.ToAdd?.Label ?? "PS_Unset".Translate()));
                        break;
                }
                return stringBuilder.ToString().TrimEndNewlines();
            }
            else
                return "PS_UIPreviewMessageInit".Translate();
        }

        public void Init(Rect rect)
        {
            this.ScrollPosition = new Vector2(0, 0);
            this.Initalized = true;

            var addTraitDrawRect = GetRecForGridLocation(1, 0, width: 1, height: 4).Rounded();
            var paddedAdd = new Rect(addTraitDrawRect.x + 5f, addTraitDrawRect.y + 5f + 20f, addTraitDrawRect.width - 10f, addTraitDrawRect.height - 10f).Rounded();
            AddTraitScrollView = new PS_ScrollView<Trait>(paddedAdd);

            var curTraitDrawRect = GetRecForGridLocation(0, 0, width: 1, height: 2).Rounded();
            var paddedcurt = new Rect(curTraitDrawRect.x + 5f, curTraitDrawRect.y + 5f + 20f, curTraitDrawRect.width - 10f, curTraitDrawRect.height - 20f).Rounded();
            CurrentTraitScrollView = new PS_ScrollView<Trait>(paddedcurt);

            var curConDrawRect = GetRecForGridLocation(0, 2, width: 1, height: 2).Rounded();
            var paddedcurtcon = new Rect(curTraitDrawRect.x + 5f, curConDrawRect.y + 5f + 30f, curTraitDrawRect.width - 10f, curTraitDrawRect.height - 20f).Rounded();
            CurrentConditioningScrollView = new PS_ScrollView<PS_Conditioning_Data>(paddedcurtcon);

            UpdateCurrentTraits(this.CurrentTraits);
            UpdateAddableTraits(this.CurrentTraits);
            UpdateCurrentConditioning();
        }
        
        public override void OnAcceptKeyPressed()
        {
            if(!this.CheatMode)
                this.ApplyNewTraits(this.Pawn);
            base.OnAcceptKeyPressed();
        }

        public override void OnCancelKeyPressed()
        {
            base.OnCancelKeyPressed();
        }
        
        private void ApplyNewTraits(Pawn Pawn)
        {
            if (this.RemoveingConditioning)
            {
                this.Pod.StartDeconditioning(Pawn, this.ConditioningToRemove);
            }
            else if(this.FixingBotch)
            {
                this.Pod.StartFixingBotched(Pawn);
            }
            else if (this.ChangeType != TraitAlterType.UNSET)
            {
                var conData = new PS_Conditioning_Data
                {
                    AlterType = this.ChangeType,
                    OriginalTraitDefName = this.ToRemove?.def.defName,
                    AddedTraitDefName = this.ToAdd?.def.defName,
                    OriginalDegree = this.ToRemove?.Degree ?? 0,
                    AddedDegree = this.ToAdd?.Degree ?? 0
                };
                this.Pod.StartReconditioning(Pawn, conData);
            }
        }

        public string GetRefreshRate()
        {
            if(CheatMode)
                return "CM";
            
            if (this.FixingBotch || (!this.RemoveingConditioning && this.ChangeType == TraitAlterType.UNSET))
            {
                var days = PS_ConditioningHelper.GetRefreshPerDay(this.StartingConditioning?.Count() ?? 0);
                return DayToSafeTime(days);
            }
            else
            {
                var tempConCount = this.StartingConditioning?.Count() ?? 0;
                if (this.RemoveingConditioning)
                    tempConCount--;
                else if (this.ChangeType != TraitAlterType.UNSET)
                    tempConCount++;

                var days = PS_ConditioningHelper.GetRefreshPerDay(tempConCount);
                return DayToSafeTime(days);
            }
        }

        public string GetDays()
        {
            if (CheatMode)
                return "CM";



            if (!this.FixingBotch && !this.RemoveingConditioning && this.ChangeType == TraitAlterType.UNSET)
                return "NA";

            if (this.FixingBotch)
                return DayToSafeTime(1f);

            if (this.RemoveingConditioning)
                return DayToSafeTime(1f);

            var tempConCount = this.StartingConditioning?.Count() ?? 0;
            if (this.RemoveingConditioning)
                tempConCount--;

            var days = PS_ConditioningHelper.DaysToCondition(tempConCount);
            return DayToSafeTime(days);
        }

        public string GetSelectedString()
        {
            if (CheatMode)
                return "CM";

            if (this.RemoveingConditioning)
                return "PS_SelectionString_RemoveingConditioning".Translate();
            else if (this.FixingBotch)
                return "PS_SelectionString_FixBotched".Translate();
            else if (this.ChangeType == TraitAlterType.Added || this.ChangeType == TraitAlterType.Altered)
                return this.ToAdd.Label;
            else if (this.ChangeType == TraitAlterType.Removed)
                return this.ToRemove.Label;
            else
                return "PS_Unset".Translate();
        }

        public string GetFailChance()
        {

            if (CheatMode)
                return "CM";
            if (!this.FixingBotch && !this.RemoveingConditioning && this.ChangeType == TraitAlterType.UNSET)
                return "NA";

            if (FixingBotch)
                return "100";
            if (this.RemoveingConditioning)
                return "100";

            var startConCount = this.StartingConditioning?.Count() ?? 0;
            return ((PS_ConditioningHelper.GetSucessChance(startConCount) * 100f)).ToString("0");
        }


        public void AddTrait(Trait t)
        {
            this.RemoveingConditioning = false;
            this.FixingBotch = false;
            this.ToAdd = t;
            this.ToRemove = null;
            this.ChangeType = TraitAlterType.Added;

            if (this.CheatMode)
                DoCheatModeChange();
        }

        public void RemoveTrait(Trait t)
        {
            this.RemoveingConditioning = false;
            this.FixingBotch = false;
            this.ToRemove = t;
            this.ToAdd = null;
            this.ChangeType = TraitAlterType.Removed;

            if (this.CheatMode)
                DoCheatModeChange();
        }

        public void AlterTrait(Trait t1, Trait t2)
        {
            this.RemoveingConditioning = false;
            this.FixingBotch = false;
            this.FixingBotch = false;
            this.ToRemove = t1;
            this.ToAdd = t2;
            this.ChangeType = TraitAlterType.Altered;

            if (this.CheatMode)
                DoCheatModeChange();
        }

        public void DoCheatModeChange()
        {
            var conData = new PS_Conditioning_Data
            {
                AlterType = this.ChangeType,
                OriginalTraitDefName = this.ToRemove?.def.defName,
                AddedTraitDefName = this.ToAdd?.def.defName,
                OriginalDegree = this.ToRemove?.Degree ?? 0,
                AddedDegree = this.ToAdd?.Degree ?? 0
            };
            PS_ConditioningHelper.DoTraitChange(this.Pawn, conData);
            if(conData.AlterType == TraitAlterType.Added || conData.AlterType == TraitAlterType.Altered)
                this.CurrentTraits.Add(this.ToAdd);
            if(conData.AlterType == TraitAlterType.Removed || conData.AlterType == TraitAlterType.Altered)
                this.CurrentTraits.Remove(this.ToRemove);
            UpdateAddableTraits(this.CurrentTraits);
            UpdateCurrentTraits(this.CurrentTraits);
        }

        public void SetConditioningToRemove(PS_Conditioning_Data data)
        {
            this.RemoveingConditioning = true;
            this.FixingBotch = false;
            this.ConditioningToRemove = data;
            this.ToRemove = null;
            this.ToAdd = null;
            this.ChangeType = TraitAlterType.UNSET;
        }

        public void SetFixingBotch()
        {
            this.FixingBotch = true;
            this.RemoveingConditioning = false;
            //this.ConditioningToRemove = data;
            this.ToRemove = null;
            this.ToAdd = null;
            this.ChangeType = TraitAlterType.UNSET;
        }

        public void UpdateAddableTraits(List<Trait> currentTraits)
        {
            var traits = PS_TraitHelper.AllTraitsCompadable(currentTraits, IncludeBlack: CheatMode);

            var options = traits.Select(trait =>
                new PS_ScrollView<Trait>.ScrollOption<Trait>
                {
                    Index = 0,
                    Value = trait,
                    Label = trait.LabelCap,
                    ToolTip = trait.TipString(this.Pawn),
                    ButtonText = "PS_Add".Translate(),
                    ButtonAction = delegate { this.AddTrait(trait); }
                }).ToList();
            AddTraitScrollView.TrySetOptions(options);
        }

        public void UpdateCurrentTraits(List<Trait> traits)
        {
            var options = traits.Select(trait =>
            {
                var black = PS_TraitHelper.IsBlacklisted(trait);
                var wasAdded = this.StartingConditioning?.Where(x => x.AddedTraitDefName == trait.def.defName).Any() ?? false;
                var valid = !(black || wasAdded);
                if (CheatMode)
                    valid = true;
                var opt = new PS_ScrollView<Trait>.ScrollOption<Trait>
                {
                    Index = 0,
                    Value = trait,
                    Label = trait.LabelCap,
                    ToolTip = trait.TipString(this.Pawn),
                    HasButton = valid
                };
                if(trait.def.defName == "PS_Trait_BotchedConditioning" && trait.Degree == -1)
                {
                    opt.ButtonAction = delegate { this.SetFixingBotch(); };
                    opt.ButtonText = "PS_Fix".Translate();
                    opt.HasButton = true;
                }
                if (valid && trait.def.degreeDatas.Count < 2)
                {
                    opt.ButtonAction = delegate { this.RemoveTrait(trait); };
                    opt.ButtonText = "PS_Remove".Translate();
                }
                else if(valid)
                {
                    opt.ButtonAction = delegate { this.ShowDegreeOptions(trait); };
                    opt.ButtonText = "PS_Change".Translate();
                }
                return opt;
            }).ToList();
            CurrentTraitScrollView.TrySetOptions(options);
        }

        public void UpdateCurrentConditioning()
        {
            var options = this.StartingConditioning.Where(x => x.IsValid()).Select(con =>
            {
                var opt = new PS_ScrollView<PS_Conditioning_Data>.ScrollOption<PS_Conditioning_Data>
                {
                    Index = 0,
                    Value = con,
                    Label = con.ToShortPrettyString(),
                    ToolTip = con.ToPrettyString()
                };
                opt.ButtonAction = delegate { this.SetConditioningToRemove(con); };
                opt.ButtonText = "PS_Remove".Translate();
                return opt;
            }).ToList();
            CurrentConditioningScrollView.TrySetOptions(options);
        }

        public void ShowDegreeOptions(Trait trait)
        {
            var dropDownActions = new List<Action>();
            var dropDownList = new List<FloatMenuOption>();
            
            dropDownActions.Add(delegate { this.RemoveTrait(trait); });
            dropDownList.Add(new FloatMenuOption("PS_Remove".Translate(), dropDownActions.Last(), MenuOptionPriority.Default, null, null, 0f, null, null));

            foreach (var degree in trait.def.degreeDatas)
            {
                if (degree.degree != trait.Degree)
                {
                    var label = degree.label;
                    dropDownActions.Add(delegate { this.AlterTrait(trait, new Trait(trait.def, degree: degree.degree)); });
                    dropDownList.Add(new FloatMenuOption("PS_ChangeTo".Translate() + label, dropDownActions.Last(), MenuOptionPriority.Default, null, null, 0f, null, null));
                }
            }
            Find.WindowStack.Add(new FloatMenu(dropDownList, null, false));
        }

        private Rect GetRecForGridLocation(float x, float y, float width = 1f, float height = 1f, float MaxWidth = 2f, float MaxHeight = 6f)
        {
            var drawRect = new Rect(0, 0, this.windowRect.width - (this.Margin * 2f), this.windowRect.height - (this.Margin * 2f));

            float gridBoxWidth = drawRect.width / MaxWidth;
            float gridBoxHeight = drawRect.height / MaxHeight;

            return new Rect(gridBoxWidth * x, gridBoxHeight * y, gridBoxWidth * width, gridBoxHeight * height);
        }

        private string DayToSafeTime(float days)
        {
            if (days > 1f)
                return days.ToString("0.0") + " " + "PS_Time_Days".Translate();
            if(days == 1f)
                return days.ToString("0") + " " + "PS_Time_Day".Translate();
            
            var hours = days * 24f;
            if(hours > 1f)
                return hours.ToString("0.0") + " " + "PS_Time_Hours".Translate();
            if (hours == 1f)
                return hours.ToString("0") + " " + "PS_Time_Hour".Translate();

            var minutes = hours * 24f;
            if (minutes == 1f)
                return minutes.ToString("0") + " " + "PS_Time_Minute".Translate();

            return minutes.ToString("0.0") + " " + "PS_Time_Minutes".Translate();

        }
    }
}
