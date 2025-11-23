using Microsoft.Extensions.Configuration;
using MS_GameHUD_Shared;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using static MS_GameHUD_Shared.IGameHUDAPI;

namespace MS_GameHUD_Example
{
    public class GameHUDExample : IModSharpModule
    {
        public string DisplayName => "GameHUD-Example";
        public string DisplayAuthor => "DarkerZ[RUS]";

        public GameHUDExample(ISharedSystem sharedSystem, string dllPath, string sharpPath, Version version, IConfiguration coreConfiguration, bool hotReload)
        {
            _modules = sharedSystem.GetSharpModuleManager();
            _clientmanager = sharedSystem.GetClientManager();
        }

        private readonly ISharpModuleManager _modules;
        private readonly IClientManager _clientmanager;

        public bool Init()
        {
            _clientmanager.InstallCommandCallback("hudtest1", OnCommandTest1);
            _clientmanager.InstallCommandCallback("hudtest2", OnCommandTest2);
            _clientmanager.InstallCommandCallback("hudtest3", OnCommandTest3);
            _clientmanager.InstallCommandCallback("hudtest4", OnCommandTest4);
            _clientmanager.InstallCommandCallback("hudtest5", OnCommandTest5);
            _clientmanager.InstallCommandCallback("hudtest6", OnCommandTest6);
            _clientmanager.InstallCommandCallback("hudtest7", OnCommandTest7);
            _clientmanager.InstallCommandCallback("hudtest8", OnCommandTest8);
            return true;
        }

        public void Shutdown()
        {
            _clientmanager.RemoveCommandCallback("hudtest1", OnCommandTest1);
            _clientmanager.RemoveCommandCallback("hudtest2", OnCommandTest2);
            _clientmanager.RemoveCommandCallback("hudtest3", OnCommandTest3);
            _clientmanager.RemoveCommandCallback("hudtest4", OnCommandTest4);
            _clientmanager.RemoveCommandCallback("hudtest5", OnCommandTest5);
            _clientmanager.RemoveCommandCallback("hudtest6", OnCommandTest6);
            _clientmanager.RemoveCommandCallback("hudtest7", OnCommandTest7);
            _clientmanager.RemoveCommandCallback("hudtest8", OnCommandTest8);
        }

        private ECommandAction OnCommandTest1(IGameClient client, StringCommand command)
        {
            if (client.IsValid && GetGameHUD() is { } _api)
            {
                var player = client.GetPlayerController()!;
                _api.Native_GameHUD_Show(player, 0, "TestMessage1", 10.0f);
            }

            return ECommandAction.Stopped;
        }

        private ECommandAction OnCommandTest2(IGameClient client, StringCommand command)
        {
            if (client.IsValid && GetGameHUD() is { } _api)
            {
                var player = client.GetPlayerController()!;
                _api.Native_GameHUD_SetParams(player, 1, new(20, 20, 80), new(255, 0, 0, 255));
                _api.Native_GameHUD_Show(player, 1, "TestMessage2", 30.0f);
            }

            return ECommandAction.Stopped;
        }

        private ECommandAction OnCommandTest3(IGameClient client, StringCommand command)
        {
            if (client.IsValid && GetGameHUD() is { } _api)
            {
                var player = client.GetPlayerController()!;
                _api.Native_GameHUD_Remove(player, 1);
            }

            return ECommandAction.Stopped;
        }

        private ECommandAction OnCommandTest4(IGameClient client, StringCommand command)
        {
            if (client.IsValid && GetGameHUD() is { } _api)
            {
                var player = client.GetPlayerController()!;
                _api.Native_GameHUD_SetParams(player, 2, new(0, 0, 7), new(100, 100, 255, 255), 24, "Arial", 0.03f, PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_CENTER, PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_BOTTOM, PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE, 0.3f, 0.15f);
                _api.Native_GameHUD_Show(player, 2, "TestMessage3", 10.0f);
            }

            return ECommandAction.Stopped;
        }

        private ECommandAction OnCommandTest5(IGameClient client, StringCommand command)
        {
            if (client.IsValid && GetGameHUD() is { } _api)
            {
                var player = client.GetPlayerController()!;
                _api.Native_GameHUD_ShowPermanent(player, 2, "TestMessage4");
            }

            return ECommandAction.Stopped;
        }

        private ECommandAction OnCommandTest6(IGameClient client, StringCommand command)
        {
            if (client.IsValid && GetGameHUD() is { } _api)
            {
                var player = client.GetPlayerController()!;
                _api.Native_GameHUD_UpdateParams(player, 2, new(-30, -30, 80), new(255, 100, 255, 255), 16, "Verdana", 0.2f, PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_RIGHT, PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_TOP, PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE, 5.0f, 10.0f);
            }

            return ECommandAction.Stopped;
        }

        private ECommandAction OnCommandTest7(IGameClient client, StringCommand command)
        {
            if (client.IsValid && GetGameHUD() is { } _api)
            {
                var player = client.GetPlayerController()!;
                Console.WriteLine($"[GameHUD-Exmaple] font_size: {_api.Native_GameHUD_GetKeyValue(player, 2, "font_size")}");
                Console.WriteLine($"[GameHUD-Exmaple] font_name: {_api.Native_GameHUD_GetKeyValue(player, 2, "font_name")}");
                Console.WriteLine($"[GameHUD-Exmaple] world_units_per_pixel: {_api.Native_GameHUD_GetKeyValue(player, 2, "world_units_per_pixel")}");
                Console.WriteLine($"[GameHUD-Exmaple] justify_horizontal: {_api.Native_GameHUD_GetKeyValue(player, 2, "justify_horizontal")}");
                Console.WriteLine($"[GameHUD-Exmaple] justify_vertical: {_api.Native_GameHUD_GetKeyValue(player, 2, "justify_vertical")}");
                Console.WriteLine($"[GameHUD-Exmaple] reorient_mode: {_api.Native_GameHUD_GetKeyValue(player, 2, "reorient_mode")}");
                Console.WriteLine($"[GameHUD-Exmaple] background_border_height: {_api.Native_GameHUD_GetKeyValue(player, 2, "background_border_height")}");
                Console.WriteLine($"[GameHUD-Exmaple] background_border_width: {_api.Native_GameHUD_GetKeyValue(player, 2, "background_border_width")}");
                Console.WriteLine($"[GameHUD-Exmaple] enabled: {_api.Native_GameHUD_GetKeyValue(player, 2, "enabled")}");
                Console.WriteLine($"[GameHUD-Exmaple] fullbright: {_api.Native_GameHUD_GetKeyValue(player, 2, "fullbright")}");
                Console.WriteLine($"[GameHUD-Exmaple] color: {_api.Native_GameHUD_GetKeyValue(player, 2, "color")}");
                Console.WriteLine($"[GameHUD-Exmaple] message: {_api.Native_GameHUD_GetKeyValue(player, 2, "message")}");
                Console.WriteLine($"[GameHUD-Exmaple] draw_background: {_api.Native_GameHUD_GetKeyValue(player, 2, "draw_background")}");
            }

            return ECommandAction.Stopped;
        }

        private ECommandAction OnCommandTest8(IGameClient client, StringCommand command)
        {
            if (client.IsValid && GetGameHUD() is { } _api)
            {
                var player = client.GetPlayerController()!;
                _api.Native_GameHUD_SetKeyValue(player, 2, "font_size", "32");
                _api.Native_GameHUD_SetKeyValue(player, 2, "font_name", "Arial");
                _api.Native_GameHUD_SetKeyValue(player, 2, "world_units_per_pixel", "0,05");
                _api.Native_GameHUD_SetKeyValue(player, 2, "justify_horizontal", "0");
                _api.Native_GameHUD_SetKeyValue(player, 2, "justify_vertical", "1");
                _api.Native_GameHUD_SetKeyValue(player, 2, "reorient_mode", "1");
                _api.Native_GameHUD_SetKeyValue(player, 2, "background_border_height", "8,0");
                _api.Native_GameHUD_SetKeyValue(player, 2, "background_border_width", "4,0");
            }

            return ECommandAction.Stopped;
        }

        //Init IGameHUDAPI
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
    }
}
