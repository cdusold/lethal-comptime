using HarmonyLib;
using System;
using UnityEngine;
namespace LethalCompTime.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {
        private static int CalculateQuotaRollover(int quotaFulfilled, int profitQuota)
        {
            int value = quotaFulfilled;
            Plugin.logger.LogInfo($"Calc: {quotaFulfilled}/{profitQuota}");
            if (quotaFulfilled > Plugin.configRolloverThreshold.Value * profitQuota / 100)
            {
                int thresholds_passed = 100 * quotaFulfilled / (Plugin.configRolloverThreshold.Value * profitQuota);
                Plugin.logger.LogInfo($"Calc: #{thresholds_passed}");
                float highest_fraction = (float)Math.Pow(((float)Plugin.configRolloverPenalty.Value) / 100, (float)thresholds_passed);
                Plugin.logger.LogInfo($"Calc: %{highest_fraction}");
                int remainder = quotaFulfilled - (thresholds_passed * Plugin.configRolloverThreshold.Value * profitQuota / 100);
                Plugin.logger.LogInfo($"Calc: r{remainder}");
                value = (int)(highest_fraction*remainder + profitQuota*(1 - highest_fraction)/(1 - ((float)Plugin.configRolloverPenalty.Value)/100));
                Plugin.logger.LogInfo($"Calc: v{value}");
            }
            return value * Plugin.configRolloverFraction.Value / 100;
        }
        [HarmonyPatch("SetNewProfitQuota")]
        [HarmonyPrefix]
        [HarmonyAfter([])]
        private static bool GetQuotaFulfilledHost(ref int ___quotaFulfilled, ref int ___profitQuota, out int __state)
        {
            Plugin.logger.LogInfo($"days: {TimeOfDay.Instance.daysUntilDeadline} time: {TimeOfDay.Instance.timeUntilDeadline} ID: {StartOfRound.Instance.currentLevelID}");
            __state = ___quotaFulfilled;
            if (TimeOfDay.Instance.daysUntilDeadline < 0)
            {
                __state = ___quotaFulfilled - ___profitQuota;
                Plugin.logger.LogInfo($"Host: Required {___profitQuota}");
                Plugin.logger.LogInfo($"Host: Got {___quotaFulfilled}");
                return true;
            }
            return false;
        }

        [HarmonyPatch("SetNewProfitQuota")]
        [HarmonyPostfix]
        [HarmonyBefore(new string[] { })]
        private static void SetQuotaFulfilledHost(ref int ___quotaFulfilled, ref int ___profitQuota, int __state)
        {
            if (Plugin.configRolloverFraction.Value > 0 && __state > 0 && TimeOfDay.Instance.daysUntilDeadline < 0)
            {
                ___quotaFulfilled = CalculateQuotaRollover(__state, ___profitQuota);
            }
            Plugin.logger.LogInfo($"Host: New quota completion at: {___quotaFulfilled}");
        }

        [HarmonyPatch("SyncNewProfitQuotaClientRpc")]
        [HarmonyPrefix]
        private static void GetNewQuotaFulfilledClient(ref int ___quotaFulfilled, ref int ___profitQuota, out int __state)
        {
            __state = ___quotaFulfilled - ___profitQuota;
            Plugin.logger.LogInfo($"Client: Required {___profitQuota}");
            Plugin.logger.LogInfo($"Client: Got {___quotaFulfilled}");
        }

        [HarmonyPatch("SyncNewProfitQuotaClientRpc")]
        [HarmonyPostfix]
        private static void SetNewQuotaFulfiledClient(ref int ___quotaFulfilled, ref int ___profitQuota, int __state)
        {
            if (___quotaFulfilled == 0 && Plugin.configRolloverFraction.Value > 0 && __state > 0 && TimeOfDay.Instance.daysUntilDeadline < 0)
            {
                ___quotaFulfilled = CalculateQuotaRollover(__state, ___profitQuota);
                Plugin.logger.LogInfo($"Client: New quota completion at: {__state}");
            }
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
                            text_color = new Color(1f, 0f, 0f, 1f);
                        }
                        else if (Plugin.configRolloverFraction.Value <= 0 || Plugin.configRolloverPenalty.Value <= 0 || ___quotaFulfilled < ___profitQuota * (((float)Plugin.configRolloverThreshold.Value) / 100 + 1))
                        {
                            // screen_color = new Color(0f, 1f, 0f, 1f);
                            text_color = new Color(0f, 1f, 0f, 1f);
                        }
                        else
                        {
                            // screen_color = new Color(1f, 1f, 0f, 1f);
                            text_color = new Color(1f, 1f, 0f, 1f);
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
                            screen_color = new Color(1f, 0f, 0f, 1f);
                            text_color = new Color(0f, 0f, 0f, 1f);
                        }
                        else if (Plugin.configRolloverFraction.Value <= 0 || Plugin.configRolloverPenalty.Value <= 0 || ___quotaFulfilled < ___profitQuota * (((float)Plugin.configRolloverThreshold.Value) / 100 + 1))
                        {
                            screen_color = new Color(0f, 1f, 0f, 1f);
                            text_color = new Color(0f, 0f, 0f, 1f);
                        }
                        else
                        {
                            screen_color = new Color(1f, 1f, 0f, 1f);
                            text_color = new Color(0f, 0f, 0f, 1f);
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
                Plugin.logger.LogError(e.Message);
            }
        }
    }
}