using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using LethalCompTime.Patches;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace LethalCompTime
{
    [BepInPlugin("com.github.cdusold.LethalCompTime", PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new Harmony("cdusold.LethalCompTime");

        private static Plugin Instance;

        private ConfigEntry<int> configRolloverFraction;
        private ConfigEntry<int> configRolloverThreshold;
        private ConfigEntry<int> configRolloverPenalty;

        public static ManualLogSource logger;

        public static int rollover_fraction;
        public static int rollover_threshold;
        public static int rollover_penalty;

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
            if (configRolloverFraction.Value != 0.0F)
                Logger.LogInfo($"Rollover percentage set to {configRolloverFraction.Value}");
            if (configRolloverThreshold.Value != 0.0F)
                Logger.LogInfo($"Rollover max percentage set to {configRolloverThreshold.Value}");
            if (configRolloverPenalty.Value != 0.0F)
                Logger.LogInfo($"Rollover max percentage set to {configRolloverPenalty.Value}");
            rollover_fraction = configRolloverFraction.Value;
            rollover_threshold = configRolloverThreshold.Value;
            rollover_penalty = configRolloverPenalty.Value;
            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(TimeOfDayPatch));
        }
    }
}
