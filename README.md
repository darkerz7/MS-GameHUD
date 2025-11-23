# [Core]MS-GameHUD
API for displaying messages to the player. GameText analogue

## Required packages:
1. [ModSharp](https://github.com/Kxnrl/modsharp-public)

## Installation:
1. Compile or copy MS-GameHUD to `sharp/modules/MS-GameHUD` folger
2. Compile or copy MS-GameHUD-Shared to `sharp/shared/MS-GameHUD-Shared` folger
3. Restart server

## CVARs(temporarily, maybe there is a better method):
Cvar | Parameter | Description
--- | --- | ---
`ms_gamehud_method` | <0/1> | true - point_orient, false - teleport

## Example:
### Add the dependency MS-GameHUD to your project:
```
using MS_GameHUD_Shared;
using static MS_GameHUD_Shared.IGameHUDAPI;

<My plugin class>: IModSharpModule
{
	<...>
	public void OnAllModulesLoaded() => GetGameHUD();
	public void OnLibraryConnected(string name)
	{
		if (name.Equals("GameHUD")) GetGameHUD();
	}
	public void OnLibraryDisconnect(string name)
	{
		if (name.Equals("GameHUD")) _igamehud = null;
	}
	private IModSharpModuleInterface<IGameHUDAPI>? _igamehud;
	private IGameHUDAPI? GetGameHUD()
	{
		if (_igamehud?.Instance is null) _igamehud = _modules.GetOptionalSharpModuleInterface<IGameHUDAPI>(IGameHUDAPI.Identity);
		return _igamehud?.Instance;
	}
	<...>
}
```

### Using API function. Description of parameters see in source code MS-GameHUD-Shared
```
public void MyExampleFunc(IPlayerController? player)
{
	<...>
	if (player.IsValid() && GetGameHUD() is { } _api)
	{
		<...>
		_api.Native_GameHUD_SetParams(player, 0, new(20, 20, 80), new(255, 0, 0, 255));
		_api.Native_GameHUD_Show(player, 0, "MyMessage", 10.0f);
		<...>
		_api.Native_GameHUD_Remove(player, 0);
		<...>
	}
	<...>
}
```
### Screenshot
![20251123094254_1](https://github.com/user-attachments/assets/cc1c4473-758b-4599-8919-442708cd87cf)
