using HarmonyLib;
using Verse;

namespace Limited_Passions
{
    public class Mod : Verse.Mod
    {
        public Mod(ModContentPack content) : base(content)
        {
            LongEventHandler.QueueLongEvent(Init, "LimitedPassions.LoadingLabel", doAsynchronously: true, null);
        }

        private void Init()
        {
            new Harmony("sk.limitedpassions").PatchAll();
        }
    }
}
