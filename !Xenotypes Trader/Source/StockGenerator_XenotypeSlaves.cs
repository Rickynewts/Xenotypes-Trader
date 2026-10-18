using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace XenotypesTrader
{
    /// <summary>
    /// 库存生成。
    /// </summary>
    public sealed class StockGenerator_XenotypeSlaves : StockGenerator
    {
        public float selectionChance = 0.6f;

        public IntRange slavesPerXenotype = new IntRange(1, 1);

        public int maxSlaves = 12;

        public bool excludeBaseliner = true;

        private const string HARPackageId = "erdelf.HumanoidAlienRaces";

        // 过滤HAR种族
        public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
        {
            XenotypesTraderSettings settings = XenotypesTraderMod.Settings;

            //拉取XenotypeDef
            List<XenotypeDef> pool = XenotypeFilterUtil.BuildTraderPool(settings);
            Log.Message($"[XenotypesTrader] 开始生成异种：可用异种人池 = {pool.Count}");

            if (excludeBaseliner)
            {
                pool.Remove(XenotypeDefOf.Baseliner);
                Log.Message($"[XenotypesTrader] 已排除 Baseliner，剩余池 = {pool.Count}");
            }

            if (pool.Count == 0)
            {
                Log.Message("[XenotypesTrader] 异种人池为空，本次库存不生成任何异种人。");
                yield break;
            }

            pool.Shuffle();

            int targetCount = Mathf.RoundToInt(pool.Count * Mathf.Clamp01(selectionChance));
            if (targetCount < 1) targetCount = 1;

            Log.Message($"[XenotypesTrader] selectionChance = {selectionChance}, " +
                        $"目标异种人数 = {targetCount}, maxSlaves = {maxSlaves}");

            int emitted = 0;

            for (int i = 0; i < targetCount && i < pool.Count; i++)
            {
                XenotypeDef xenotype = pool[i];
                int count = Mathf.Max(1, slavesPerXenotype.RandomInRange);

                Log.Message($"[XenotypesTrader] 选中异种人：{xenotype.defName}（{xenotype.label}），计划生成 {count} 个奴隶");

                for (int j = 0; j < count; j++)
                {
                    if (maxSlaves > 0 && emitted >= maxSlaves)
                    {
                        Log.Message($"[XenotypesTrader] 达到上限 maxSlaves = {maxSlaves}，提前结束。已产出 {emitted} 个奴隶。");
                        yield break;
                    }

                    Pawn slave = GenerateSlave(xenotype, faction, forTile);
                    if (slave != null)
                    {
                        emitted++;
                        Log.Message($"[XenotypesTrader] 生成 #{emitted}：{slave.LabelShort} / xenotype = {xenotype.defName}");
                        yield return slave;
                    }
                    else
                    {
                        Log.Message($"[XenotypesTrader] 异种人 {xenotype.defName} 生成失败，跳过。");
                    }
                }
            }

            Log.Message($"[XenotypesTrader] 本次库存生成结束，共生成 {emitted} 。");
        }

        private static Pawn GenerateSlave(XenotypeDef xenotype, Faction faction, PlanetTile tile)
        {
            try
            {
                PawnGenerationRequest request = new PawnGenerationRequest(
                    kind: PawnKindDefOf.Slave,
                    faction: faction,
                    context: PawnGenerationContext.NonPlayer,
                    tile: tile,
                    forceGenerateNewPawn: true,
                    canGeneratePawnRelations: false,
                    allowFood: false,
                    allowAddictions: false);

                Pawn pawn = PawnGenerator.GeneratePawn(request);
                if (pawn == null)
                {
                    return null;
                }

                if (xenotype != null && pawn.genes != null)
                {
                    pawn.genes.SetXenotype(xenotype);
                }

                StripAllApparel(pawn);

                return pawn;
            }
            catch (Exception ex)
            {
                Log.Error($"[XenotypesTrader] 异种人 {xenotype.defName} 生成失败: {ex}");
                return null;
            }
        }

        private static void StripAllApparel(Pawn pawn)
        {
            if (pawn?.apparel == null)
            {
                return;
            }

            List<Apparel> worn = pawn.apparel.WornApparel?.ToList();
            if (worn == null || worn.Count == 0)
            {
                return;
            }

            foreach (Apparel ap in worn)
            {
                if (ap == null)
                {
                    continue;
                }

                try
                {
                    pawn.apparel.Remove(ap);
                    if (!ap.Destroyed)
                    {
                        ap.Destroy();
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning($"[XenotypesTrader] 移除 {pawn.LabelShort} 衣物 {ap?.def?.defName} 失败: {ex}");
                }
            }
        }


        public override bool HandlesThingDef(ThingDef thingDef)
        {
            return thingDef.race != null && thingDef.race.Humanlike;
        }

        public override IEnumerable<string> ConfigErrors(TraderKindDef parentDef)
        {
            foreach (string error in base.ConfigErrors(parentDef))
            {
                yield return error;
            }

            if (selectionChance < 0f || selectionChance > 1f)
            {
                yield return "selectionChance 必须在 0 到 1 之间";
            }
        }
    }
}