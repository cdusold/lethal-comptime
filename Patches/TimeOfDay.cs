using HarmonyLib;
using LethalCompTime.Configs;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace LethalCompTime.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {
        public static int CalculateQuotaRollover(int quotaFulfilled, int profitQuota)
        {
            bool debug = false;
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
        [HarmonyPatch("SetNewProfitQuota")]
        [HarmonyPrefix]
        [HarmonyAfter([])]
        private static bool GetQuotaFulfilledHost(ref int ___quotaFulfilled, ref int ___profitQuota, ref int ___timesFulfilledQuota,
                                                  ref QuotaSettings ___quotaVariables, ref float ___timeUntilDeadline, ref float ___totalTime,
                                                  ref int ___daysUntilDeadline)
        {
            Plugin.logger.LogInfo($"days: {TimeOfDay.Instance.daysUntilDeadline} time: {TimeOfDay.Instance.timeUntilDeadline} ID: {StartOfRound.Instance.currentLevelID}");
            Plugin.currentOverfulfillment = 0;
            if (TimeOfDay.Instance.daysUntilDeadline < 0)
            {
                Plugin.currentOverfulfillment = ___quotaFulfilled - ___profitQuota;
                Plugin.logger.LogInfo($"Host: Required {___profitQuota}");
                Plugin.logger.LogInfo($"Host: Got {___quotaFulfilled}");
                if (TimeOfDay.Instance.IsServer)
                {
                    // DIRECTLY COPIED FROM DISASSEMBLY OF V69. WITH APPROPRIATE MODIFICATION. WILL NEED MANUAL UPDATES.
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
                    ___quotaFulfilled = CalculateQuotaRollover(Plugin.currentOverfulfillment, ___profitQuota); // THIS IS MODIFIED.
                    if (ConfigManager.OvertimeOverride.Value)    // THIS IS ADDED.
                        num -= Math.Min(___quotaFulfilled, num); // THIS IS ADDED.
                    ___timeUntilDeadline = ___totalTime * 4f;
                    int overtimeBonus = num / 5 + 15 * ___daysUntilDeadline;
                    TimeOfDay.Instance.furniturePlacedAtQuotaStart.Clear();
                    AutoParentToShip[] array = UnityEngine.Object.FindObjectsByType<AutoParentToShip>(FindObjectsSortMode.None);
                    for (int i = 0; i < array.Length; i++)
                    {
                        if (array[i].unlockableID != -1)
                        {
                            TimeOfDay.Instance.furniturePlacedAtQuotaStart.Add(array[i].unlockableID);
                        }
                    }
                    TimeOfDay.Instance.SyncNewProfitQuotaClientRpc(___profitQuota, overtimeBonus, ___timesFulfilledQuota);
                    return false; // Prevents original function call.
                }
                return true; // I dunno. If someone else is hooking in, let them have it.
            }
            return false; // Prevents early fulfillment.
        }

        [HarmonyPatch("SetNewProfitQuota")]
        [HarmonyPostfix]
        [HarmonyBefore(new string[] { })]
        private static void SetQuotaFulfilledHost(ref int ___quotaFulfilled, ref int ___profitQuota)
        {
            if (ConfigManager.RolloverFraction.Value > 0 && Plugin.currentOverfulfillment > 0 && TimeOfDay.Instance.daysUntilDeadline < 0)
            {
                ___quotaFulfilled = CalculateQuotaRollover(Plugin.currentOverfulfillment, ___profitQuota);
            }
            Plugin.logger.LogInfo($"Host: New quota completion at: {___quotaFulfilled}");
        }

        [HarmonyPatch("SyncNewProfitQuotaClientRpc")]
        [HarmonyPrefix]
        private static void GetNewQuotaFulfilledClient(ref int ___quotaFulfilled, ref int ___profitQuota)
        {
            if (Plugin.currentOverfulfillment == 0)
            {
                Plugin.currentOverfulfillment = ___quotaFulfilled - ___profitQuota;
            }
            Plugin.logger.LogInfo($"Client: Required {___profitQuota}");
            Plugin.logger.LogInfo($"Client: Got {___quotaFulfilled}");
        }

        [HarmonyPatch("SyncNewProfitQuotaClientRpc")]
        [HarmonyPostfix]
        private static void SetNewQuotaFulfiledClient(ref int ___quotaFulfilled, ref int ___profitQuota)
        {
            if (___quotaFulfilled == 0 && ConfigManager.RolloverFraction.Value > 0 && Plugin.currentOverfulfillment > 0 && TimeOfDay.Instance.daysUntilDeadline < 0)
            {
                ___quotaFulfilled = CalculateQuotaRollover(Plugin.currentOverfulfillment, ___profitQuota);
                Plugin.logger.LogInfo($"Client: New quota completion at: {___quotaFulfilled}");
            }
            Plugin.currentOverfulfillment = 0;
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
    }
}