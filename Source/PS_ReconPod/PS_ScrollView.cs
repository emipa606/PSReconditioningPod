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

        private readonly float Padding;
        private float OptionHeight;
        private readonly float OptionSpacing;
        public Rect DrawRect;
        private Vector2 ScrollPos;
        private List<ScrollOption<T>> Options;

        public Func<ScrollOption<T>, string> ToolTipFunc;
        
        public PS_ScrollView(Rect drawRect, float Padding = 0.5f, float OptionHeight = 30f, float OptionSpacing = 5f)
        {
            this.Padding = Padding;
            DrawRect = drawRect;
            ToolTipFunc = (ScrollOption<T> t) => t.ToolTip;
            this.OptionHeight = OptionHeight;
            this.OptionSpacing = OptionSpacing;
        }

        public bool TrySetOptions(List<ScrollOption<T>> newOptions)
        {
            Options = newOptions;
            return true;
        }

        public void Draw()
        {
            Text.Font = GameFont.Small;
            OptionHeight = Text.CalcHeight("Test", 500f);
            
            var trueDrawRect = DrawRect;// new Rect(this.DrawRect.x + Padding, this.DrawRect.y + Padding, this.DrawRect.width - (this.Padding * 2f), this.DrawRect.height - (this.Padding * 2f));
            
            var optionsRect = new Rect(0, 0, trueDrawRect.width - 16f, Options.Count * (OptionHeight + OptionSpacing));
            Widgets.BeginScrollView(trueDrawRect, ref ScrollPos, optionsRect);
            
            if (Options == null)
            {
                Log.Error("PS_ReconPod.PS_ScrollView: Tired to draw scroll view with no options");
                return;
            }
            
            var tempRect = new Rect(0, 0, optionsRect.width, OptionHeight + OptionSpacing);
            foreach (var opt in Options)
            {
                Widgets.Label(tempRect, opt.Label);
                TooltipHandler.TipRegion(tempRect, ToolTipFunc(opt));
                if (opt.HasButton && Widgets.ButtonText(new Rect(tempRect.x + tempRect.width - 60f, tempRect.y, 60f, tempRect.height), opt.ButtonText) && opt.ButtonAction != null)
                {
                    opt.ButtonAction();
                }

                tempRect.y += OptionHeight + OptionSpacing;
            }
            
            Widgets.EndScrollView();
        }
    }
}
