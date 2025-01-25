using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using LethalCompTime.Patches;
using LethalCompTime.Configs;
using LethalCompTime.Compat;
using HarmonyLib;

namespace LethalCompTime
{
    [BepInPlugin("com.github.cdusold.LethalCompTime", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInDependency("BMX.LobbyCompatibility", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("cdusold.LethalCompTime");

        private static Plugin Instance;

        public static ManualLogSource logger;

        public static int currentOverfulfillment = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            logger = Logger;
            Logger.LogInfo("Mod cdusold.LethalCompTime is loaded!");

            ConfigManager.Init(Config);

            if (Chainloader.PluginInfos.ContainsKey("ainavt.lc.lethalconfig"))
            {
                LethalConfigManager.Init(Config);
            }
            if (Chainloader.PluginInfos.ContainsKey("BMX.LobbyCompatibility"))
            {
                LobbyCompatibilityManager.Init();
            }
            if (ConfigManager.RolloverFraction.Value != 0.0F)
                Logger.LogInfo($"Rollover percentage set to {ConfigManager.RolloverFraction.Value}");
            if (ConfigManager.RolloverThreshold.Value != 0.0F)
                Logger.LogInfo($"Rollover max percentage set to {ConfigManager.RolloverThreshold.Value}");
            if (ConfigManager.RolloverPenalty.Value != 0.0F)
                Logger.LogInfo($"Rollover max percentage set to {ConfigManager.RolloverPenalty.Value}");
            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(TimeOfDayPatch));
        }
    }
}
