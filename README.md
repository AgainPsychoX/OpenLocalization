
# OpenLocalization

Mod for [Pharaoh: A New Era](https://store.steampowered.com/app/1351080/Pharaoh_A_New_Era/) game that allows loading unofficial localizations.

[Join modding community at Discord!](https://discord.gg/avWq99Aw)





## Features

+ Loading unofficial localizations.
+ Configurable updating from online spreadsheets.
+ Open for new translations and contrubution.





## Notes

+ The game uses `I2Loc` solution for localization. 
+ Use with [BepInEx.ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager) to see some simple config GUI that includes few buttons for instant update & reload.
+ Structure of external sources:
	* Separate folder for each source
	* Meta file:
		+ Enabled flag
		+ Website link
		+ Updates related info (frequency, spreadsheet key/macro link) 
	* CSV files (one per "main category" of localized terms).
* Assumption: There is only one built-in (asset) source for localization. We assume it will stay like that as PANE is not that large game.
* The built-in localization source is converted to the same structure (if folder not found already), as special name: `BuiltIn` with default order value 1. It isn't loaded if `SkipAssets` is false.



### To-do

+ Testing & fixing...
+ Expose `eGoogleUpdateSynchronization` as mode (shared across all sources) 
+ One unofficial all-in spreadsheet, community maintained.
+ Think about rewriting the backend code?
+ Patch `I2Loc.LanguageSourceData.Import_Google_CreateWWWcall(bool ForceUpdate, bool justCheck)` and backend to make use of `justCheck` flag, 
	+ Avoid downloading large amount of data when "just checking" if update is avaliable.
	+ Add checking for the update on GUI open/before downloading. 
	+ Show installed & latest version in the GUI.
+ Add GUI for simple config (hook up to configuration manager private code using reflections).
+ Lazy load actual data from `LocalizationSource` (only if enabled and once set to enabled).
+ Make configuration/GUI compatibile with [alternative configuration manager from `sinai-dev`](https://github.com/sinai-dev/BepInExConfigManager).
+ Maybe try do UI inside Pharaoh options themself? Would be nice to have separate library for that tho, maybe even working the same was as the configuration managers.


