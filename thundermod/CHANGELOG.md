# 1.3.1

- Adding optional quota variable controls (Most will not occur if EasyQuota is installed, reach out if other mods conflict)
  - daysPerQuota will set the vanilla variable QuotaSettings.deadlineDaysAmount
  - daysUntilFulfillment will change the first day that the Early Fulfillment check happens (0 for vanilla)
  - startingQuota will set the vanilla variable QuotaSettings.startingQuota
  - startingCredits will set the vanilla variable QuotaSettings.startingCredits
  - baseIncrease will set the vanilla variable QuotaSettings.baseIncrease
  - randomizerPortion will set the vanilla variable QuotaSettings.randomizerMultiplier
  - inverseSteepness will set the vanilla variable QuotaSettings.increaseSteepness (which actually is divided out of the exponential increase)
- Adds a boolean setting that disables these that defaults to False
- A note: Other than the days until fulfillment and the setting for early fulfillment on any planet, all of these are handled externally, so mods that directly change quota interactions will override them probably entirely. Depending on how they're implemented of course

# 1.3.0

- Added optional early quota turn in, as is in vanilla. Off by default.
- Added debug messages and hid most of them behind a setting. Off by default.
- Reworked logic to use the vanilla code, then modify the results before the RPC call.
  - This should reduce work required on updates to the main game, and reduce incompatibilities with other mods, if they occur.

# 1.2.1

- Variable reference related bugfix.

# 1.2.0

- Adding change for v70. Nothing major.

# 1.1.5

- Bugfix on how I was pulling the furniture list for luck calculations.
- Overtime override bugfix.

# 1.1.4

- Bugfix for quota rollovers to be more accurate (and thus predictible)
- Updated code to allow luck values to be used still!

# 1.1.3

- The wrong DLL was uploaded...

# 1.1.2

- Found documentation on soft compatibilities and added back in LobbyCompatibility stuff
- Added in example entry to test settings before starting the game.

# 1.1.1

- Removing LobbyCompatibility as I can't lazy load it like I can LethalConfig

# 1.1.0

- Adding LethalConfigManager with presets:
    - ClassicQR
    - QRBalanced
    - ScaledDown
    - HardCapped
    - Recommended
    - CompanyIssue
    - Vanilla
- Adding LobbyCompatibility details.

# 1.0.3

- Coloration defaults to off.
- Rollover subtracts from overtime (overridable in configs).
- Coloration fully configurable.
- Logarithmic penalty is available as a replacement for asymptotic.

# 1.0.2

- Fixing divide by zero problems.
- Making error message more descriptive.

# 1.0.1

- Adding some checks for visuals, which can clash with custom monitor mods.
- Adding basic visual configs, including turning it off altogether.

# 1.0.0

- Initial release