Allows for more controlled quota rollover. For if Quota Rollover starts feeling too easy.

### Features
- Sets up quota rollover to help reduce hoarding
- Adds a percentage penalty for rollover to encourage hoarding a little
- Attempts to balance rollover making the game too easy and front loaded

### Usage
There are three main settings available, the base rollover percent, the threshold, and the penalty percent.
The base rollover percent controls how much quota is rolled over in general.
The threshold percent is the percent of the next quota obtained before applying the penalty to the rest of the rollover.
The penalty percent is only applied to the quota above each multiple of the threshold, stacking multiplicatively.

Additionally, you can set the penalty to be logarithmic, and only apply once the penalized rollover hits the next threshold amount, instead of the unpenalized rollover.
This is a slightly more QuotaRollover like experience, but still decreases the rollover.

The coloration options default to None.
Setting them to Text mode uses the indication colors to color the text and leaves the background the default color.
Setting them to Screen colorizes the background with the indication color and allows you to override the text colors to make them readable.

If you want to play vanilla, and just have the color highlights on the quota board, set the base rollover to 0%.
This only enables Low and Fulfilled color values.

If you want the settings to match Quota Rollover, set the base rollover to 100%, and the threshold to 0% to disable it.
Additionally, turn off the overtime override, as the overtime bonus double dips the rollover money in vanilla Quota Rollover.
This also only enables Low and Fulfilled color values.

By default, the base is 50%, the threshold is 100%, and the penalty is 50%, which makes it impossible to rollover enough to get the next three days off.
Afterall, the company doesn't want you to get away with not feeding it valuable scrap.
You must not rest.
You must work.
You must be a great asset to the company.
Great- great asset to the company.