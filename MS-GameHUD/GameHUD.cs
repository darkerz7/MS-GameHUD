using Microsoft.Extensions.Configuration;
using MS_GameHUD_Shared;
using Sharp.Shared;
using Sharp.Shared.Enums;
using Sharp.Shared.GameEntities;
using Sharp.Shared.HookParams;
using Sharp.Shared.Listeners;
using Sharp.Shared.Managers;
using Sharp.Shared.Objects;
using Sharp.Shared.Units;
using static MS_GameHUD_Shared.IGameHUDAPI;

namespace MS_GameHUD
{
    public class GameHUD: IModSharpModule, IGameHUDAPI, IGameListener, IClientListener
    {
        public string DisplayName => "GameHUD";
        public string DisplayAuthor => "DarkerZ[RUS]";
        public GameHUD(ISharedSystem sharedSystem, string dllPath, string sharpPath, Version version, IConfiguration coreConfiguration, bool hotReload)
        {
            _modules = sharedSystem.GetSharpModuleManager();
            _convars = sharedSystem.GetConVarManager();
            _modSharp = sharedSystem.GetModSharp();
            _entityManager = sharedSystem.GetEntityManager();
            _transmits = sharedSystem.GetTransmitManager();
            _clientManager = sharedSystem.GetClientManager();
            _hooks = sharedSystem.GetHookManager();
        }
        private readonly ISharpModuleManager _modules;
        private readonly IConVarManager _convars;
        public static IModSharp? _modSharp;
        public static IEntityManager? _entityManager;
        public static ITransmitManager? _transmits;
        private readonly IClientManager _clientManager;
        private readonly IHookManager _hooks;

        public static bool g_bMethod = false;
        public static HUD[] g_HUD = new HUD[PlayerSlot.MaxPlayerCount];
        private IConVar? g_cvar_method;

        public bool Init()
        {
            for (int i = 0; i < g_HUD.Length; i++) g_HUD[i] = new HUD();
            g_cvar_method = _convars.CreateConVar("ms_gamehud_method", false, "true - point_orient, false - teleport", ConVarFlags.Notify);
            if (g_cvar_method != null) _convars.InstallChangeHook(g_cvar_method, OnCvarMethodChanged);

            _modSharp!.InstallGameListener(this);
            _modSharp!.InstallGameFrameHook(null, OnGameFramePost);

            _clientManager.InstallClientListener(this);

            _hooks.PlayerSpawnPost.InstallForward(OnPlayerSpawn);
            _hooks.PlayerKilledPre.InstallForward(OnPlayerKilled);

            return true;
        }

        public void PostInit()
        {
            _modules.RegisterSharpModuleInterface<IGameHUDAPI>(this, IGameHUDAPI.Identity, this);

            _modSharp!.PushTimer(OnTransmit, 0.1, GameTimerFlags.Repeatable);
        }

        public void Shutdown()
        {
            _modSharp!.RemoveGameListener(this);
            _modSharp!.RemoveGameFrameHook(null, OnGameFramePost);

            _clientManager.RemoveClientListener(this);

            _hooks.PlayerSpawnPost.RemoveForward(OnPlayerSpawn);
            _hooks.PlayerKilledPre.RemoveForward(OnPlayerKilled);

            if (g_cvar_method != null) _convars.RemoveChangeHook(g_cvar_method, OnCvarMethodChanged);

            foreach (HUD hud in g_HUD)
            {
                hud.RemoveAllHUD();
                hud.RemovePointOrient();
            }
        }

        public void OnClientPutInServer(IGameClient client)
        {
            g_HUD[client.Slot].SetHUDPlayer(client);
        }

        public void OnClientDisconnected(IGameClient client, NetworkDisconnectionReason reason)
        {
            g_HUD[client.Slot].RemoveAllHUD();
            g_HUD[client.Slot].RemovePointOrient();
            g_HUD[client.Slot].SetHUDPlayer(null);
        }

        private void OnPlayerSpawn(IPlayerSpawnForwardParams @params)
        {
            UpdateEvent(@params.Client);
        }

        private void OnPlayerKilled(IPlayerKilledForwardParams @params)
        {
            UpdateEvent(@params.Client);
        }

        public void OnRoundRestarted()
        {
            _modSharp!.PushTimer(() =>
            {
                foreach (var client in _clientManager.GetGameClients(true)) UpdateEvent(client);
            }, 1.0f);
        }

        private void OnGameFramePost(bool simulating, bool firstTick, bool lastTick)
        {
            if (g_bMethod) return;
            foreach (var hud in g_HUD) hud.ShowAllHUD();
        }

        private void OnTransmit()
        {
            foreach (var player in _entityManager!.GetPlayerControllers().ToArray())
            {
                for (int i = 0; i < g_HUD.Length; i++)
                {
                    if (player.PlayerSlot != i)
                        foreach (var channel in g_HUD[i].Channel)
                            if (channel.Value.WTTransmitValid()) _transmits!.SetEntityState(channel.Value.WTGetIndex(), player.Index, false, -1);
                }
            }
        }

        private static void UpdateEvent(IGameClient? client)
        {
            if (client != null && client.IsValid)
            {
                foreach (var pair in g_HUD[client.Slot].Channel)
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

        int IGameListener.ListenerVersion => IGameListener.ApiVersion;
        int IGameListener.ListenerPriority => 0;
        int IClientListener.ListenerVersion => IClientListener.ApiVersion;
        int IClientListener.ListenerPriority => 0;
    }
}