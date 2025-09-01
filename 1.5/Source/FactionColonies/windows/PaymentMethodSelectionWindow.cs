using RimWorld;
using UnityEngine;
using Verse;
using VanillaTradingExpanded;

namespace FactionColonies
{
    public class PaymentMethodSelectionWindow : Window
    {
        private readonly SettlementFC settlement;
        private PaymentMethod selectedMethod;

        public override Vector2 InitialSize => new Vector2(400f, 300f);

        public PaymentMethodSelectionWindow(SettlementFC settlement)
        {
            this.settlement = settlement;
            this.selectedMethod = settlement.preferredPaymentMethod;
            this.forcePause = true;
            this.doCloseX = true;
            this.absorbInputAroundWindow = true;
            this.closeOnAccept = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 40f), "Select Payment Method");
            Text.Font = GameFont.Small;

            float curY = 50f;
            bool isOrbital = settlement?.worldSettlement?.def?.defName == "FCOrbitalPlatform";

            if (!isOrbital)
            {
                if (Widgets.RadioButtonLabeled(new Rect(10f, curY, inRect.width - 20f, 30f), "Caravan Delivery", selectedMethod == PaymentMethod.Caravan))
                {
                    selectedMethod = PaymentMethod.Caravan;
                }
                curY += 40f;
            }

            if (DefDatabase<ResearchProjectDef>.GetNamed("TransportPod").IsFinished)
            {
                if (Widgets.RadioButtonLabeled(new Rect(10f, curY, inRect.width - 20f, 30f), "Drop Pod Delivery", selectedMethod == PaymentMethod.DropPod))
                {
                    selectedMethod = PaymentMethod.DropPod;
                }
                curY += 40f;
            }

            if (ModLister.HasActiveModWithName("Vanilla Trading Expanded") && DefDatabase<ResearchProjectDef>.GetNamed("EmpireBanking").IsFinished)
            {
                if (Widgets.RadioButtonLabeled(new Rect(10f, curY, inRect.width - 20f, 30f), "Bank Transfer", selectedMethod == PaymentMethod.Bank))
                {
                    selectedMethod = PaymentMethod.Bank;
                }
                curY += 40f;

                if (selectedMethod == PaymentMethod.Bank)
                {
                    float bankTaxRate = DefDatabase<ResearchProjectDef>.GetNamed("EmpireBankingEfficiency").IsFinished ? 0.25f : 0.5f;
                    GUI.color = Color.gray;
                    Widgets.Label(new Rect(30f, curY, inRect.width - 40f, 60f), 
                        $"Bank transfers will incur a {bankTaxRate.ToStringPercent()} transaction fee. Research Banking Efficiency to reduce this to 25%.");
                    GUI.color = Color.white;
                    curY += 70f;
                }
            }

            if (Widgets.ButtonText(new Rect(inRect.width - 120f, inRect.height - 40f, 100f, 35f), "Accept"))
            {
                settlement.preferredPaymentMethod = selectedMethod;
                Close();
            }
        }
    }
}
