using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MS_GameHUD_Shared;
using Sharp.Extensions.GameEventManager;
using Sharp.Shared;
using Sharp.Shared.Abstractions;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using Sharp.Shared.Units;
using static MS_GameHUD_Shared.IGameHUDAPI;

namespace MS_GameHUD
{
    public class GameHUD: IModSharpModule, IGameHUDAPI
    {
        public string DisplayName => "GameHUD";
        public string DisplayAuthor => "DarkerZ[RUS]";
        public GameHUD(ISharedSystem sharedSystem, string dllPath, string sharpPath, Version version, IConfiguration coreConfiguration, bool hotReload)
        {
            _modules = sharedSystem.GetSharpModuleManager();
            _convars = sharedSystem.GetConVarManager();
            _modSharp = sharedSystem.GetModSharp();
            _entityManager = sharedSystem.GetEntityManager();
            _entities = sharedSystem.GetEntityManager();
            _transmits = sharedSystem.GetTransmitManager();

            var services = new ServiceCollection();
            services.AddSingleton(sharedSystem);
            services.AddGameEventManager();
            _provider = services.BuildServiceProvider();
            _gameEventManager = _provider.GetRequiredService<IGameEventManager>();
        }
        private readonly ISharpModuleManager _modules;
        private readonly IConVarManager _convars;
        public static IModSharp? _modSharp;
        public static IEntityManager? _entityManager;
        private readonly IEntityManager _entities;
        public static ITransmitManager? _transmits;
        private readonly IServiceProvider _provider;
        private readonly IGameEventManager _gameEventManager;


        public static bool g_bMethod = false;
        public static HUD[] g_HUD = new HUD[65];
        private IConVar? g_cvar_method;

        public bool Init()
        {
            for (int i = 0; i < g_HUD.Length; i++) g_HUD[i] = new HUD();
            g_cvar_method = _convars.CreateConVar("ms_gamehud_method", false, "true - point_orient, false - teleport", ConVarFlags.Notify);
            if (g_cvar_method != null) _convars.InstallChangeHook(g_cvar_method, OnCvarMethodChanged);

            _provider.LoadAllSharpExtensions();
            _gameEventManager.HookEvent("player_connect_full", OnPlayerConnectFull);
            _gameEventManager.HookEvent("player_disconnect", OnPlayerDisconnect);
            _gameEventManager.HookEvent("player_spawn", OnPlayerSpawn);
            _gameEventManager.HookEvent("player_death", OnPlayerDeath);
            _gameEventManager.HookEvent("round_start", OnRoundStart);

            return true;
        }

        

        public void PostInit()
        {
            _modules.RegisterSharpModuleInterface<IGameHUDAPI>(this, IGameHUDAPI.Identity, this);

            _modSharp!.PushTimer(OnTick, 0.02, GameTimerFlags.Repeatable);
            _modSharp!.PushTimer(OnTransmit, 0.1, GameTimerFlags.Repeatable);
        }

        public void Shutdown()
        {
            _provider.ShutdownAllSharpExtensions();

            if (g_cvar_method != null) _convars.RemoveChangeHook(g_cvar_method, OnCvarMethodChanged);

            foreach (HUD hud in g_HUD)
            {
                hud.RemoveAllHUD();
                hud.RemovePointOrient();
            }
        }

        private HookReturnValue<bool> OnPlayerConnectFull(IGameEvent e, ref bool serverOnly)
        {
            if (e.GetPlayerController("userid") is { } player)
            {
                g_HUD[player.PlayerSlot].SetHUDPlayer(player);
            }
            return new HookReturnValue<bool>();
        }

        private HookReturnValue<bool> OnPlayerDisconnect(IGameEvent e, ref bool serverOnly)
        {
            if (e.GetPlayerController("userid") is { } player)
            {
                g_HUD[player.PlayerSlot].RemoveAllHUD();
                g_HUD[player.PlayerSlot].RemovePointOrient();
                g_HUD[player.PlayerSlot].SetHUDPlayer(null);
            }
            return new HookReturnValue<bool>();
        }

        private HookReturnValue<bool> OnPlayerSpawn(IGameEvent e, ref bool serverOnly)
        {
            if (e.GetPlayerController("userid") is { } player)
            {
                UpdateEvent(player);
            }
            return new HookReturnValue<bool>();
        }

        private HookReturnValue<bool> OnPlayerDeath(IGameEvent e, ref bool serverOnly)
        {
            if (e.GetPlayerController("userid") is { } player)
            {
                UpdateEvent(player);
            }
            return new HookReturnValue<bool>();
        }

        private HookReturnValue<bool> OnRoundStart(IGameEvent e, ref bool serverOnly)
        {
            foreach (var hud in g_HUD)
            {
                _ = _modSharp!.PushTimer(() => UpdateEvent(hud.GetHUDPlayer()), 1.0f);
            }
            return new HookReturnValue<bool>();
        }

        private void OnTick()
        {
            if (g_bMethod) return;
            foreach (var hud in g_HUD) hud.ShowAllHUD();
        }

        private void OnTransmit()
        {
            var controllers = GetControllersToTransmit().ToArray();
            foreach (var player in controllers)
            {
                for (int i = 0; i < g_HUD.Length; i++)
                {
                    if (player.PlayerSlot != i)
                        foreach (var channel in g_HUD[i].Channel)
                            if (channel.Value.WTTransmitValid()) _transmits!.SetEntityState(channel.Value.WTGetIndex(), player.Index, true, -1);
                }
            }
        }

        private IEnumerable<IPlayerController> GetControllersToTransmit()
        {
            var max = new PlayerSlot((byte)_modSharp!.GetGlobals().MaxClients);

            for (PlayerSlot slot = 0; slot <= max; slot++)
            {
                if (_entities.FindPlayerControllerBySlot(slot) is { } c)
                {
                    yield return c;
                }
            }
        }

        private static void UpdateEvent(IPlayerController? player)
        {
            if (player != null && player.IsValid())
            {
                foreach (var pair in g_HUD[player.PlayerSlot].Channel)
                {
                    if (!pair.Value.EmptyMessage())
                    {
                        pair.Value.CreateHUD();
                    }
                }
            }
        }

        private void OnCvarMethodChanged(IConVar conVar)
        {
            g_bMethod = conVar.GetBool();
        }

        public void Native_GameHUD_SetParams(IPlayerController Player, byte channel, Sharp.Shared.Types.Vector vec, Sharp.Shared.Types.Color32 color, int fontsize, string fontname, float units, PointWorldTextJustifyHorizontal_t justifyhorizontal, PointWorldTextJustifyVertical_t justifyvertical, PointWorldTextReorientMode_t reorientmode, float bgborderheight, float bgborderwidth)
        {
            if (!Player.IsValid()) return;
            g_HUD[Player.PlayerSlot].CreateorGetChannel(channel)?.Params(vec, color, fontsize, fontname, units, justifyhorizontal, justifyvertical, reorientmode, bgborderheight, bgborderwidth);
        }
        public void Native_GameHUD_UpdateParams(IPlayerController Player, byte channel, Sharp.Shared.Types.Vector vec, Sharp.Shared.Types.Color32 color, int fontsize = 18, string fontname = "Verdana", float units = 0.25F, PointWorldTextJustifyHorizontal_t justifyhorizontal = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT, PointWorldTextJustifyVertical_t justifyvertical = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_TOP, PointWorldTextReorientMode_t reorientmode = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE, float bgborderheight = 0, float bgborderwidth = 0)
        {
            if (!Player.IsValid()) return;
            g_HUD[Player.PlayerSlot].CreateorGetChannel(channel)?.UpdateParams(vec, color, fontsize, fontname, units, justifyhorizontal, justifyvertical, reorientmode, bgborderheight, bgborderwidth);
        }
        public void Native_GameHUD_Show(IPlayerController Player, byte channel, string message, float time)
        {
            if (!Player.IsValid()) return;
            g_HUD[Player.PlayerSlot].CreateorGetChannel(channel)?.Show(message, time);
        }
        public void Native_GameHUD_ShowPermanent(IPlayerController Player, byte channel, string message)
        {
            if (!Player.IsValid()) return;
            g_HUD[Player.PlayerSlot].CreateorGetChannel(channel)?.ShowPermanent(message);
        }
        public void Native_GameHUD_Remove(IPlayerController Player, byte channel)
        {
            if (!Player.IsValid()) return;
            g_HUD[Player.PlayerSlot].RemoveChannel(channel);
        }
        public void Native_GameHUD_SetKeyValue(IPlayerController Player, byte channel, string key, string value)
        {
            if (!Player.IsValid()) return;
            g_HUD[Player.PlayerSlot].CreateorGetChannel(channel)?.SetKeyValue(key, value);
        }
        public string? Native_GameHUD_GetKeyValue(IPlayerController Player, byte channel, string key)
        {
            if (!Player.IsValid()) return null;
            return g_HUD[Player.PlayerSlot].CreateorGetChannel(channel)?.GetKeyValue(key);
        }
    }
}