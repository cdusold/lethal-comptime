using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using LethalCompTime.Patches;
using HarmonyLib;

namespace LethalCompTime
{
    [BepInPlugin("com.github.cdusold.LethalCompTime", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("cdusold.LethalCompTime");

        private static Plugin Instance;

        public static ConfigEntry<int> configRolloverFraction;
        public static ConfigEntry<int> configRolloverThreshold;
        public static ConfigEntry<int> configRolloverPenalty;

        public enum ColorOptions
        {
            None = 0,
            Text = 1,
            Screen = 2,
            // Custom = 3
        };

        public static ConfigEntry<ColorOptions> configScreenColoration;

        public static ManualLogSource logger;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            logger = Logger;
            Logger.LogInfo("Mod cdusold.LethalCompTime is loaded!");
            configRolloverFraction = Config.Bind(
                "General",
                "RolloverBasePercent",
                50,
                "The starting percentage of your excess quota that automatically carries over to the next cycle."
            );
            configRolloverThreshold = Config.Bind(
                "General",
                "RolloverPenaltyThreshold",
                100,
                "The point at which diminishing returns take effect for rollover amounts. Each multiple of the threshold exceeded reduces the rollover of the surplus over that threshold."
            );
            configRolloverPenalty = Config.Bind(
                "General",
                "RolloverPenaltyPercent",
                50,
                "The percentage deduction applied to your rollover amount for each threshold exceeded. This creates a gradual decline in rollover benefits for larger surpluses."
            );
            configScreenColoration = Config.Bind(
                "Visual",
                "RolloverScreenColoration",
                ColorOptions.Screen,
                "The type of coloration to use. None, Text (only), Screen (background)."//, Custom."
            );
            if (configRolloverFraction.Value != 0.0F)
                Logger.LogInfo($"Rollover percentage set to {configRolloverFraction.Value}");
            if (configRolloverThreshold.Value != 0.0F)
                Logger.LogInfo($"Rollover max percentage set to {configRolloverThreshold.Value}");
            if (configRolloverPenalty.Value != 0.0F)
                Logger.LogInfo($"Rollover max percentage set to {configRolloverPenalty.Value}");
            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(TimeOfDayPatch));
        }
    }
}
