using BepInEx;
using BepInEx.Unity;

namespace Orbblade
{
    [BepInPlugin("raimumaker.elin.orbblade", "Orbblade", "0.1.0")]
    public sealed class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo("Orbblade foundation loaded. Gameplay integration is pending.");
        }
    }
}
