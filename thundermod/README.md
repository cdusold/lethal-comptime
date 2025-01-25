Allows for more controlled quota rollover. For if Quota Rollover starts feeling too easy.

By default, half of the value exceeding quota is rolled over and half is kicked into overtime bonus,
but with diminishing returns that prevent reaching the next quota on rollover alone.
Afterall, the company doesn't want you to get away with not feeding it valuable scrap.
You must not rest.
You must work.
You must be a great asset to the company.
Great- great asset to the company.

### Features
- Sets up quota rollover to help reduce hoarding
- Adds a percentage penalty for rollover to encourage hoarding a little
- Attempts to balance rollover making the game too easy and front loaded
- Completely client sideable cosmetic effects, and completely host sideable rollover effects
- LobbyCompatibility integration
- LethalConfig integration includes seven prebuilt settings available for quick access.
- LethalConfig example fields allow to test settings.

### Usage
There are five main functional settings available:
- RolloverBasePercent
    - controls how much quota is rolled over in general.
- RolloverPenaltyThreshold
    - the percent of the next quota obtained before applying the penalty to the rest of the rollover.
- RolloverPenaltyPercent
    - the penalty percent is only applied to the quota above each multiple of the threshold, stacking multiplicatively.
- RolloverPenaltyType
    - Determines how the threshold is handled.
        - Asymptotic applies to each multiple of the quota before penalty. This makes the amount possible to rollover asymptotically approach a value... calculateable by the geometric series. (I don't know if I can get a display in LethalConfig to show the current asymptote, but I could be convinced to change how I calculate this so the asymptote is specified and then a scaling factor is used to make the curve.)
        - Logarithmic applies to each multiple of the quota after penalty. This makes it harder to reach the next multiple of the quota, but it will always be possible given enough value over the current quota.
- RolloverOvertimeOverride
    - Removes the amount rolled over from the overtime bonus calculation, otherwise significantly beating quota early will cause many time the overtime bonus compounding overtime. It's easy to get immensely rich in standard QuotaRollover due to this oversight.
    - I do not recommend setting this to False unlesss you need a boost or just want to have fun. No shade either way, the game can be unfair sometimes. I added this so we all can play how we want.

There are seven visual settings available.
They will not apply to the monitor during the challenge moon.
The values they use for the colors are HTML color codes,
where strings starting with '#' will be parsed as hex codes,
and everything else will be parsed as a color name.
([Full name list not available in Unity's documentation of the function I use.](https://docs.unity3d.com/ScriptReference/ColorUtility.TryParseHtmlString.html))
- RolloverScreenColoration
    - If set to None, no coloration will apply and the monitors will have the default green on black. This is the default partially due to complaints about my lack of color theory knowledge (which is fair).
    - Text applies the next three options to the text on the monitors, leaving the screen background black.
    - Background applies the next three options to the screen background and the three options after that are used to recolor the text in this mode if wanted.
- RolloverColorUnderFulfilled
    - This color applies to the place selected above (text or background) when the quota turned in is less than is needed for this quota. Mostly is an easy way to check if you need to throw a few more scrap in at the company building.
- RolloverColorFulfilled
    - This color applies to the place selected above (text or background) when the quota is met and none of it is getting penalized for exceeding the threshold. If there is no rollover, then there's no penalty and this will still be used.
- RolloverColorOverFulfilled
    - This color applies to the place selected above (text or background) when the quota turned in is enough to hit the penalty threshold specified. Indicates that further scrap turned in will lose more and more rollover value. If there is no threshold (such as with QuotaRollover defaults) then this should never be used.
- RolloverTextColorOverrideUnderFulfilled
    - This color applies to the text when the above is set to Background when the quota turned in is less than is needed for this quota. Mostly is an easy way to check if you need to throw a few more scrap in at the company building.
- RolloverTextColorOverrideFulfilled
    - This color applies to the text when the above is set to Background when the quota is met and none of it is getting penalized for exceeding the threshold. If there is no rollover, then there's no penalty and this will still be used.
- RolloverTextColorOverrideOverFulfilled
    - This color applies to the text when the above is set to Background when the quota turned in is enough to hit the penalty threshold specified. Indicates that further scrap turned in will lose more and more rollover value. If there is no threshold (such as with QuotaRollover defaults) then this should never be used.

### Recommendations

Installing LethalConfig provides vanilla compatible tools to mess with settings in game, and in the case of this mod comes with seven presets to choose from.

- Vanilla offers no rollover, like the base game, and therefore is the hardest setting.
    - To enable this manually, set the base rollover to 0%.
    - Coloration for when you have and have not met quota should still work if that interests you.
- CompanyIssue is the default for the mod, halving initial rollover and diminishing rollover value such that you'll never reach a full week off.
    - To enable this manually set:
        - RolloverOvertimeOverride to true
        - RolloverPenaltyType to Asymptotic
        - RolloverBasePercent to 50
        - RolloverPenaltyThreshold to 100
        - RolloverPenaltyPercent to 50
- Recommended is a setting that I recommend if you just want your first rollover settings to play with. It's not the default because it decreases the overtime bonus a bit more substantially for lower rollover values, which doesn't quite feel like it's the best for more experienced players. That being said, everyone is free to play how they want.
    - To enable this manually set:
        - RolloverOvertimeOverride to true
        - RolloverPenaltyType to Asymptotic
        - RolloverBasePercent to 100
        - RolloverPenaltyThreshold to 50
        - RolloverPenaltyPercent to 50
- HardCapped allows you to rollover 100% of the next quota worth maximum. After that the rest goes to overtime bonus and your credits on the ship only.
    - To enable this manually set:
        - RolloverOvertimeOverride to true
        - RolloverPenaltyType to Asymptotic
        - RolloverBasePercent to 100
        - RolloverPenaltyThreshold to 100
        - RolloverPenaltyPercent to 0
- ScaledDown provides a slightly more diminished return that is somewhere between Quota Rollover's original functionality and the defaults for this mod.
    - To enable this manually set:
        - RolloverOvertimeOverride to true
        - RolloverPenaltyType to Logarithmic
        - RolloverBasePercent to 100
        - RolloverPenaltyThreshold to 50
        - RolloverPenaltyPercent to 75
- QRBalanced balances the original functionality of Quota Rollover by effectively disabling overtime bonus
    - To enable this manually set:
        - RolloverOvertimeOverride to true
        - RolloverPenaltyType to Asymptotic
        - RolloverBasePercent to 100
        - RolloverPenaltyThreshold to 100
        - RolloverPenaltyPercent to 100
- ClassicQR imitates the Quota Rollover mod directly by double counting rollover towards overtime bonus.
    - To enable this manually set:
        - RolloverOvertimeOverride to false
        - RolloverPenaltyType to Asymptotic
        - RolloverBasePercent to 100
        - RolloverPenaltyThreshold to 100
        - RolloverPenaltyPercent to 100