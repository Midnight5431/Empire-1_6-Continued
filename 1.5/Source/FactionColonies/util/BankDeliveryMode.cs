using RimWorld;
using System.Linq;
using Verse;

namespace FactionColonies
{
    public class BankDeliveryMode
    {
        public static bool CanUseBankDelivery(FCEvent evt)
        {
            if (!ModLister.HasActiveModWithName("Vanilla Trading Expanded"))
                return false;

            if (!DefDatabase<ResearchProjectDef>.GetNamed("EmpireBanking", false)?.IsFinished ?? false)
                return false;

            var settlement = Find.World.GetComponent<FactionFC>().settlements.FirstOrFallback(s => s.mapLocation == evt.source);
            if (settlement == null)
                return false;

            return settlement.preferredPaymentMethod == PaymentMethod.Bank;
        }

        public static void SendToBank(FCEvent evt)
        {
            if (!ModLister.HasActiveModWithName("Vanilla Trading Expanded"))
            {
                PaymentUtil.placeThing(evt.goods.FirstOrDefault());
                return;
            }

            // Let any errors bubble up to DeliveryEvent's error handler which will fallback to tax spot
            var tradingManager = Current.Game.GetComponent(GenTypes.GetTypeInAnyAssembly("VanillaTradingExpanded.TradingManager"));
            var bank = tradingManager?.GetType().GetProperty("banksByFaction")?.GetValue(tradingManager)
                ?.GetType().GetProperty("Values")?.GetValue(tradingManager)
                ?.GetType().GetMethod("FirstOrDefault")?.Invoke(null, null);

            if (bank == null)
            {
                PaymentUtil.placeThing(evt.goods.FirstOrDefault());
                return;
            }

            float bankTaxRate = DefDatabase<ResearchProjectDef>.GetNamed("EmpireBankingEfficiency").IsFinished ? 0.25f : 0.5f;
            int totalSilver = 0;

            foreach (Thing thing in evt.goods)
            {
                float marketValue = thing.MarketValue * thing.stackCount;
                totalSilver += (int)marketValue;
                thing.Destroy();
            }

            int afterTaxAmount = (int)(totalSilver * (1f - bankTaxRate));
            bank.GetType().GetProperty("DepositAmount").SetValue(bank, (int)bank.GetType().GetProperty("DepositAmount").GetValue(bank) + afterTaxAmount);

            string str = "TaxesFrom".Translate() + " " +
                Find.World.GetComponent<FactionFC>().getSettlementName(evt.source, evt.planetName) + " " +
                "HaveBeenDeposited".Translate() + "!";

            Messages.Message(str, MessageTypeDefOf.PositiveEvent);
            Find.LetterStack.ReceiveLetter(
                "TaxesHaveBeenDeposited".Translate(),
                str + "\n" + "BankDepositAmount".Translate(afterTaxAmount.ToString(), bankTaxRate.ToStringPercent()),
                LetterDefOf.PositiveEvent);
        }
    }
}