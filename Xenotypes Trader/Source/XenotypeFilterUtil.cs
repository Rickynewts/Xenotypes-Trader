using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace XenotypesTrader
{
    /// <summary>
    /// 过滤调试。
    /// </summary>
    public static class XenotypeFilterUtil
    {
        public const string HARPackageId = "erdelf.HumanoidAlienRaces";

        /// <summary>
        /// 是否是 HAR 从 AlienRace 动态转换出来的 XenotypeDef。
        /// modContentPack.PackageId 判断
        /// </summary>
        public static bool IsHARGeneratedXenotype(XenotypeDef def)
        {
            return def?.modContentPack?.PackageId == HARPackageId;
        }

        /// <summary>
        /// 会输出详细日志
        /// </summary>
        public static List<XenotypeDef> BuildTraderPool(XenotypesTraderSettings settings)
        {
            List<XenotypeDef> all = DefDatabase<XenotypeDef>.AllDefsListForReading
                .Where(x => x != null)
                .ToList();

            bool excludeHAR = settings?.excludeHARXenotypes ?? true;
            Log.Message($"[XenotypesTrader] 全量异种人 = {all.Count}，" +
                        $"排除HAR = {excludeHAR}，" +
                        $"玩家禁用数 = {settings?.disabledXenotypes?.Count ?? 0}");

            int harCount = 0;
            int disabledCount = 0;
            List<XenotypeDef> pool = new List<XenotypeDef>();

            foreach (XenotypeDef def in all)
            {
                if (excludeHAR && IsHARGeneratedXenotype(def))
                {
                    harCount++;
                    Log.Message($"[XenotypesTrader] 排除（HAR自动生成）：{def.defName}（{def.label}），" +
                                $"来源 = {def.modContentPack?.PackageId}");
                    continue;
                }

                if (settings != null && !settings.IsEnabled(def.defName))
                {
                    disabledCount++;
                    Log.Message($"[XenotypesTrader] 排除（玩家禁用）：{def.defName}（{def.label}）");
                    continue;
                }

                pool.Add(def);
            }

            Log.Message($"[XenotypesTrader] 过滤完成：排除HAR {harCount} 个，排除玩家禁用 {disabledCount} 个，" +
                        $"剩余可用池 = {pool.Count}");
            return pool;
        }
        public static IEnumerable<XenotypeDef> FilterForSettings(string search, bool excludeHAR)
        {
            IEnumerable<XenotypeDef> query = DefDatabase<XenotypeDef>.AllDefsListForReading
                .Where(x => x != null);

            if (excludeHAR)
            {
                query = query.Where(x => !IsHARGeneratedXenotype(x));
            }

            if (!search.NullOrEmpty())
            {
                string s = search.ToLowerInvariant();
                query = query.Where(x =>
                    (x.LabelCap.ToString() ?? "").ToLowerInvariant().Contains(s) ||
                    (x.defName ?? "").ToLowerInvariant().Contains(s));
            }

            return query.OrderBy(x => x.LabelCap.ToString());
        }
    }
}