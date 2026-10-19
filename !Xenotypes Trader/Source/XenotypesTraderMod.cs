using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static XenotypesTrader.XenotypeFilterUtil;

namespace XenotypesTrader
{
    public class XenotypesTraderMod : Mod
    {
        public static XenotypesTraderSettings Settings;

        private Vector2 scrollPosition;
        private float contentHeight;
        private string searchText = "";

        public XenotypesTraderMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<XenotypesTraderSettings>();
        }

        public override string SettingsCategory()
        {
            return "XenotypesTrader_SettingsCategory".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect outRect = new Rect(inRect.x, inRect.y, inRect.width, inRect.height - 40f);
            Rect resetRect = new Rect(inRect.x, inRect.yMax - 30f, 200f, 30f);
            float viewWidth = outRect.width - 16f;

            Widgets.BeginScrollView(
                outRect,
                ref scrollPosition,
                new Rect(0f, 0f, viewWidth, Mathf.Max(contentHeight, outRect.height)));

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(new Rect(0f, 0f, viewWidth - 8f, 99999f));

            listing.Label("勾选的异种人会出现在商船里；取消勾选后该异种人不再被出售。");
            listing.Gap(4f);

            // 搜索框
            Rect searchRect = listing.GetRect(28f);
            string newSearch = Widgets.TextField(searchRect, searchText ?? "");
            if (newSearch != searchText)
            {
                searchText = newSearch;
                scrollPosition = Vector2.zero;
            }
            listing.Gap(4f);

            // 全开 / 全关
            Rect toggleRow = listing.GetRect(28f);
            float half = (toggleRow.width - 6f) / 2f;
            Rect onRect = new Rect(toggleRow.x, toggleRow.y, half, 28f);
            Rect offRect = new Rect(toggleRow.x + half + 6f, toggleRow.y, half, 28f);
            if (Widgets.ButtonText(onRect, "全部启用"))
            {
                Settings.disabledXenotypes.Clear();
            }
            if (Widgets.ButtonText(offRect, "全部禁用"))
            {
                Settings.disabledXenotypes.Clear();
                foreach (XenotypeDef def in DefDatabase<XenotypeDef>.AllDefsListForReading)
                {
                    if (def != null) Settings.disabledXenotypes.Add(def.defName);
                }
            }
            listing.Gap(6f);

            // 逐个异种人一行
            List<XenotypeDef> list = FilteredXenotypes(searchText).ToList();
            if (list.Count == 0)
            {
                listing.Label("(没有匹配的异种人)");
            }
            else
            {
                foreach (XenotypeDef def in list)
                {
                    DrawXenotypeRow(listing, def);
                }
            }

            contentHeight = listing.CurHeight;
            listing.End();
            Widgets.EndScrollView();

            if (Widgets.ButtonText(resetRect, "ResetButton".Translate()))
            {
                Settings.disabledXenotypes.Clear();
            }

            WriteSettings();
        }

        private static IEnumerable<XenotypeDef> FilteredXenotypes(string search)
        {
            bool excludeHAR = Settings?.excludeHARXenotypes ?? true;
            return XenotypeFilterUtil.FilterForSettings(search, excludeHAR);
        }

        private static void DrawXenotypeRow(Listing_Standard listing, XenotypeDef def)
        {
            Rect row = listing.GetRect(28f);
            Widgets.DrawHighlightIfMouseover(row);

            Rect iconRect = new Rect(row.x + 4f, row.y + 2f, 24f, 24f);
            Widgets.DefIcon(iconRect, def);

            Rect cbRect = new Rect(row.xMax - 28f, row.y + 2f, 24f, 24f);
            Rect labelRect = new Rect(
                iconRect.xMax + 6f,
                row.y,
                cbRect.x - iconRect.xMax - 12f,
                row.height);

            bool enabled = Settings.IsEnabled(def.defName);
            bool checkOn = enabled;
            Widgets.Checkbox(cbRect.position, ref checkOn);
            if (checkOn != enabled)
            {
                Settings.SetEnabled(def.defName, checkOn);
            }

            string label = def.LabelCap.ToString();

            if (enabled)
            {
                Widgets.Label(labelRect, label);
            }
            else
            {
                Widgets.Label(labelRect, label.Colorize(Color.gray));
            }
        }
    }
}