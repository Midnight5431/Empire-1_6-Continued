using HarmonyLib;
using RimWorld;
using Verse;
using System.Linq;
using System.Reflection;

namespace FactionColonies
{
    [HarmonyPatch(typeof(ResearchManager), "FinishProject")]
    class ResearchCompleted
    {
        static void Postfix(ResearchProjectDef proj, bool doCompletionDialog = false, Pawn researcher = null)
        {
            FactionFC fc = Find.World.GetComponent<FactionFC>();
            fc.roadBuilder.CheckForTechChanges();
        }
    }

    [HarmonyPatch]
    class Bank_Fees_Patch
    {
        static bool Prepare()
        {
            return ModLister.HasActiveModWithName("Vanilla Trading Expanded");
        }

        static MethodBase TargetMethod()
        {
            var type = GenTypes.GetTypeInAnyAssembly("VanillaTradingExpanded.Bank");
            return type?.GetProperty("Fees")?.GetGetMethod();
        }

        static bool Prefix(ref float __result)
        {
            var fc = Find.World?.GetComponent<FactionFC>();
            if (fc?.factionDef != null)
            {
                bool bankingEfficiencyResearched = DefDatabase<ResearchProjectDef>.GetNamed("EmpireBankingEfficiency", false)?.IsFinished ?? false;
                __result = bankingEfficiencyResearched ? 0.25f : 0.5f;
                return false;
            }
            return true;
        }
    }
}