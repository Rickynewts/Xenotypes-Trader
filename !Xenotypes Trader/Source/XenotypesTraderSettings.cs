using System.Collections.Generic;
using Verse;

namespace XenotypesTrader
{
    /// <summary>
    /// 全局 mod 设置。
    /// 只保存一个「被禁用的异种人 defName 集合」。
    /// 集合为空 = 所有异种人都允许在商船里生成奴隶。
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

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref disabledXenotypes, "disabledXenotypes", LookMode.Value);
            if (Scribe.mode == LoadSaveMode.LoadingVars && disabledXenotypes == null)
            {
                disabledXenotypes = new HashSet<string>();
            }
        }
    }
}