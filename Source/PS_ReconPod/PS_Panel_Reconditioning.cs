using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace PS_ReconPod
{
    // Token: 0x02000095 RID: 149
    public class PS_Panel_Reconditioning : Window
    {
        private PS_ScrollView<Trait> AddTraitScrollView;

        private TraitAlterType ChangeType = TraitAlterType.UNSET;

        public bool CheatMode;
        private PS_Conditioning_Data ConditioningToRemove;
        private PS_ScrollView<PS_Conditioning_Data> CurrentConditioningScrollView;
        private List<Trait> CurrentTraits;
        private PS_ScrollView<Trait> CurrentTraitScrollView;
        private bool FixingBotch;

        private bool Initalized;
        private Pawn Pawn;
        private PS_Buildings_ReconPod Pod;

        private bool RemoveingConditioning;
        private Vector2 ScrollPosition;
        private List<PS_Conditioning_Data> StartingConditioning;
        private List<Trait> StartTraits;
        private Trait ToAdd;
        public Func<string, string> ToolTipFunc;
        private Trait ToRemove;

        public PS_Panel_Reconditioning()
        {
            forcePause = true;
            absorbInputAroundWindow = true;
            Initalized = false;
            draggable = true;
            focusWhenOpened = false;
        }

        private void DebugLog(string s)
        {
            Log.Message($"[PS] Reconditioning Pod Logging: {s}");
        }

        public void SetPawnAndPod(Pawn pawn, PS_Buildings_ReconPod reconPod)
        {
            ToolTipFunc = t => t;
            Pod = reconPod;
            Pawn = pawn;

            StartTraits = pawn.story.traits.allTraits;
            CurrentTraits = new List<Trait>();
            foreach (var t in StartTraits)
            {
                CurrentTraits.Add(t);
            }

            StartingConditioning = new List<PS_Conditioning_Data>();
            if (PS_ConditioningHelper.IsReconditioned(pawn))
            {
                var condata = PS_ConditioningHelper.GetConditioningDataFromHediff(pawn);
                StartingConditioning = condata;
            }


            CheatMode = reconPod.CheatMod;
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (!Initalized)
            {
                Init(inRect);
            }

            Widgets.Label(new Rect(3f, CurrentTraitScrollView.DrawRect.y - 23f, windowRect.width, 20f),
                "PS_CurrentTraitsLab".Translate());
            Widgets.Label(
                new Rect(((windowRect.width - (Margin * 2f)) * 0.5f) + 3f, CurrentTraitScrollView.DrawRect.y - 23f,
                    windowRect.width, 20f), "PS_OptionalTraitsLab".Translate());
            Widgets.Label(new Rect(3f, CurrentConditioningScrollView.DrawRect.y - 20f - 5f, windowRect.width, 20f),
                "PS_CurrentConditioningLab".Translate());

            GUI.color = Color.gray;
            var addtempRect = new Rect(AddTraitScrollView.DrawRect.x - 3f, AddTraitScrollView.DrawRect.y - 3f,
                AddTraitScrollView.DrawRect.width + 6f, AddTraitScrollView.DrawRect.height + 6f);
            Widgets.DrawBox(addtempRect);
            var curtempRect = new Rect(CurrentTraitScrollView.DrawRect.x - 3f, CurrentTraitScrollView.DrawRect.y - 3f,
                CurrentTraitScrollView.DrawRect.width + 6f, CurrentTraitScrollView.DrawRect.height + 6f);
            Widgets.DrawBox(curtempRect);
            var curContempRect = new Rect(CurrentConditioningScrollView.DrawRect.x - 3f,
                CurrentConditioningScrollView.DrawRect.y - 3f, CurrentConditioningScrollView.DrawRect.width + 6f,
                CurrentConditioningScrollView.DrawRect.height + 6f);
            Widgets.DrawBox(curContempRect);


            var labBox = new Rect(curContempRect.x, curContempRect.yMax + 3f, windowRect.width - (Margin * 2f) - 3f,
                60f);
            Widgets.DrawBox(labBox);


            var refreshLabBox = new Rect(labBox.x, labBox.yMax + 3f, curContempRect.width, 26f);
            TooltipHandler.TipRegion(refreshLabBox, ToolTipFunc("PS_ToolTips_ConditioningFallRate".Translate()));
            Widgets.DrawBox(refreshLabBox);

            var daysBox = new Rect(addtempRect.x, labBox.yMax + 3f, (addtempRect.width * 0.5f) - 1.5f, 26f);
            Widgets.DrawBox(daysBox);
            TooltipHandler.TipRegion(daysBox, ToolTipFunc("PS_ToolTips_ConditioningTime".Translate()));
            var chanceBox = new Rect(daysBox.x + daysBox.width + 3f, daysBox.y, daysBox.width, daysBox.height);
            TooltipHandler.TipRegion(chanceBox, ToolTipFunc("PS_ToolTips_SuccessChance".Translate()));
            Widgets.DrawBox(chanceBox);

            GUI.color = Color.white;
            Widgets.Label(
                new Rect(refreshLabBox.x + 3f, refreshLabBox.y + 2f, refreshLabBox.width - 6f,
                    refreshLabBox.height - 2f),
                string.Format("PS_UILabels_ConditioningFallRate".Translate(), GetRefreshRate()));
            Widgets.Label(new Rect(daysBox.x + 3f, daysBox.y + 2f, daysBox.width - 6f, daysBox.height - 2f),
                string.Format("PS_UILabels_ConditioningTime".Translate(), GetDays()));
            Widgets.Label(new Rect(chanceBox.x + 3f, chanceBox.y + 2f, chanceBox.width - 6f, chanceBox.height - 2f),
                string.Format("PS_UILabels_SuccessChance".Translate(), GetFailChance()));

            Widgets.Label(new Rect(labBox.x + 3f, labBox.y + 2f, labBox.width - 6f, labBox.height - 2f),
                BuildInfoString());
            //Widgets.Label(labBox, (this.AddingTrait ? "Adding" : "Removeing") + ":\n " + (this.ToAlter != null ? this.ToAlter.LabelCap : "Unset"));

            AddTraitScrollView.Draw();
            CurrentTraitScrollView.Draw();
            CurrentConditioningScrollView.Draw();

            if (ChangeType == TraitAlterType.Added && StartTraits.Count >= 3)
            {
                if (!PS_TextureLoader.Loaded)
                {
                    PS_TextureLoader.Reset();
                }

                var warnBox = new Rect(labBox.xMax - 20f, labBox.y + 2, 18f, 18f);
                Widgets.DrawAtlas(warnBox, PS_TextureLoader.Warning);

                TooltipHandler.TipRegion(warnBox, ToolTipFunc("PS_3OrMoreWarning".Translate()));
            }


            Text.Font = GameFont.Small;

            Text.Font = GameFont.Medium;
            // Cancel Button
            var cancelButtonRecGrid =
                GetRecForGridLocation(0, 5.5f, 1,
                    0.5f); // new Rect(inRect.width * 0.5f, inRect.height - inRect.height * 0.1f, inRect.width * 0.5f, inRect.height * 0.1f);
            var cancelButtonRectTrue = new Rect(cancelButtonRecGrid.x + 5, cancelButtonRecGrid.y,
                cancelButtonRecGrid.width - 10f, cancelButtonRecGrid.height);
            var cancelButton = Widgets.ButtonText(cancelButtonRectTrue, "PS_Cancel".Translate());
            if (cancelButton)
            {
                Close();
            }

            // Submit Button
            var submitButtonRecGrid =
                GetRecForGridLocation(1, 5.5f, 1,
                    0.5f); // new Rect(0, inRect.height - inRect.height * 0.1f, inRect.width * 0.5f, inRect.height * 0.1f);
            var submitButtonRectTrue = new Rect(submitButtonRecGrid.x + 5, submitButtonRecGrid.y,
                submitButtonRecGrid.width - 10f, submitButtonRecGrid.height);
            var button = Widgets.ButtonText(submitButtonRectTrue, "PS_Accept".Translate());
            if (!button)
            {
                return;
            }

            if (!CheatMode)
            {
                ApplyNewTraits(Pawn);
            }

            Close();
        }

        private string BuildInfoString()
        {
            var stringBuilder = new StringBuilder();

            if (CheatMode)
            {
                return "PS_CheatModeString".Translate();
            }

            if (RemoveingConditioning)
            {
                stringBuilder.AppendLine(string.Format("PS_SelectedTraitMessage".Translate(),
                    ConditioningToRemove.ToPrettyString()));
                stringBuilder.AppendLine(string.Format("PS_UIPreviewMessage_RemoveConditioning".Translate(),
                    Pawn.LabelShort, ConditioningToRemove.ToShortPrettyString().ToLower()));
                return stringBuilder.ToString().TrimEndNewlines();
            }

            if (FixingBotch)
            {
                stringBuilder.AppendLine(string.Format("PS_SelectedTraitMessage".Translate(),
                    "PS_SelectionString_FixBotched".Translate()));
                stringBuilder.AppendLine(string.Format("PS_UIPreviewMessage_FixBotch".Translate(), Pawn.LabelShort));
                return stringBuilder.ToString().TrimEndNewlines();
            }

            if (ChangeType == TraitAlterType.UNSET || ToAdd == null && ToRemove == null)
            {
                return "PS_UIPreviewMessageInit".Translate();
            }

            switch (ChangeType)
            {
                case TraitAlterType.Added:
                    if (ToAdd != null)
                    {
                        stringBuilder.AppendLine(string.Format("PS_SelectedTraitMessage".Translate(), ToAdd.LabelCap));
                        stringBuilder.AppendLine(string.Format("PS_UIPreviewMessage_Add".Translate(), Pawn.LabelShort,
                            ToAdd?.Label ?? "PS_Unset".Translate()));
                    }

                    break;
                case TraitAlterType.Removed:
                    stringBuilder.AppendLine(
                        string.Format("PS_SelectedTraitMessage".Translate(), ToRemove.LabelCap));
                    stringBuilder.AppendLine(string.Format("PS_UIPreviewMessage_Remove".Translate(),
                        Pawn.LabelShort, ToRemove?.Label ?? "PS_Unset".Translate()));
                    break;
                case TraitAlterType.Altered:
                    if (ToAdd != null)
                    {
                        stringBuilder.AppendLine(string.Format("PS_SelectedTraitMessage".Translate(), ToAdd.LabelCap));
                        stringBuilder.AppendLine(string.Format("PS_UIPreviewMessage_Change".Translate(),
                            Pawn.LabelShort, ToRemove?.Label ?? "PS_Unset".Translate(),
                            ToAdd?.Label ?? "PS_Unset".Translate()));
                    }

                    break;
            }

            return stringBuilder.ToString().TrimEndNewlines();
        }

        public void Init(Rect rect)
        {
            ScrollPosition = new Vector2(0, 0);
            Initalized = true;

            var addTraitDrawRect = GetRecForGridLocation(1, 0, 1, 4).Rounded();
            var paddedAdd = new Rect(addTraitDrawRect.x + 5f, addTraitDrawRect.y + 5f + 20f,
                addTraitDrawRect.width - 10f, addTraitDrawRect.height - 10f).Rounded();
            AddTraitScrollView = new PS_ScrollView<Trait>(paddedAdd);

            var curTraitDrawRect = GetRecForGridLocation(0, 0, 1, 2).Rounded();
            var paddedcurt = new Rect(curTraitDrawRect.x + 5f, curTraitDrawRect.y + 5f + 20f,
                curTraitDrawRect.width - 10f, curTraitDrawRect.height - 20f).Rounded();
            CurrentTraitScrollView = new PS_ScrollView<Trait>(paddedcurt);

            var curConDrawRect = GetRecForGridLocation(0, 2, 1, 2).Rounded();
            var paddedcurtcon = new Rect(curTraitDrawRect.x + 5f, curConDrawRect.y + 5f + 30f,
                curTraitDrawRect.width - 10f, curTraitDrawRect.height - 20f).Rounded();
            CurrentConditioningScrollView = new PS_ScrollView<PS_Conditioning_Data>(paddedcurtcon);

            UpdateCurrentTraits(CurrentTraits);
            UpdateAddableTraits(CurrentTraits);
            UpdateCurrentConditioning();
        }

        public override void OnAcceptKeyPressed()
        {
            if (!CheatMode)
            {
                ApplyNewTraits(Pawn);
            }

            base.OnAcceptKeyPressed();
        }

        private void ApplyNewTraits(Pawn pawn)
        {
            if (RemoveingConditioning)
            {
                Pod.StartDeconditioning(pawn, ConditioningToRemove);
            }
            else if (FixingBotch)
            {
                Pod.StartFixingBotched(pawn);
            }
            else if (ChangeType != TraitAlterType.UNSET)
            {
                var conData = new PS_Conditioning_Data
                {
                    AlterType = ChangeType,
                    OriginalTraitDefName = ToRemove?.def.defName,
                    AddedTraitDefName = ToAdd?.def.defName,
                    OriginalDegree = ToRemove?.Degree ?? 0,
                    AddedDegree = ToAdd?.Degree ?? 0
                };
                Pod.StartReconditioning(pawn, conData);
            }
        }

        public string GetRefreshRate()
        {
            if (CheatMode)
            {
                return "CM";
            }

            if (FixingBotch || !RemoveingConditioning && ChangeType == TraitAlterType.UNSET)
            {
                var days = PS_ConditioningHelper.GetRefreshPerDay(StartingConditioning?.Count ?? 0);
                return DayToSafeTime(days);
            }
            else
            {
                var tempConCount = StartingConditioning?.Count ?? 0;
                if (RemoveingConditioning)
                {
                    tempConCount--;
                }
                else if (ChangeType != TraitAlterType.UNSET)
                {
                    tempConCount++;
                }

                var days = PS_ConditioningHelper.GetRefreshPerDay(tempConCount);
                return DayToSafeTime(days);
            }
        }

        public string GetDays()
        {
            if (CheatMode)
            {
                return "CM";
            }

            if (!FixingBotch && !RemoveingConditioning && ChangeType == TraitAlterType.UNSET)
            {
                return "NA";
            }

            if (FixingBotch)
            {
                return DayToSafeTime(1f);
            }

            if (RemoveingConditioning)
            {
                return DayToSafeTime(1f);
            }

            var tempConCount = StartingConditioning?.Count ?? 0;
            if (RemoveingConditioning)
            {
                tempConCount--;
            }

            var days = PS_ConditioningHelper.DaysToCondition(tempConCount);
            return DayToSafeTime(days);
        }

        public string GetSelectedString()
        {
            if (CheatMode)
            {
                return "CM";
            }

            if (RemoveingConditioning)
            {
                return "PS_SelectionString_RemoveingConditioning".Translate();
            }

            if (FixingBotch)
            {
                return "PS_SelectionString_FixBotched".Translate();
            }

            if (ChangeType == TraitAlterType.Added || ChangeType == TraitAlterType.Altered)
            {
                return ToAdd.Label;
            }

            if (ChangeType == TraitAlterType.Removed)
            {
                return ToRemove.Label;
            }

            return "PS_Unset".Translate();
        }

        public string GetFailChance()
        {
            if (CheatMode)
            {
                return "CM";
            }

            if (!FixingBotch && !RemoveingConditioning && ChangeType == TraitAlterType.UNSET)
            {
                return "NA";
            }

            if (FixingBotch)
            {
                return "100";
            }

            if (RemoveingConditioning)
            {
                return "100";
            }

            var startConCount = StartingConditioning?.Count ?? 0;
            return (PS_ConditioningHelper.GetSucessChance(startConCount) * 100f).ToString("0");
        }


        public void AddTrait(Trait t)
        {
            RemoveingConditioning = false;
            FixingBotch = false;
            ToAdd = t;
            ToRemove = null;
            ChangeType = TraitAlterType.Added;

            if (CheatMode)
            {
                DoCheatModeChange();
            }
        }

        public void RemoveTrait(Trait t)
        {
            RemoveingConditioning = false;
            FixingBotch = false;
            ToRemove = t;
            ToAdd = null;
            ChangeType = TraitAlterType.Removed;

            if (CheatMode)
            {
                DoCheatModeChange();
            }
        }

        public void AlterTrait(Trait t1, Trait t2)
        {
            RemoveingConditioning = false;
            FixingBotch = false;
            FixingBotch = false;
            ToRemove = t1;
            ToAdd = t2;
            ChangeType = TraitAlterType.Altered;

            if (CheatMode)
            {
                DoCheatModeChange();
            }
        }

        public void DoCheatModeChange()
        {
            var conData = new PS_Conditioning_Data
            {
                AlterType = ChangeType,
                OriginalTraitDefName = ToRemove?.def.defName,
                AddedTraitDefName = ToAdd?.def.defName,
                OriginalDegree = ToRemove?.Degree ?? 0,
                AddedDegree = ToAdd?.Degree ?? 0
            };
            PS_ConditioningHelper.DoTraitChange(Pawn, conData);
            if (conData.AlterType == TraitAlterType.Added || conData.AlterType == TraitAlterType.Altered)
            {
                CurrentTraits.Add(ToAdd);
            }

            if (conData.AlterType == TraitAlterType.Removed || conData.AlterType == TraitAlterType.Altered)
            {
                CurrentTraits.Remove(ToRemove);
            }

            UpdateAddableTraits(CurrentTraits);
            UpdateCurrentTraits(CurrentTraits);
        }

        public void SetConditioningToRemove(PS_Conditioning_Data data)
        {
            RemoveingConditioning = true;
            FixingBotch = false;
            ConditioningToRemove = data;
            ToRemove = null;
            ToAdd = null;
            ChangeType = TraitAlterType.UNSET;
        }

        public void SetFixingBotch()
        {
            FixingBotch = true;
            RemoveingConditioning = false;
            //this.ConditioningToRemove = data;
            ToRemove = null;
            ToAdd = null;
            ChangeType = TraitAlterType.UNSET;
        }

        public void UpdateAddableTraits(List<Trait> currentTraits)
        {
            var traits = PS_TraitHelper.AllTraitsCompadable(currentTraits, CheatMode);

            var options = traits.Select(trait =>
                new PS_ScrollView<Trait>.ScrollOption<Trait>
                {
                    Index = 0,
                    Value = trait,
                    Label = trait.LabelCap,
                    ToolTip = trait.TipString(Pawn),
                    ButtonText = "PS_Add".Translate(),
                    ButtonAction = delegate { AddTrait(trait); }
                }).ToList();
            AddTraitScrollView.TrySetOptions(options);
        }

        public void UpdateCurrentTraits(List<Trait> traits)
        {
            var options = traits.Select(trait =>
            {
                var black = PS_TraitHelper.IsBlacklisted(trait);
                var wasAdded = StartingConditioning?.Where(x => x.AddedTraitDefName == trait.def.defName).Any() ??
                               false;
                var valid = !(black || wasAdded) || CheatMode;

                var opt = new PS_ScrollView<Trait>.ScrollOption<Trait>
                {
                    Index = 0,
                    Value = trait,
                    Label = trait.LabelCap,
                    ToolTip = trait.TipString(Pawn),
                    HasButton = valid
                };
                if (trait.def.defName == "PS_Trait_BotchedConditioning" && trait.Degree == -1)
                {
                    opt.ButtonAction = SetFixingBotch;
                    opt.ButtonText = "PS_Fix".Translate();
                    opt.HasButton = true;
                }

                if (valid && trait.def.degreeDatas.Count < 2)
                {
                    opt.ButtonAction = delegate { RemoveTrait(trait); };
                    opt.ButtonText = "PS_Remove".Translate();
                }
                else if (valid)
                {
                    opt.ButtonAction = delegate { ShowDegreeOptions(trait); };
                    opt.ButtonText = "PS_Change".Translate();
                }

                return opt;
            }).ToList();
            CurrentTraitScrollView.TrySetOptions(options);
        }

        public void UpdateCurrentConditioning()
        {
            var options = StartingConditioning.Where(x => x.IsValid()).Select(con =>
            {
                var opt = new PS_ScrollView<PS_Conditioning_Data>.ScrollOption<PS_Conditioning_Data>
                {
                    Index = 0,
                    Value = con,
                    Label = con.ToShortPrettyString(),
                    ToolTip = con.ToPrettyString(),
                    ButtonAction = delegate { SetConditioningToRemove(con); },
                    ButtonText = "PS_Remove".Translate()
                };
                return opt;
            }).ToList();
            CurrentConditioningScrollView.TrySetOptions(options);
        }

        public void ShowDegreeOptions(Trait trait)
        {
            var dropDownActions = new List<Action>();
            var dropDownList = new List<FloatMenuOption>();

            dropDownActions.Add(delegate { RemoveTrait(trait); });
            dropDownList.Add(new FloatMenuOption("PS_Remove".Translate(), dropDownActions.Last()));

            foreach (var degree in trait.def.degreeDatas)
            {
                if (degree.degree == trait.Degree)
                {
                    continue;
                }

                var label = degree.label;
                dropDownActions.Add(delegate { AlterTrait(trait, new Trait(trait.def, degree.degree)); });
                dropDownList.Add(new FloatMenuOption("PS_ChangeTo".Translate() + label, dropDownActions.Last()));
            }

            Find.WindowStack.Add(new FloatMenu(dropDownList, null));
        }

        private Rect GetRecForGridLocation(float x, float y, float width = 1f, float height = 1f, float MaxWidth = 2f,
            float MaxHeight = 6f)
        {
            var drawRect = new Rect(0, 0, windowRect.width - (Margin * 2f), windowRect.height - (Margin * 2f));

            var gridBoxWidth = drawRect.width / MaxWidth;
            var gridBoxHeight = drawRect.height / MaxHeight;

            return new Rect(gridBoxWidth * x, gridBoxHeight * y, gridBoxWidth * width, gridBoxHeight * height);
        }

        private string DayToSafeTime(float days)
        {
            if (days > 1f)
            {
                return days.ToString("0.0") + " " + "PS_Time_Days".Translate();
            }

            if (days == 1f)
            {
                return days.ToString("0") + " " + "PS_Time_Day".Translate();
            }

            var hours = days * 24f;
            if (hours > 1f)
            {
                return hours.ToString("0.0") + " " + "PS_Time_Hours".Translate();
            }

            if (hours == 1f)
            {
                return hours.ToString("0") + " " + "PS_Time_Hour".Translate();
            }

            var minutes = hours * 24f;
            if (minutes == 1f)
            {
                return minutes.ToString("0") + " " + "PS_Time_Minute".Translate();
            }

            return minutes.ToString("0.0") + " " + "PS_Time_Minutes".Translate();
        }
    }
}