using BepInEx;
using LethalConfig.ConfigItems;
using LethalCompTime.Patches;
using BepInEx.Configuration;
using LethalConfig.ConfigItems.Options;
using LobbyCompatibility.Configuration;

namespace LethalCompTime.Configs
{
    [BepInDependency("ainavt.lc.lethalconfig")]
    internal class LethalConfigManager
    {
        public static LethalConfigManager Instance { get; private set; }
        public static ConfigEntry<int> ExampleQuota { get; private set; }
        public static ConfigEntry<int> ExampleRollover { get; private set; }

        public static void Init(ConfigFile config)
        {
            if (Instance == null)
            {
                Instance = new LethalConfigManager(config);
            }
        }

        private LethalConfigManager(ConfigFile config)
        {
            LethalConfig.LethalConfigManager.SetModDescription("Allows for more controlled quota rollover. For if Quota Rollover starts feeling too easy.");

            LethalConfig.LethalConfigManager.AddConfigItem(new GenericButtonConfigItem(
                "Quick Settings",
                "ClassicQR",
                "Sets rollover to be fully held at week end and still give overtime bonus as well.",
                "Classic Quota Rollover (Easiest)",
                () =>
                {
                    ConfigManager.OvertimeOverride.Value = false;
                    ConfigManager.PenaltyUsed.Value = ConfigManager.PenaltyType.Asymptotic;
                    ConfigManager.RolloverFraction.Value = 100;
                    ConfigManager.RolloverThreshold.Value = 100;
                    ConfigManager.RolloverPenalty.Value = 100;
                }));

            LethalConfig.LethalConfigManager.AddConfigItem(new GenericButtonConfigItem(
                "Quick Settings",
                "QRBalanced",
                "Sets rollover to be fully held at week end but doesn't double dip with the overtime bonus.",
                "Balanced Quota Rollover (Easy)",
                () =>
                {
                    ConfigManager.OvertimeOverride.Value = true;
                    ConfigManager.PenaltyUsed.Value = ConfigManager.PenaltyType.Asymptotic;
                    ConfigManager.RolloverFraction.Value = 100;
                    ConfigManager.RolloverThreshold.Value = 100;
                    ConfigManager.RolloverPenalty.Value = 100;
                }));

            LethalConfig.LethalConfigManager.AddConfigItem(new GenericButtonConfigItem(
                "Quick Settings",
                "ScaledDown",
                "Gradually reduces rollover out the more you turn in.",
                "Significant Rollover (Easy)",
                () =>
                {
                    ConfigManager.OvertimeOverride.Value = true;
                    ConfigManager.PenaltyUsed.Value = ConfigManager.PenaltyType.Logarithmic;
                    ConfigManager.RolloverFraction.Value = 100;
                    ConfigManager.RolloverThreshold.Value = 50;
                    ConfigManager.RolloverPenalty.Value = 75;
                }));

            LethalConfig.LethalConfigManager.AddConfigItem(new GenericButtonConfigItem(
                "Quick Settings",
                "HardCapped",
                "You can rollover exactly one quota's worth. No more.",
                "Hard Capped (Easy)",
                () =>
                {
                    ConfigManager.OvertimeOverride.Value = true;
                    ConfigManager.PenaltyUsed.Value = ConfigManager.PenaltyType.Asymptotic;
                    ConfigManager.RolloverFraction.Value = 100;
                    ConfigManager.RolloverThreshold.Value = 100;
                    ConfigManager.RolloverPenalty.Value = 0;
                }));

            LethalConfig.LethalConfigManager.AddConfigItem(new GenericButtonConfigItem(
                "Quick Settings",
                "Recommended",
                "The more you turn in, the closer you'll get to the next quota, but you shouldn't be able to reach it.",
                "Recommended (Medium)",
                () =>
                {
                    ConfigManager.OvertimeOverride.Value = true;
                    ConfigManager.PenaltyUsed.Value = ConfigManager.PenaltyType.Asymptotic;
                    ConfigManager.RolloverFraction.Value = 100;
                    ConfigManager.RolloverThreshold.Value = 50;
                    ConfigManager.RolloverPenalty.Value = 50;
                }));

            LethalConfig.LethalConfigManager.AddConfigItem(new GenericButtonConfigItem(
                "Quick Settings",
                "CompanyIssue",
                "Rollover starts at 50% and decreases from there so that you can never reach a full quota.",
                "Company Issued Rollover (Hard)",
                () =>
                {
                    ConfigManager.OvertimeOverride.Value = true;
                    ConfigManager.PenaltyUsed.Value = ConfigManager.PenaltyType.Asymptotic;
                    ConfigManager.RolloverFraction.Value = 50;
                    ConfigManager.RolloverThreshold.Value = 100;
                    ConfigManager.RolloverPenalty.Value = 50;
                }));

            LethalConfig.LethalConfigManager.AddConfigItem(new GenericButtonConfigItem(
                "Quick Settings",
                "Vanilla",
                "No rollover is given. Only overtime bonus. Just like the base game.",
                "No Rollover (Hardest)",
                () =>
                {
                    ConfigManager.OvertimeOverride.Value = true;
                    ConfigManager.PenaltyUsed.Value = ConfigManager.PenaltyType.Asymptotic;
                    ConfigManager.RolloverFraction.Value = 0;
                    ConfigManager.RolloverThreshold.Value = 100;
                    ConfigManager.RolloverPenalty.Value = 100;
                }));

            LethalConfig.LethalConfigManager.AddConfigItem(new IntSliderConfigItem(ConfigManager.RolloverFraction, false));
            LethalConfig.LethalConfigManager.AddConfigItem(new IntSliderConfigItem(ConfigManager.RolloverThreshold, false));
            LethalConfig.LethalConfigManager.AddConfigItem(new IntSliderConfigItem(ConfigManager.RolloverPenalty, false));
            LethalConfig.LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<ConfigManager.PenaltyType>(ConfigManager.PenaltyUsed, false));
            LethalConfig.LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(ConfigManager.OvertimeOverride, false));
            ExampleQuota = config.Bind("General", "ExampleQuota", 200, "Enter an example amount of quota to rollover (assuming a required 100 quota).");
            var ExampleQuotaItem = new IntInputFieldConfigItem(ExampleQuota, false);
            LethalConfig.LethalConfigManager.AddConfigItem(ExampleQuotaItem);

            ExampleRollover = config.Bind("General", "ExampleRollover", TimeOfDayPatch.CalculateQuotaRollover(ExampleQuota.Value-100, 100), "After 100 quota is collected, this much will be rolled over (assuming the next quota is 100 for simplicity).");
            var ExampleRolloverItem = new IntInputFieldConfigItem(ExampleRollover, new IntInputFieldOptions
            {
                RequiresRestart = false,
                CanModifyCallback = () => CanModifyResult.False("Press Apply to update this value."),
            });

            LethalConfig.LethalConfigManager.AddConfigItem(ExampleRolloverItem);
            ExampleQuota.SettingChanged += (sender, args) =>
            {
                ExampleRollover.Value = TimeOfDayPatch.CalculateQuotaRollover(ExampleQuota.Value-100, 100);
                ExampleRolloverItem.ApplyChanges();
            };
            ConfigManager.RolloverFraction.SettingChanged += (sender, args) =>
            {
                ExampleRollover.Value = TimeOfDayPatch.CalculateQuotaRollover(ExampleQuota.Value-100, 100);
                ExampleRolloverItem.ApplyChanges();
            };
            ConfigManager.RolloverThreshold.SettingChanged += (sender, args) =>
            {
                ExampleRollover.Value = TimeOfDayPatch.CalculateQuotaRollover(ExampleQuota.Value-100, 100);
                ExampleRolloverItem.ApplyChanges();
            };
            ConfigManager.RolloverPenalty.SettingChanged += (sender, args) =>
            {
                ExampleRollover.Value = TimeOfDayPatch.CalculateQuotaRollover(ExampleQuota.Value-100, 100);
                ExampleRolloverItem.ApplyChanges();
            };
            ConfigManager.PenaltyUsed.SettingChanged += (sender, args) =>
            {
                ExampleRollover.Value = TimeOfDayPatch.CalculateQuotaRollover(ExampleQuota.Value-100, 100);
                ExampleRolloverItem.ApplyChanges();
            };
            LethalConfig.LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<ConfigManager.ColorOptions>(ConfigManager.ScreenColoration, false));
            LethalConfig.LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(ConfigManager.ColorUnderFulfilled, false));
            LethalConfig.LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(ConfigManager.ColorFulfilled, false));
            LethalConfig.LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(ConfigManager.ColorOverFulfilled, false));
            LethalConfig.LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(ConfigManager.TextColorOverrideUnderFulfilled, false));
            LethalConfig.LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(ConfigManager.TextColorOverrideFulfilled, false));
            LethalConfig.LethalConfigManager.AddConfigItem(new TextInputFieldConfigItem(ConfigManager.TextColorOverrideOverFulfilled, false));
        }
    }
}