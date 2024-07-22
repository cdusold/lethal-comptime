using HarmonyLib;
using System;
using UnityEngine;
namespace LethalCompTime.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {
        public static int CalculateQuotaRollover(int quotaFulfilled, int profitQuota)
        {
            int value = quotaFulfilled;
            Plugin.logger.LogInfo($"Calc: {quotaFulfilled}/{profitQuota}");
            int threshold = Plugin.configRolloverThreshold.Value;
            int penalty = Plugin.configRolloverPenalty.Value;
            int fraction = Plugin.configRolloverFraction.Value;
            Plugin.logger.LogInfo($"Thresh: {threshold} penalty: {penalty} fraction: {fraction}");
            bool log = Plugin.configPenaltyType.Value == Plugin.PenaltyType.Logarithmic;
            if (log)
            {
                value *= fraction;
                value /= 100;
            }
            if (threshold > 0 && penalty != 100 && value > threshold * profitQuota / 100)
            {
                int thresholds_passed;
                if (log)
                {
                    double temp3 = 100.0 / penalty;
                    double temp2 = threshold * profitQuota / 100.0;
                    double temp = temp2 - value * (1.0 - temp3);
                    Plugin.logger.LogInfo($"Math.Log({temp}/{temp2}, {temp3})");
                    thresholds_passed = (int)Math.Log((temp) / (temp2), temp3);
                }
                else
                {
                    thresholds_passed = 100 * value / (threshold * profitQuota);
                }
                Plugin.logger.LogInfo($"Calc: #{thresholds_passed}");
                float highest_fraction = (float)Math.Pow(((float)penalty) / 100, (float)thresholds_passed);
                Plugin.logger.LogInfo($"Calc: %{highest_fraction}");
                int remainder;
                if (log)
                {
                    remainder = value - (int)(threshold * profitQuota * (1 - (1 / highest_fraction)) / (1 - (100.0 / penalty)))/100;
                }
                else
                {
                    remainder = value - (thresholds_passed * threshold * profitQuota / 100);
                }
                Plugin.logger.LogInfo($"Calc: r{remainder}");
                if (log)
                {
                    value = (int)(highest_fraction * remainder + threshold * profitQuota * thresholds_passed / 100);
                }
                else
                {
                    value = (int)(highest_fraction * remainder + profitQuota * (1 - highest_fraction) / (1 - ((float)penalty) / 100));
                }
                Plugin.logger.LogInfo($"Calc: v{value}");
            }
            if (log)
            {
                return value;
            }
            return value * fraction / 100;
        }
        [HarmonyPatch("SetNewProfitQuota")]
        [HarmonyPrefix]
        [HarmonyAfter([])]
        private static bool GetQuotaFulfilledHost(ref int ___quotaFulfilled, ref int ___profitQuota, ref int ___timesFulfilledQuota,
                                                  ref QuotaSettings ___quotaVariables, ref float ___timeUntilDeadline, ref float ___totalTime, ref int ___daysUntilDeadline)
        {
            Plugin.logger.LogInfo($"days: {TimeOfDay.Instance.daysUntilDeadline} time: {TimeOfDay.Instance.timeUntilDeadline} ID: {StartOfRound.Instance.currentLevelID}");
            Plugin.currentOverfulfillment = 0;
            if (TimeOfDay.Instance.daysUntilDeadline < 0)
            {
                Plugin.currentOverfulfillment = ___quotaFulfilled - ___profitQuota;
                Plugin.logger.LogInfo($"Host: Required {___profitQuota}");
                Plugin.logger.LogInfo($"Host: Got {___quotaFulfilled}");
                if (Plugin.configOvertimeOverride.Value)
                {
                    if (TimeOfDay.Instance.IsServer)
                    {
                        // DIRECTLY COPIED FROM DISASSEMBLY OF V55. WITH APPROPRIATE MODIFICATION. WILL NEED MANUAL UPDATES.
                        ___timesFulfilledQuota++;
                        int num = ___quotaFulfilled - ___profitQuota;
                        float num2 = Mathf.Clamp(1f + (float)___timesFulfilledQuota * ((float)___timesFulfilledQuota / ___quotaVariables.increaseSteepness), 0f, 10000f);
                        num2 = ___quotaVariables.baseIncrease * num2 * (___quotaVariables.randomizerCurve.Evaluate(UnityEngine.Random.Range(0f, 1f)) * ___quotaVariables.randomizerMultiplier + 1f);
                        ___profitQuota = (int)Mathf.Clamp((float)___profitQuota + num2, 0f, 1E+09f);
                        ___quotaFulfilled = CalculateQuotaRollover(Plugin.currentOverfulfillment, ___profitQuota); // THIS IS MODIFIED.
                        num -= Math.Min(___quotaFulfilled, num); // THIS IS ADDED.
                        ___timeUntilDeadline = ___totalTime * 4f;
                        int overtimeBonus = num / 5 + 15 * ___daysUntilDeadline;
                        TimeOfDay.Instance.SyncNewProfitQuotaClientRpc(___profitQuota, overtimeBonus, ___timesFulfilledQuota);
                    }
                    return false; // Prevents original function call.
                }
                return true;
            }
            return false;
        }

        [HarmonyPatch("SetNewProfitQuota")]
        [HarmonyPostfix]
        [HarmonyBefore(new string[] { })]
        private static void SetQuotaFulfilledHost(ref int ___quotaFulfilled, ref int ___profitQuota)
        {
            if (Plugin.configRolloverFraction.Value > 0 && Plugin.currentOverfulfillment > 0 && TimeOfDay.Instance.daysUntilDeadline < 0)
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
            if (___quotaFulfilled == 0 && Plugin.configRolloverFraction.Value > 0 && Plugin.currentOverfulfillment > 0 && TimeOfDay.Instance.daysUntilDeadline < 0)
            {
                ___quotaFulfilled = CalculateQuotaRollover(Plugin.currentOverfulfillment, ___profitQuota);
                Plugin.logger.LogInfo($"Client: New quota completion at: {Plugin.currentOverfulfillment}");
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
                    if (Plugin.configScreenColoration.Value == Plugin.ColorOptions.None)
                        return;
                    Color text_color;
                    if (Plugin.configScreenColoration.Value == Plugin.ColorOptions.Text)
                    {
                        if (___quotaFulfilled < ___profitQuota)
                        {
                            // screen_color = new Color(1f, 0f, 0f, 1f);
                            ColorUtility.TryParseHtmlString(Plugin.configColorUnderFulfilled.Value, out text_color);
                        }
                        else if (Plugin.configRolloverFraction.Value <= 0 || Plugin.configRolloverPenalty.Value <= 0 || ___quotaFulfilled < ___profitQuota * (((float)Plugin.configRolloverThreshold.Value) / 100 + 1))
                        {
                            // screen_color = new Color(0f, 1f, 0f, 1f);
                            ColorUtility.TryParseHtmlString(Plugin.configColorFulfilled.Value, out text_color);
                        }
                        else
                        {
                            // screen_color = new Color(1f, 1f, 0f, 1f);
                            ColorUtility.TryParseHtmlString(Plugin.configColorOverFulfilled.Value, out text_color);
                        }
                    }
                    else //if (Plugin.configScreenColoration.Value == Plugin.ColorOptions.Screen)
                    {
                        Color screen_color;
                        if (___quotaFulfilled == 0)
                        {
                            screen_color = new Color(0f, 0f, 0f, 1f);
                            text_color = new Color(0f, 1f, 0f, 1f);
                        }
                        else if (___quotaFulfilled < ___profitQuota)
                        {
                            ColorUtility.TryParseHtmlString(Plugin.configColorUnderFulfilled.Value, out screen_color);
                            ColorUtility.TryParseHtmlString(Plugin.configTextColorOverrideUnderFulfilled.Value, out text_color);
                        }
                        else if (Plugin.configRolloverFraction.Value <= 0 || Plugin.configRolloverPenalty.Value <= 0 || ___quotaFulfilled < ___profitQuota * (((float)Plugin.configRolloverThreshold.Value) / 100 + 1))
                        {
                            ColorUtility.TryParseHtmlString(Plugin.configColorFulfilled.Value, out screen_color);
                            ColorUtility.TryParseHtmlString(Plugin.configTextColorOverrideFulfilled.Value, out text_color);
                        }
                        else
                        {
                            ColorUtility.TryParseHtmlString(Plugin.configColorOverFulfilled.Value, out screen_color);
                            ColorUtility.TryParseHtmlString(Plugin.configTextColorOverrideOverFulfilled.Value, out text_color);
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