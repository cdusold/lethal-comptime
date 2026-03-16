#define LETHALCOMPTIME_PRESERVE_VANILLA_QUOTA_LOGIC

using HarmonyLib;
using LethalCompTime.Configs;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
namespace LethalCompTime.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {
        public static int currentOverfulfillment = 0;
        public static int rolloverResult = 0;

        public static int CalculateQuotaRollover(int quotaFulfilled, int profitQuota)
        {
            bool debug = ConfigManager.EnableDebug.Value;
            double value = quotaFulfilled;
            if (debug)
                Plugin.logger.LogInfo($"Calc: {quotaFulfilled}/{profitQuota}");
            double threshold = ConfigManager.RolloverThreshold.Value / 100.0;
            double penalty = ConfigManager.RolloverPenalty.Value / 100.0;
            double fraction = ConfigManager.RolloverFraction.Value / 100.0;
            if (debug)
                Plugin.logger.LogInfo($"Thresh: {threshold} penalty: {penalty} fraction: {fraction}");
            bool log = ConfigManager.PenaltyUsed.Value == ConfigManager.PenaltyType.Logarithmic;
            if (log)
            {
                value *= fraction;
            }
            double quota_threshold = threshold * profitQuota;
            if (quota_threshold > 0 && penalty != 1 && value > quota_threshold)
            {
                int thresholds_passed;
                if (log)
                {
                    double invPenalty = 1.0 / penalty;
                    double temp = quota_threshold - value * (1.0 - invPenalty);
                    if (debug)
                        Plugin.logger.LogInfo($"Math.Log({temp}/{quota_threshold}, {invPenalty})");
                    thresholds_passed = (int)Math.Floor(Math.Log(temp / quota_threshold, invPenalty));
                }
                else
                {
                    thresholds_passed = (int)Math.Floor(value / quota_threshold);
                }
                if (debug)
                    Plugin.logger.LogInfo($"Calc: #{thresholds_passed}");
                double highest_fraction = Math.Pow(penalty, thresholds_passed);
                if (debug)
                {
                    Plugin.logger.LogInfo($"Calc: %{highest_fraction}");
                }
                double remainder;
                if (log)
                {
                    remainder = value - (quota_threshold * (1 - (1 / highest_fraction)) / (1 - (1 / penalty)));
                    if (debug)
                        Plugin.logger.LogInfo($"Calc: {quota_threshold * (1 - (1 / highest_fraction)) / (1 - (1 / penalty))} < {value} < {quota_threshold * (1 - (1 / (highest_fraction * penalty))) / (1 - 1 / penalty)}");
                }
                else
                {
                    remainder = value - (thresholds_passed * quota_threshold);
                }
                if (debug)
                    Plugin.logger.LogInfo($"Calc: r{remainder}");
                value = highest_fraction * remainder;
                if (log)
                {
                    value += quota_threshold * thresholds_passed;
                }
                else
                {
                    value += quota_threshold * (1 - highest_fraction) / (1 - penalty);
                }
            }
            if (!log)
            {
                value *= fraction;
            }
            if (debug)
                Plugin.logger.LogInfo($"Calc: v{value}");
            return (int)Math.Floor(value);
        }

        #if LETHALCOMPTIME_PRESERVE_VANILLA_QUOTA_LOGIC
        private static int CalculateOvertimeBonus(int quotaOverfulfilled, int daysUntilDeadline)
        {
            int overtimeBonus = quotaOverfulfilled / 5 + 15 * daysUntilDeadline;
            if (ConfigManager.EnableDebug.Value)
                Plugin.logger.LogInfo($"Overtime: q{quotaOverfulfilled}  d{daysUntilDeadline} o{overtimeBonus}");
            return overtimeBonus;
        }

        #endif
        [HarmonyPatch("SetNewProfitQuota")]
        [HarmonyPrefix]
        [HarmonyAfter([])]
        private static bool GetQuotaFulfilledHost(ref int ___quotaFulfilled, ref int ___profitQuota, ref int ___timesFulfilledQuota,
                                                  ref QuotaSettings ___quotaVariables, ref float ___timeUntilDeadline, ref float ___totalTime,
                                                  ref int ___daysUntilDeadline)
        {
            Plugin.logger.LogInfo($"days: {TimeOfDay.Instance.daysUntilDeadline} time: {TimeOfDay.Instance.timeUntilDeadline} ID: {StartOfRound.Instance.currentLevelID}");
            currentOverfulfillment = 0;
            if (TimeOfDay.Instance.daysUntilDeadline <= 0 || (ConfigManager.EarlyFulfillment.Value && TimeOfDay.Instance.daysUntilDeadline <= ConfigManager.QuotaDaysPerCycle.Value - ConfigManager.EarlyFulfillStartDay.Value))
            {
                currentOverfulfillment = ___quotaFulfilled - ___profitQuota;
                if (ConfigManager.EnableDebug.Value)
                {
                    Plugin.logger.LogInfo($"Host: Current overfulfillment: {currentOverfulfillment}");
                    Plugin.logger.LogInfo($"Host: Required {___profitQuota}");
                    Plugin.logger.LogInfo($"Host: Got {___quotaFulfilled}");
                }
                #if LETHALCOMPTIME_PRESERVE_VANILLA_QUOTA_LOGIC
                if (ConfigManager.OvertimeOverride.Value)
                {
                    ___quotaFulfilled = ___profitQuota;
                }
                #else
                if (TimeOfDay.Instance.IsServer)
                {
                    // DIRECTLY COPIED FROM DISASSEMBLY OF V70. WITH APPROPRIATE MODIFICATION. WILL NEED MANUAL UPDATES.
                    ___timesFulfilledQuota++;
                    int num = ___quotaFulfilled - ___profitQuota;
                    float num2 = Mathf.Clamp(1f + (float)___timesFulfilledQuota * ((float)___timesFulfilledQuota / ___quotaVariables.increaseSteepness), 0f, 10000f);
                    TimeOfDay.Instance.CalculateLuckValue();
                    float num3 = UnityEngine.Random.Range(0f, 1f);
                    Debug.Log($"Randomizer amount before: {num3}");
                    num3 *= Mathf.Abs(TimeOfDay.Instance.luckValue - 1f);
                    Debug.Log($"Randomizer amount after: {num3}");
                    num2 = ___quotaVariables.baseIncrease * num2 * (___quotaVariables.randomizerCurve.Evaluate(num3) * ___quotaVariables.randomizerMultiplier + 1f);
                    Debug.Log($"Amount to increase quota:{num2}");
                    ___profitQuota = (int)Mathf.Clamp((float)___profitQuota + num2, 0f, 1E+09f);
                    ___quotaFulfilled = CalculateQuotaRollover(currentOverfulfillment, ___profitQuota); // THIS IS MODIFIED.
                    if (ConfigManager.OvertimeOverride.Value)    // THIS IS ADDED.
                        num -= Math.Min(___quotaFulfilled, num); // THIS IS ADDED.
                    ___timeUntilDeadline = ___totalTime * 4f;
                    int overtimeBonus = num / 5 + 15 * ___daysUntilDeadline;
                    TimeOfDay.Instance.furniturePlacedAtQuotaStart.Clear();
                    AutoParentToShip[] array = UnityEngine.Object.FindObjectsByType<AutoParentToShip>(FindObjectsSortMode.None);
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i].unlockableID != -1 && StartOfRound.Instance.unlockablesList.unlockables[array[i].unlockableID].spawnPrefab)
                        {
                            TimeOfDay.Instance.furniturePlacedAtQuotaStart.Add(array[i].unlockableID);
                        }
                    }

                    TimeOfDay.Instance.hasShownAdThisQuota = false;
                    TimeOfDay.Instance.SyncNewProfitQuotaClientRpc(___profitQuota, overtimeBonus, ___timesFulfilledQuota);
                    return false; // Prevents original function call.
                }
                #endif
                return true; // I dunno. If someone else is hooking in, let them have it.
            }
            return false; // Prevents early fulfillment.
        }

        [HarmonyPatch("SyncNewProfitQuotaClientRpc")]
        [HarmonyPrefix]
        private static void GetNewQuotaFulfilledClient(ref int ___quotaFulfilled, ref int ___profitQuota, ref int newProfitQuota, ref int overtimeBonus)
        {
            var debug = ConfigManager.EnableDebug.Value;
            if (debug)
                Plugin.logger.LogInfo($"ClientRpc: Next quota {newProfitQuota}");
            if (currentOverfulfillment == 0)
            {
                currentOverfulfillment = ___quotaFulfilled - ___profitQuota;
                if (debug)
                {
                    Plugin.logger.LogInfo($"ClientRpc: Required {___profitQuota}");
                    Plugin.logger.LogInfo($"ClientRpc: Got {___quotaFulfilled}");
                }
            }
            rolloverResult = CalculateQuotaRollover(currentOverfulfillment, newProfitQuota);
            #if LETHALCOMPTIME_PRESERVE_VANILLA_QUOTA_LOGIC
            if (ConfigManager.OvertimeOverride.Value)
            {
                overtimeBonus = CalculateOvertimeBonus(currentOverfulfillment - rolloverResult, TimeOfDay.Instance.daysUntilDeadline);
            }
            #endif
            if (debug)
                Plugin.logger.LogInfo($"ClientRpc: Overtime bonus {overtimeBonus}");
        }

        [HarmonyPatch("SyncNewProfitQuotaClientRpc")]
        [HarmonyPostfix]
        private static void SetNewQuotaFulfiledClient(ref int ___quotaFulfilled, ref int ___profitQuota)
        {
            if (___quotaFulfilled == 0 && ConfigManager.RolloverFraction.Value > 0 && currentOverfulfillment > 0)
            {
                ___quotaFulfilled = rolloverResult;
                Plugin.logger.LogInfo($"New quota completion at: {___quotaFulfilled}");
            }
            currentOverfulfillment = 0;
        }

        [HarmonyPatch("UpdateProfitQuotaCurrentTime")]
        [HarmonyPostfix]
        public static void SetUpdateProfitQuotaCurrentTime(ref int ___quotaFulfilled, ref int ___profitQuota)
        {
            try
            {
                if (!StartOfRound.Instance.isChallengeFile)
                {
                    if (ConfigManager.ScreenColoration.Value == ConfigManager.ColorOptions.None)
                        return;
                    Color text_color;
                    if (ConfigManager.ScreenColoration.Value == ConfigManager.ColorOptions.Text)
                    {
                        if (___quotaFulfilled < ___profitQuota)
                        {
                            // screen_color = new Color(1f, 0f, 0f, 1f);
                            ColorUtility.TryParseHtmlString(ConfigManager.ColorUnderFulfilled.Value, out text_color);
                        }
                        else if (ConfigManager.RolloverFraction.Value <= 0 || ConfigManager.RolloverPenalty.Value <= 0 || ___quotaFulfilled < ___profitQuota * (((float)ConfigManager.RolloverThreshold.Value) / 100 + 1))
                        {
                            // screen_color = new Color(0f, 1f, 0f, 1f);
                            ColorUtility.TryParseHtmlString(ConfigManager.ColorFulfilled.Value, out text_color);
                        }
                        else
                        {
                            // screen_color = new Color(1f, 1f, 0f, 1f);
                            ColorUtility.TryParseHtmlString(ConfigManager.ColorOverFulfilled.Value, out text_color);
                        }
                    }
                    else //if (ConfigManager.ScreenColoration.Value == Plugin.ColorOptions.Screen)
                    {
                        Color screen_color;
                        if (___quotaFulfilled == 0)
                        {
                            screen_color = new Color(0f, 0f, 0f, 1f);
                            text_color = new Color(0f, 1f, 0f, 1f);
                        }
                        else if (___quotaFulfilled < ___profitQuota)
                        {
                            ColorUtility.TryParseHtmlString(ConfigManager.ColorUnderFulfilled.Value, out screen_color);
                            ColorUtility.TryParseHtmlString(ConfigManager.TextColorOverrideUnderFulfilled.Value, out text_color);
                        }
                        else if (ConfigManager.RolloverFraction.Value <= 0 || ConfigManager.RolloverPenalty.Value <= 0 || ___quotaFulfilled < ___profitQuota * (((float)ConfigManager.RolloverThreshold.Value) / 100 + 1))
                        {
                            ColorUtility.TryParseHtmlString(ConfigManager.ColorFulfilled.Value, out screen_color);
                            ColorUtility.TryParseHtmlString(ConfigManager.TextColorOverrideFulfilled.Value, out text_color);
                        }
                        else
                        {
                            ColorUtility.TryParseHtmlString(ConfigManager.ColorOverFulfilled.Value, out screen_color);
                            ColorUtility.TryParseHtmlString(ConfigManager.TextColorOverrideOverFulfilled.Value, out text_color);
                        }
                        StartOfRound.Instance.deadlineMonitorBGImage.color = screen_color;
                        StartOfRound.Instance.profitQuotaMonitorBGImage.color = screen_color;
                    }
                    StartOfRound.Instance.profitQuotaMonitorText.color = text_color;
                    StartOfRound.Instance.deadlineMonitorText.color = text_color;
                }
            }
            catch (Exception e)
            {
                Plugin.logger.LogError("Error in monitor update");
                Plugin.logger.LogError(e.ToString());
            }
        }

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        private static void SetQuotaVariables(ref QuotaSettings ___quotaVariables)
        {
            if (ConfigManager.ConfigQuota.Value)
            {
                if (ConfigManager.EnableDebug.Value)
                {
                    Plugin.logger.LogInfo($"Quota variables:");
                    Plugin.logger.LogInfo($"daysPerQuota: {ConfigManager.QuotaDaysPerCycle.Value}");
                    Plugin.logger.LogInfo($"earlyFulfillStartDay: {ConfigManager.EarlyFulfillStartDay.Value}");
                    Plugin.logger.LogInfo($"startingQuota: {ConfigManager.QuotaStartingQuota.Value}");
                    Plugin.logger.LogInfo($"startingCredits: {ConfigManager.QuotaStartingCredits.Value}");
                    Plugin.logger.LogInfo($"baseIncrease: {ConfigManager.QuotaBaseIncrease.Value}");
                    Plugin.logger.LogInfo($"randomizerPortion: {ConfigManager.QuotaRandomizerPortion.Value}");
                    Plugin.logger.LogInfo($"inverseSteepness: {ConfigManager.QuotaInverseSteepness.Value}");
                    Plugin.logger.LogInfo($"In game values:");
                    Plugin.logger.LogInfo($"daysPerQuota: {___quotaVariables.deadlineDaysAmount}");
                    Plugin.logger.LogInfo($"startingQuota: {___quotaVariables.startingQuota}");
                    Plugin.logger.LogInfo($"startingCredits: {___quotaVariables.startingCredits}");
                    Plugin.logger.LogInfo($"baseIncrease: {___quotaVariables.baseIncrease}");
                    Plugin.logger.LogInfo($"randomizerPortion: {___quotaVariables.randomizerMultiplier}");
                    Plugin.logger.LogInfo($"inverseSteepness: {___quotaVariables.increaseSteepness}");
                }
                UpdateQuotaVariables();
                if (ConfigManager.EnableDebug.Value)
                {
                    Plugin.logger.LogInfo("Set quota variables.");
                }
            }
        }

        public static void UpdateQuotaVariables()
        {
            if  (Plugin.QuotaCompat)
                return;
            var timeOfDay = TimeOfDay.Instance;
            var quotaVariables = Traverse.Create(timeOfDay).Field("quotaVariables").GetValue<QuotaSettings>();
            if (ConfigManager.ConfigQuota.Value)
            {
                if (ConfigManager.QuotaDaysPerCycle.Value != quotaVariables.deadlineDaysAmount)
                {
                    Plugin.logger.LogInfo($"Updating daysPerQuota from {quotaVariables.deadlineDaysAmount} to {ConfigManager.QuotaDaysPerCycle.Value}");
                    quotaVariables.deadlineDaysAmount = ConfigManager.QuotaDaysPerCycle.Value;
                }
                if (ConfigManager.QuotaStartingQuota.Value != quotaVariables.startingQuota)
                {
                    Plugin.logger.LogInfo($"Updating startingQuota from {quotaVariables.startingQuota} to {ConfigManager.QuotaStartingQuota.Value}");
                    quotaVariables.startingQuota = ConfigManager.QuotaStartingQuota.Value;
                }
                if (ConfigManager.QuotaStartingCredits.Value != quotaVariables.startingCredits)
                {
                    Plugin.logger.LogInfo($"Updating startingCredits from {quotaVariables.startingCredits} to {ConfigManager.QuotaStartingCredits.Value}");
                    quotaVariables.startingCredits = ConfigManager.QuotaStartingCredits.Value;
                }
                if (ConfigManager.QuotaBaseIncrease.Value != quotaVariables.baseIncrease)
                {
                    Plugin.logger.LogInfo($"Updating baseIncrease from {quotaVariables.baseIncrease} to {ConfigManager.QuotaBaseIncrease.Value}");
                    quotaVariables.baseIncrease = ConfigManager.QuotaBaseIncrease.Value;
                }
                if (ConfigManager.QuotaRandomizerPortion.Value != quotaVariables.randomizerMultiplier)
                {
                    Plugin.logger.LogInfo($"Updating randomizerPortion from {quotaVariables.randomizerMultiplier} to {ConfigManager.QuotaRandomizerPortion.Value}");
                    quotaVariables.randomizerMultiplier = ConfigManager.QuotaRandomizerPortion.Value;
                }
                if (ConfigManager.QuotaInverseSteepness.Value != quotaVariables.increaseSteepness)
                {
                    Plugin.logger.LogInfo($"Updating randomizerPortion from {quotaVariables.increaseSteepness} to {ConfigManager.QuotaInverseSteepness.Value}");
                    quotaVariables.increaseSteepness = ConfigManager.QuotaInverseSteepness.Value;
                }
                if (ConfigManager.EnableDebug.Value)
                {
                    Plugin.logger.LogInfo("Updated quota variables.");
                }
            }
            else
            { // TODO: Double check these variables
                quotaVariables.deadlineDaysAmount = 3;
                quotaVariables.startingQuota = 130;
                quotaVariables.startingCredits = 60;
                quotaVariables.baseIncrease = 100;
                quotaVariables.randomizerMultiplier = 1f;
                quotaVariables.increaseSteepness = 16f;
                if (ConfigManager.EnableDebug.Value)
                {
                    Plugin.logger.LogInfo("Reset quota variables.");
                }
            }
        }
    }
}