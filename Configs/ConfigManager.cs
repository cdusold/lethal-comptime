using BepInEx.Configuration;

namespace LethalCompTime.Configs
{
    internal class ConfigManager
    {
        public static ConfigManager Instance { get; private set; }

        public static void Init(ConfigFile config)
        {
            if (Instance == null)
            {
                Instance = new ConfigManager(config);
            }
        }

        public static ConfigEntry<int> RolloverFraction { get; private set; }
        public static ConfigEntry<int> RolloverThreshold { get; private set; }
        public static ConfigEntry<int> RolloverPenalty { get; private set; }

        public enum PenaltyType
        {
            Asymptotic = 0,
            Logarithmic = 1,
        };

        public static ConfigEntry<PenaltyType> PenaltyUsed { get; private set; }
        public static ConfigEntry<bool> OvertimeOverride { get; private set; }

        public enum ColorOptions
        {
            None = 0,
            Text = 1,
            Screen = 2
        };

        public static ConfigEntry<ColorOptions> ScreenColoration { get; private set; }
        public static ConfigEntry<string> ColorOverFulfilled { get; private set; }
        public static ConfigEntry<string> ColorFulfilled { get; private set; }
        public static ConfigEntry<string> ColorUnderFulfilled { get; private set; }
        public static ConfigEntry<string> TextColorOverrideOverFulfilled { get; private set; }
        public static ConfigEntry<string> TextColorOverrideFulfilled { get; private set; }
        public static ConfigEntry<string> TextColorOverrideUnderFulfilled { get; private set; }

        private ConfigManager(ConfigFile config)
        {
            RolloverFraction = config.Bind(
                "General",
                "RolloverBasePercent",
                50,
                "The starting percentage of your excess quota that automatically carries over to the next cycle."
            );
            RolloverThreshold = config.Bind(
                "General",
                "RolloverPenaltyThreshold",
                100,
                "The point at which diminishing returns take effect for rollover amounts. Each multiple of the threshold exceeded reduces the rollover of the surplus over that threshold."
            );
            RolloverPenalty = config.Bind(
                "General",
                "RolloverPenaltyPercent",
                50,
                "The percentage deduction applied to your rollover amount for each threshold exceeded. This creates a gradual decline in rollover benefits for larger surpluses."
            );
            PenaltyUsed = config.Bind(
                "General",
                "RolloverPenaltyType",
                PenaltyType.Asymptotic,
                "Whether to use asymptotic or logarithmic rollover scaling."
            );
            OvertimeOverride = config.Bind(
                "General",
                "RolloverOvertimeOverride",
                true,
                "Set this to false to allow full overtime bonus even with rollover. (Not Recommended.)"
            );
            ScreenColoration = config.Bind(
                "Visual",
                "RolloverScreenColoration",
                ColorOptions.None,
                "If and how to display quota status colors. None, Text (only), Screen (background)."
            );
            ColorUnderFulfilled = config.Bind(
                "Visual",
                "RolloverColorUnderFulfilled",
                "red",
                "The indication color for if the quota isn't met yet."
            );
            ColorFulfilled = config.Bind(
                "Visual",
                "RolloverColorFulfilled",
                "green",
                "The indication color for if the quota has been met."
            );
            ColorOverFulfilled = config.Bind(
                "Visual",
                "RolloverColorOverFulfilled",
                "yellow",
                "The indication color for if a penalty will be applied to the rollover."
            );
            TextColorOverrideUnderFulfilled = config.Bind(
                "Visual",
                "RolloverTextColorOverrideUnderFulfilled",
                "#000000",
                "The text color in Scene mode for when the quota isn't met yet."
            );
            TextColorOverrideFulfilled = config.Bind(
                "Visual",
                "RolloverTextColorOverrideFulfilled",
                "#000000",
                "The text color in Scene mode for when the quota has been met."
            );
            TextColorOverrideOverFulfilled = config.Bind(
                "Visual",
                "RolloverTextColorOverrideOverFulfilled",
                "#000000",
                "The text color in Scene mode for when a penalty will be applied to the rollover."
            );
        }
    }
}