using BepInEx;
using BepInEx.Unity;
using HarmonyLib;

namespace Orbblade
{
    [BepInPlugin("raimumaker.elin.orbblade", "Orbblade", "0.1.0")]
    public sealed class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            new Harmony("raimumaker.elin.orbblade").PatchAll();
            Logger.LogInfo("Orbblade 0.1.0 loaded: fire orb, mastery and Flame Step.");
        }
    }
}
