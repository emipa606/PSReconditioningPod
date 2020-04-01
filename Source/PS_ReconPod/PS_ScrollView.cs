using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace PS_ReconPod
{
    public class PS_ScrollView<T>
    {
        public class ScrollOption<O>
        {
            public int Index;
            public string Label;
            public string ToolTip;
            public O Value;
            public Action ButtonAction;
            public string ButtonText;
            public bool HasButton = true;
        }

        private float Padding;
        private float OptionHeight;
        private float OptionSpacing;
        public Rect DrawRect;
        private Vector2 ScrollPos;
        private List<ScrollOption<T>> Options;

        public Func<ScrollOption<T>, string> ToolTipFunc;
        
        public PS_ScrollView(Rect drawRect, float Padding = 0.5f, float OptionHeight = 30f, float OptionSpacing = 5f)
        {
            this.Padding = Padding;
            this.DrawRect = drawRect;
            this.ToolTipFunc = ((ScrollOption<T> t) => t.ToolTip);
            this.OptionHeight = OptionHeight;
            this.OptionSpacing = OptionSpacing;
        }

        public bool TrySetOptions(List<ScrollOption<T>> newOptions)
        {
            this.Options = newOptions;
            return true;
        }

        public void Draw()
        {
            Text.Font = GameFont.Small;
            this.OptionHeight = Text.CalcHeight("Test", 500f);
            
            var trueDrawRect = this.DrawRect;// new Rect(this.DrawRect.x + Padding, this.DrawRect.y + Padding, this.DrawRect.width - (this.Padding * 2f), this.DrawRect.height - (this.Padding * 2f));
            
            var optionsRect = new Rect(0, 0, trueDrawRect.width - 16f, this.Options.Count * (this.OptionHeight + this.OptionSpacing));
            Widgets.BeginScrollView(trueDrawRect, ref this.ScrollPos, optionsRect);
            
            if (this.Options == null)
            {
                Log.Error("PS_ReconPod.PS_ScrollView: Tired to draw scroll view with no options");
                return;
            }
            
            var tempRect = new Rect(0, 0, optionsRect.width, this.OptionHeight + this.OptionSpacing);
            foreach (var opt in this.Options)
            {
                Widgets.Label(tempRect, opt.Label);
                TooltipHandler.TipRegion(tempRect, this.ToolTipFunc(opt));
                if (opt.HasButton && Widgets.ButtonText(new Rect(tempRect.x + tempRect.width - 60f, tempRect.y, 60f, tempRect.height), opt.ButtonText) && opt.ButtonAction != null)
                    opt.ButtonAction();
                tempRect.y += this.OptionHeight + this.OptionSpacing;
            }
            
            Widgets.EndScrollView();
        }
    }
}
