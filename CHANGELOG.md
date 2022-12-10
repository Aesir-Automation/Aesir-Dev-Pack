# Dev Pack Change Log

## v1.5.0

- Added Toggle(string name) for toggles that are rotation toggleable
- Added MountedAction() to separate out mounted ticks
  - Auto-dismount will only occur if the user has AutoStart turned on
- Non-breaking change to CreateManualMacro(string name, string macroText) to remove target since it is unused