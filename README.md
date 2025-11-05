# Foldables
You can now fold backpacks and vests!

**Foldables** introduces the ability to fold your gear, *collapsing* into smaller grid sizes. This way you can save space in your stash, or keep valuable backpacks/vests you found in-raid.

---

## Foldables {.tabset}
### Features 
- Every compatible gear now has a folded state: Fold like you would a gun by using <kbd>middle mouse click</kbd> or by using the context menu
- Compatible gear includes: Backpacks, Unarmored Vests
- Realism and balancing: Folded gear cannot be opened and cannot contain any items. Folding in-raid takes time, scaled based on the size of the gear
- Seamless experience: Unfold gear on equip. Unfold gear when trying to open it. Ask to spill the contents when folding a container that is not empty. Ability to fold/unfold items on the ground in-raid, by holding the interaction button (<kbd>F</kbd>)
- Configurable: Configuration is available server side and client side

### Installation
- Extract the contents of the .zip archive into your SPT folder.
![Installation](https://i.imgur.com/3N6gTe2.gif)
Thank you [DrakiaXYZ](https://forge.sp-tarkov.com/user/27605/drakiaxyz) for the gif

### Configuration
#### Server Side
In the `config.json` file:
- `MinFoldingTime` - Minimum folding time in-raid for the smallest gear. Default is `1` second
- `MaxFoldingTime` - Maximum folding time in-raid for the largest gear. Default is `5` seconds. Setting both to `0` effectively removes the delay in-raid

For a more tailored experience:
- `Overrides` - Override foldable properties for certain gear. See `config.json` for an example
- `BackpackFoldedCellSizes` and `VestFoldedCellSizes` - Folded sizes are calculated based on this, for example (using default values) all backpacks with a grid count of <= `15` will have a folded size of `1x2`

#### Client Side
In the BepInEx configuration manager (<kbd>F12</kbd>)
- `Fold While Equipped` - Allow folding while gear is equipped. Default is `true` 
- `Spill Confirmation` - Show a confirmation dialogue before spilling contents, if possible. If disabled, always spill without asking. Default is `true`

### Compatibility
- [UIFixes](https://forge.sp-tarkov.com/mod/1342/ui-fixes) by [Tyfon](https://forge.sp-tarkov.com/user/46005/tyfon) MultiSelect interoperability: Fold/unfold multiple gear if it allows
- This should work with mods that add custom gear

### Support
If you find bugs, or have feature suggestions, feel free to post them on the comment section, or open an issue on GitHub, or most preferably through the SPT Discord, ozen

#### Contributing
- I would appreciate it if you could help translate this mod into your language. Locale files are inside `config/locales/`, with your language `__.json`

#### Future Plans
- Folded static loot
- Custom folded models (okay, maybe not)

### Credits
- Thanks to [Tyfon](https://forge.sp-tarkov.com/user/46005/tyfon) for entertaining my questions, and his work on UIFixes which showed what else can be possible
- Thanks to [BloodRain13](https://forge.sp-tarkov.com/user/86388/bloodrain13) for helping test this mod extensively
- Thanks to [Gleneth](https://forge.sp-tarkov.com/user/26904/gleneth) for his help on initial testing
- FlatIcon - [1](https://www.flaticon.com/free-icon/military_3856880), [2](https://www.flaticon.com/free-icon/clean-clothes_9619571)

{.endtabset}
