using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements.UIR;
namespace LethalCompTime.Patches
{
    [HarmonyPatch(typeof(TimeOfDay))]
    internal class TimeOfDayPatch
    {
        private static int CalculateQuotaRollover(int quotaFulfilled, int profitQuota)
        {
            int value = quotaFulfilled;
            Plugin.logger.LogInfo($"Calc: {quotaFulfilled}/{profitQuota}");
            if (quotaFulfilled > Plugin.rollover_threshold * profitQuota / 100)
            {
                int thresholds_passed = 100 * quotaFulfilled / (Plugin.rollover_threshold * profitQuota);
                Plugin.logger.LogInfo($"Calc: #{thresholds_passed}");
                float highest_fraction = (float)Math.Pow(((float)Plugin.rollover_penalty) / 100, (float)thresholds_passed);
                Plugin.logger.LogInfo($"Calc: %{highest_fraction}");
                int remainder = quotaFulfilled - (thresholds_passed * Plugin.rollover_threshold * profitQuota / 100);
                Plugin.logger.LogInfo($"Calc: r{remainder}");
                value = (int)(highest_fraction*remainder + profitQuota*(1 - highest_fraction)/(1 - ((float)Plugin.rollover_penalty)/100));
                Plugin.logger.LogInfo($"Calc: v{value}");
            }
            return value * Plugin.rollover_fraction / 100;
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
            if (Plugin.rollover_fraction > 0 && __state > 0 && TimeOfDay.Instance.daysUntilDeadline < 0)
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
            if (___quotaFulfilled == 0 && Plugin.rollover_fraction > 0 && __state > 0 && TimeOfDay.Instance.daysUntilDeadline < 0)
            {
                ___quotaFulfilled = CalculateQuotaRollover(__state, ___profitQuota);
                Plugin.logger.LogInfo($"Client: New quota completion at: {__state}");
            }
        }

        [HarmonyPatch("UpdateProfitQuotaCurrentTime")]
        [HarmonyPostfix]
        public static void SetUpdateProfitQuotaCurrentTime(ref int ___quotaFulfilled, ref int ___profitQuota)
        {
            if (!StartOfRound.Instance.isChallengeFile)
            {
                Color color;
                Color text_color;
                if (___quotaFulfilled == 0)
                {
                    color = new Color(0f, 0f, 0f, 1f);
                    text_color = new Color(0f, 1f, 0f, 1f);
                }
                else if (___quotaFulfilled < ___profitQuota)
                {
                    color = new Color(1f, 0f, 0f, 1f);
                    text_color = new Color(0f, 1f, 0f, 1f);
                }
                else if (Plugin.rollover_fraction <= 0 || Plugin.rollover_penalty <= 0 || ___quotaFulfilled < ___profitQuota * (((float)Plugin.rollover_threshold) / 100 + 1))
                {
                    color = new Color(0f, 1f, 0f, 1f);
                    text_color = new Color(0f, 0f, 0f, 1f);
                }
                else
                {
                    color = new Color(1f, 1f, 0f, 1f);
                    text_color = new Color(0f, 0f, 0f, 1f);
                }
                StartOfRound.Instance.deadlineMonitorBGImage.color = color;
                StartOfRound.Instance.profitQuotaMonitorBGImage.color = color;
                StartOfRound.Instance.profitQuotaMonitorText.color = text_color;
                StartOfRound.Instance.deadlineMonitorText.color = text_color;
                
            }
        }
    }
}