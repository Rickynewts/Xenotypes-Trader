using System.Collections.Generic;
using Verse;
namespace XenotypesTrader
{
    /// <summary>
    /// 全局 mod 设置。
    /// </summary>
    public class XenotypesTraderSettings : ModSettings
    {
        public HashSet<string> disabledXenotypes = new HashSet<string>();

        public bool IsEnabled(string defName)
        {
            if (defName == null) return true;
            return !disabledXenotypes.Contains(defName);
        }

        public void SetEnabled(string defName, bool enabled)
        {
            if (defName == null) return;
            if (enabled)
            {
                disabledXenotypes.Remove(defName);
            }
            else
            {
                disabledXenotypes.Add(defName);
            }
        }
        /// <summary>
        /// 是否允许HAR种族刷出。
        /// </summary>
        public bool excludeHARXenotypes = true;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref disabledXenotypes, "disabledXenotypes", LookMode.Value);
            Scribe_Values.Look(ref excludeHARXenotypes, "excludeHARXenotypes", true);
            if (Scribe.mode == LoadSaveMode.LoadingVars && disabledXenotypes == null)
                disabledXenotypes = new HashSet<string>();
        }


    }
}