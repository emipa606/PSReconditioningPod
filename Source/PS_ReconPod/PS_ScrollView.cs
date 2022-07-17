using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace PS_ReconPod;

public class PS_ScrollView<T>
{
    private readonly float OptionSpacing;

    private readonly float Padding;

    public readonly Func<ScrollOption<T>, string> ToolTipFunc;
    public Rect DrawRect;
    private float OptionHeight;
    private List<ScrollOption<T>> Options;
    private Vector2 ScrollPos;

    public PS_ScrollView(Rect drawRect, float Padding = 0.5f, float OptionHeight = 30f, float OptionSpacing = 5f)
    {
        this.Padding = Padding;
        DrawRect = drawRect;
        ToolTipFunc = t => t.ToolTip;
        this.OptionHeight = OptionHeight;
        this.OptionSpacing = OptionSpacing;
    }

    public bool TrySetOptions(List<ScrollOption<T>> newOptions)
    {
        Options = newOptions;
        return true;
    }

    public void Draw(string filter = null)
    {
        Text.Font = GameFont.Small;
        OptionHeight = Text.CalcHeight("Test", 500f);

        var trueDrawRect = DrawRect;
        // new Rect(this.DrawRect.x + Padding, this.DrawRect.y + Padding, this.DrawRect.width - (this.Padding * 2f), this.DrawRect.height - (this.Padding * 2f));
        if (Options == null)
        {
            Log.Error("PS_ReconPod.PS_ScrollView: Tired to draw scroll view with no options");
            return;
        }

        var options = Options;
        if (!string.IsNullOrEmpty(filter))
        {
            options = Options.Where(option => option.Label.ToLower().Contains(filter.ToLower())).ToList();
        }

        var optionsRect = new Rect(0, 0, trueDrawRect.width - 16f, options.Count * (OptionHeight + OptionSpacing));
        Widgets.BeginScrollView(trueDrawRect, ref ScrollPos, optionsRect);

        var tempRect = new Rect(0, 0, optionsRect.width, OptionHeight + OptionSpacing);


        foreach (var opt in options)
        {
            Widgets.Label(tempRect, opt.Label);
            TooltipHandler.TipRegion(tempRect, ToolTipFunc(opt));
            if (opt.HasButton &&
                Widgets.ButtonText(new Rect(tempRect.x + tempRect.width - 60f, tempRect.y, 60f, tempRect.height),
                    opt.ButtonText) && opt.ButtonAction != null)
            {
                opt.ButtonAction();
            }

            tempRect.y += OptionHeight + OptionSpacing;
        }

        Widgets.EndScrollView();
    }

    public class ScrollOption<O>
    {
        public Action ButtonAction;
        public string ButtonText;
        public bool HasButton = true;
        public int Index;
        public string Label;
        public string ToolTip;
        public O Value;
    }
}