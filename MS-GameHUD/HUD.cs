using Sharp.Shared.GameEntities;
using Sharp.Shared.Types;

namespace MS_GameHUD
{
    public class HUD
    {
        IPlayerController? HUDPlayer;
        public Dictionary<byte, HUDChannel> Channel = [];
        IBaseEntity? PointOrient;

        public void SetHUDPlayer(IPlayerController? player)
        {
            HUDPlayer = player;
        }

        public IPlayerController? GetHUDPlayer()
        {
            return HUDPlayer;
        }

        public bool CreateOrGetPointOrient()
        {
            if (PointOrient != null && PointOrient.IsValid()) return true;

            if (HUDPlayer == null || !HUDPlayer.IsValid()) return false;
            var pawn = HUDPlayer.GetPawn();

            var kv = new Dictionary<string, KeyValuesVariantValueItem>
            {
                {"start_active", true},
                {"goal_direction_type", (uint)0x4} //eEyesForward
            };

            if (GameHUD._entityManager!.SpawnEntitySync<IBaseEntity>("point_orient", kv) is { } entOrient)
            {
                entOrient.DispatchSpawn();

                Vector vecPos = pawn!.GetAbsOrigin() with { Z = pawn.GetAbsOrigin().Z + pawn.ViewOffset.Z };
                entOrient.SetAbsOrigin(vecPos);
                entOrient.AcceptInput("SetParent", pawn, null, "!activator");
                entOrient.AcceptInput("SetTarget", pawn, null, "!activator");
                //entOrient.AcceptInput("SetParentAttachmentMaintainOffset", pawn, null, "look_straight_ahead_stand");

                PointOrient = entOrient;

                return true;
            }
            return false;
        }

        public void RemovePointOrient()
        {
            if (PointOrient != null && PointOrient.IsValid()) PointOrient.Kill();
            PointOrient = null;
        }

        public IBaseEntity? GetPointOrient()
        {
            return PointOrient;
        }

        public HUDChannel? CreateorGetChannel(byte channel)
        {
            if (HUDPlayer != null && HUDPlayer.IsValid() && (Channel.ContainsKey(channel) || Channel.TryAdd(channel, new HUDChannel(HUDPlayer)))) return Channel[channel];
            return null;
        }

        public void RemoveChannel(byte channel)
        {
            Channel.TryGetValue(channel, out HUDChannel? chnl);
            chnl?.WTRemove();
            Channel.Remove(channel);
        }

        public void ShowAllHUD()
        {
            if (HUDPlayer != null && HUDPlayer.IsValid())
                foreach (var pair in Channel) pair.Value.ShowHUD();
        }

        public void RemoveAllHUD()
        {
            foreach (var pair in Channel) pair.Value.WTRemove();
            Channel.Clear();
        }
    }
}
