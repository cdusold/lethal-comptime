using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using LethalCompTime.Patches;
using HarmonyLib;
using LobbyCompatibility.Enums;
using LobbyCompatibility.Attributes;
using static BepInEx.BepInDependency;

namespace LethalCompTime
{
    [BepInPlugin("com.github.cdusold.LethalCompTime", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [LobbyCompatibility(CompatibilityLevel.ClientOnly, VersionStrictness.None)]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("cdusold.LethalCompTime");

        private static Plugin Instance;

        public static ConfigEntry<int> configRolloverFraction;
        public static ConfigEntry<int> configRolloverThreshold;
        public static ConfigEntry<int> configRolloverPenalty;

        public enum PenaltyType
        {
            Asymptotic = 0,
            Logarithmic = 1,
        };

        public static ConfigEntry<PenaltyType> configPenaltyType;
        public static ConfigEntry<bool> configOvertimeOverride;

        public enum ColorOptions
        {
            None = 0,
            Text = 1,
            Screen = 2
        };

        public static ConfigEntry<ColorOptions> configScreenColoration;
        public static ConfigEntry<string> configColorOverFulfilled;
        public static ConfigEntry<string> configColorFulfilled;
        public static ConfigEntry<string> configColorUnderFulfilled;
        public static ConfigEntry<string> configTextColorOverrideOverFulfilled;
        public static ConfigEntry<string> configTextColorOverrideFulfilled;
        public static ConfigEntry<string> configTextColorOverrideUnderFulfilled;

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
            configPenaltyType = Config.Bind(
                "General",
                "RolloverPenaltyType",
                PenaltyType.Asymptotic,
                "Whether to use asymptotic or logarithmic rollover scaling."
            );
            configOvertimeOverride = Config.Bind(
                "General",
                "RolloverOvertimeOverride",
                true,
                "Set this to false to allow full overtime bonus even with rollover. (Not Recommended.)"
            );
            configScreenColoration = Config.Bind(
                "Visual",
                "RolloverScreenColoration",
                ColorOptions.None,
                "If and how to display quota status colors. None, Text (only), Screen (background)."
            );
            configColorUnderFulfilled = Config.Bind(
                "Visual",
                "RolloverColorUnderFulfilled",
                "red",
                "The indication color for if the quota isn't met yet."
            );
            configColorFulfilled = Config.Bind(
                "Visual",
                "RolloverColorFulfilled",
                "green",
                "The indication color for if the quota has been met."
            );
            configColorOverFulfilled = Config.Bind(
                "Visual",
                "RolloverColorOverFulfilled",
                "yellow",
                "The indication color for if a penalty will be applied to the rollover."
            );
            configTextColorOverrideUnderFulfilled = Config.Bind(
                "Visual",
                "RolloverTextColorOverrideUnderFulfilled",
                "#000000",
                "The text color in Scene mode for when the quota isn't met yet."
            );
            configTextColorOverrideFulfilled = Config.Bind(
                "Visual",
                "RolloverTextColorOverrideFulfilled",
                "#000000",
                "The text color in Scene mode for when the quota has been met."
            );
            configTextColorOverrideOverFulfilled = Config.Bind(
                "Visual",
                "RolloverTextColorOverrideOverFulfilled",
                "#000000",
                "The text color in Scene mode for when a penalty will be applied to the rollover."
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
