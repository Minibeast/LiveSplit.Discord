# Syntax for LiveSplit.Discord

The customization options offered in LiveSplit.Discord require some explanation for how they work and how certain logic is handled. This document intends to provide its best explanation on the available options for customizing the Rich Presence, as well as some of the limitations.

## Basic Syntax
***Note, all options start with a `%` sign when typed into the options page.***

`game` - Displays the "Game Name" for whatever splits are currently open in LiveSplit.

`game_short` - Displays the shortened version of the "Game Name" for whatever splits are currently open in LiveSplit.

`category` - Displays the "Run Category" for whatever splits are currently open in LiveSplit.

`category_detailed` - Displays the "Run Category," along with whatever variables are set in the "Additional Info" tab, for whatever splits are currently open in LiveSplit.

`attempts` - Displays the number of attempts for whatever splits are currently open in LiveSplit.

`delta` - Displays a Delta of your choosing (See Delta Customization).

`split` - Displays the name of the current split.

`comparison` - Displays the name of the Comparison selected in the "Global Comparison" option (See Global Comparison).

`time` - Displays the current time of the run (Only intended to be used for when the run has finished, use "Elapsed Time" if you wish to have the time displayed during a run).

`inherit` - Inherits the "Running" text (See Inheriting).

## Delta Customization
The `delta` option provides several methods of customization to display the delta for any vanilla comparison (i.e included in the standard version of LiveSplit [version 1.8.16]). The comparison must be enabled in LiveSplit's settings (Settings -> Choose active comparisons).

***These options are typed with the syntax of `delta_[comparison_identifier]`. If no identifier is included with its usage in the settings, the Global Comparison will be used instead (See Global Comparison).***


`pb` - Delta for the Personal Best Comparison.

`cur` - Delta for the Current Comparison (independent from the Global Comparison).

`bst` - Delta for the Best Split Times Comparison.

`best` - Delta for the Best Segments Comparison.

`avg` - Delta for the Average Segments Comparison.

`med` - Delta for the Median Segments Comparison.

`worst` - Delta for the Worst Segments Comparison.

`bal` - Delta for the Balanced PB Comparison.

`latest` - Delta for the Latest Run Comparison.

*Example: `%delta_pb` - Displays the delta relative to the Personal Best on the splits.*

## Global Comparison
The Global Comparison is an option available in the base settings of LiveSplit.Discord, and allows for the user to set a default comparison for options to utilize (presuming the user has not overridden the Global Comparison (See Delta Customization)).

All active comparisons are available in the dropdown menu, including "Current Comparison," which will default to whatever comparison is currently selected in base LiveSplit.

*Note: If "Current Comparison" is the selected option, the `comparison` option will display the name of the Current Comparison, not the literal text "Current Comparison."*

If "None" is selected from the dropdown, the Small Image in the Rich Presence will not be displayed. This will only happen if "None" is manually selected and not if the user switches to the "None" comparison while "Current Comparison" is the selected Global Comparison.

## Inheriting
Using the `inherit` option for any field outside of the "Running" tab will inherit the text for the relative field in the "Running" tab. This makes customization easier for users who do not wish to customize to the full extent LiveSplit.Discord provides.

Little logic is used for the `inherit` text, so it will function in an easily predictable way.

If the particular section utilizes the `delta` or `split` option, the text will be overridden by custom text that's dependent on the tab it is located in:

For the Paused tab, it will replace the text with "Paused"

For the Not Running tab, it will replace the text with "Not Running"

For the Ended tab, it will replace the text with "Ended. Final time: [`time` option]"