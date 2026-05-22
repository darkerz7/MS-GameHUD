using Sharp.Shared.GameEntities;
using Sharp.Shared.Objects;
using Sharp.Shared.Types;
using static MS_GameHUD_Shared.IGameHUDAPI;

namespace MS_GameHUD
{
    public class HUDChannel(IGameClient client)
    {
        Vector Position = new(0, 0, 7);
        Vector CurrentPosition = new();
        Vector CurrentAngle = new();

        Color32 Color = new(255, 255, 255, 255);
        int FontSize = 18;
        string FontName = "Verdana";
        float WorldUnitsPerPx = 0.25f;
        PointWorldTextJustifyHorizontal_t JustifyHorizontal = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT;
        PointWorldTextJustifyVertical_t JustifyVertical = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_TOP;
        PointWorldTextReorientMode_t ReorientMode = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE;
        float BackgroundBorderHeight = 0.0f;
        float BackgroundBorderWidth = 0.0f;

        Guid? timer;
        IWorldText? WorldText;
        string Message = "";
        IGameClient HUDClient = client;

        public bool Show(string MessageText, float fTime = 1.0f)
        {
            if (MessageText == null || fTime <= 0.0f) return false;

            CloseTimer();

            if (!WTIsValid()) CreateHUD();
            if (WTIsValid())
            {
                Message = MessageText;
                WorldText!.AcceptInput("SetMessage", null, null, MessageText);
                
                timer = GameHUD._modSharp!.PushTimer(OnTimer, fTime);
            }

            return true;
        }

        public bool ShowPermanent(string MessageText)
        {
            if (MessageText == null) return false;

            CloseTimer();

            if (!WTIsValid()) CreateHUD();
            if (WTIsValid())
            {
                Message = MessageText;
                WorldText!.AcceptInput("SetMessage", null, null, MessageText);
            }

            return true;
        }

        public void Params(Vector vec, Color32 color, int fontsize = 18, string fontname = "Verdana", float units = 0.25f, PointWorldTextJustifyHorizontal_t JH = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT, PointWorldTextJustifyVertical_t JV = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_TOP, PointWorldTextReorientMode_t RM = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE, float BGBH = 0.0f, float BGBW = 0.0f)
        {
            Position = vec;
            Color = color;
            FontSize = fontsize;
            FontName = fontname;
            WorldUnitsPerPx = units;
            JustifyHorizontal = JH;
            JustifyVertical = JV;
            ReorientMode = RM;
            BackgroundBorderHeight = BGBH;
            BackgroundBorderWidth = BGBW;
            CreateHUD();
        }

        public void UpdateParams(Vector vec, Color32 color, int fontsize = 18, string fontname = "Verdana", float units = 0.25f, PointWorldTextJustifyHorizontal_t JH = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT, PointWorldTextJustifyVertical_t JV = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_TOP, PointWorldTextReorientMode_t RM = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE, float BGBH = 0.0f, float BGBW = 0.0f)
        {
            if (IsValidChannel())
            {
                SetPosition(vec);
                SetColor(color);
                SetFontSize(fontsize);
                SetFontName(fontname);
                SetWorldUnitsPerPx(units);
                SetJustifyHorizontal(JH);
                SetJustifyVertical(JV);
                SetReorientMode(RM);
                SetBackgroundBorder(BGBH, BGBW);
            }
        }

        public bool CreateHUD()
        {
            WTRemove();
            WorldText = null;

            if (HUDClient == null || !HUDClient.IsValid || HUDClient.GetPlayerController() is not { } HUDPlayer || !HUDPlayer.IsValid() || HUDPlayer.GetPawn() is not { } pawn) return false;

            if (GameHUD.g_bMethod && !GameHUD.g_HUD[HUDPlayer.PlayerSlot].CreateOrGetPointOrient()) return false;

            var kv = new Dictionary<string, KeyValuesVariantValueItem>
            {
                {"font_size", FontSize},
                {"font_name", FontName},
                {"enabled", true},
                {"fullbright", true},
                {"world_units_per_pixel", WorldUnitsPerPx},
                {"color", $"{Color.R} {Color.G} {Color.B} {Color.A}"},
                {"message", ""},
                {"justify_horizontal", (uint)JustifyHorizontal},
                {"justify_vertical", (uint)JustifyVertical},
                {"reorient_mode", (uint)ReorientMode},
                {"background_border_height", BackgroundBorderHeight},
                {"background_border_width", BackgroundBorderWidth},
                {"draw_background", BackgroundBorderHeight != 0.0f || BackgroundBorderWidth != 0.0f}
            };

            if (GameHUD._entityManager!.SpawnEntitySync<IWorldText>("point_worldtext", kv) is { } entity)
            {
                entity.DispatchSpawn();

                if (GameHUD.g_bMethod)
                {
                    entity.AcceptInput("SetParent", GameHUD.g_HUD[HUDPlayer.PlayerSlot].GetPointOrient(), null, "!activator");
                    GetPositionOrient();
                    entity.SetAbsOrigin(CurrentPosition);
                    entity.SetAbsAngles(CurrentAngle);
                }
                else
                {
                    entity.AcceptInput("SetParent", pawn, null, "!activator");
                    GetPositionTeleport();
                    entity.SetAbsOrigin(CurrentPosition);
                    entity.SetAbsAngles(CurrentAngle);
                }

                WorldText = entity;
                GameHUD._transmits!.AddEntityHooks(WorldText, true);
                WorldText.AcceptInput("SetMessage", null, null, Message);

                return true;
            }
            return false;
        }

        public void ShowHUD()
        {
            if (!WTIsValid() || EmptyMessage()) return;
            GetPositionTeleport();
            WorldText!.SetAbsOrigin(CurrentPosition);
            WorldText!.SetAbsAngles(CurrentAngle);
        }

        public void SetKeyValue(string key, string value)
        {
            if (WTIsValid())
            {
                switch (key)
                {
                    case "font_size": { if (IsValidChannel() && Int32.TryParse(value, out int val)) SetFontSize(val); return; }
                    case "font_name": { if (IsValidChannel() && !string.IsNullOrEmpty(value)) SetFontName(value); return; }
                    case "world_units_per_pixel": { if (IsValidChannel() && float.TryParse(value, out float val)) SetWorldUnitsPerPx(val); return; }
                    case "justify_horizontal": { if (IsValidChannel() && uint.TryParse(value, out uint val)) SetJustifyHorizontal((PointWorldTextJustifyHorizontal_t)val); return; }
                    case "justify_vertical": { if (IsValidChannel() && uint.TryParse(value, out uint val)) SetJustifyVertical((PointWorldTextJustifyVertical_t)val); return; }
                    case "reorient_mode": { if (IsValidChannel() && uint.TryParse(value, out uint val)) SetReorientMode((PointWorldTextReorientMode_t)val); return; }
                    case "background_border_height": { if (IsValidChannel() && float.TryParse(value, out float val)) SetBackgroundBorder(val, BackgroundBorderWidth); return; }
                    case "background_border_width": { if (IsValidChannel() && float.TryParse(value, out float val)) SetBackgroundBorder(BackgroundBorderHeight, val); return; }
                }
            }
        }

        public string? GetKeyValue(string key)
        {
            if (WTIsValid())
            {
                switch (key)
                {
                    case "font_size": { return WorldText!.FontSize.ToString(); }
                    case "font_name": { return WorldText!.GetNetVar<string>("m_FontName"); }
                    case "world_units_per_pixel": { return WorldText!.GetNetVar<float>("m_flWorldUnitsPerPx").ToString(); }
                    case "justify_horizontal": { return WorldText!.GetNetVar<uint>("m_nJustifyHorizontal").ToString(); }
                    case "justify_vertical": { return WorldText!.GetNetVar<uint>("m_nJustifyVertical").ToString(); }
                    case "reorient_mode": { return WorldText!.GetNetVar<uint>("m_nReorientMode").ToString(); }
                    case "background_border_height": { return WorldText!.BackgroundBorderHeight.ToString(); }
                    case "background_border_width": { return WorldText!.BackgroundBorderWidth.ToString(); }
                    case "enabled": { return WorldText!.Enabled ? "1" : "0"; }
                    case "fullbright": { return WorldText!.FullBright ? "1" : "0"; }
                    case "color": { return $"{WorldText!.Color.R} {WorldText!.Color.G} {WorldText!.Color.B} {WorldText!.Color.A}"; }
                    case "message": { return WorldText!.Message; }
                    case "draw_background": { return WorldText!.DrawBackground ? "1" : "0"; }
                }
            }
            return null; 
        }

        public bool EmptyMessage()
        {
            if (string.Equals(Message, "")) return true;
            return false;
        }

        public bool WTIsValid()
        {
            if (WorldText == null || !WorldText.IsValid()) return false;
            return true;
        }

        public Sharp.Shared.Units.EntityIndex WTGetIndex()
        {
            return WorldText!.Index;
        }

        public void WTRemove()
        {
            if (WTIsValid()) WorldText!.Kill();
        }

        public bool WTTransmitValid()
        {
            return WTIsValid() && GameHUD._transmits!.IsEntityHooked(WorldText!);
        }

        bool IsValidChannel()
        {
            return WTIsValid() && HUDClient != null && HUDClient.IsValid;
        }

        void SetPosition(Vector vec)
        {
            if (Position.X != vec.X || Position.Y != vec.Y || Position.Z != vec.Z)
            {
                Position = vec;
                if (GameHUD.g_bMethod) GetPositionOrient();
                else GetPositionTeleport();
                WorldText!.SetAbsOrigin(CurrentPosition);
                WorldText!.SetAbsAngles(CurrentAngle);
            }
        }

        void SetColor(Color32 color)
        {
            if (Color != color)
            {
                Color = color;
                WorldText!.Color = Color;
            }
        }

        void SetFontSize(int fontsize = 18)
        {
            if (FontSize != fontsize)
            {
                FontSize = fontsize;
                WorldText!.FontSize = FontSize;
            }
        }

        void SetFontName(string fontname = "Verdana")
        {
            if (FontName != fontname)
            {
                FontName = fontname;
                WorldText!.SetNetVar("m_FontName", FontName, FontName.Length);
            }
        }

        void SetWorldUnitsPerPx(float units = 0.25f)
        {
            if (WorldUnitsPerPx != units)
            {
                WorldUnitsPerPx = units;
                WorldText!.SetNetVar("m_flWorldUnitsPerPx", WorldUnitsPerPx);
            }
        }

        void SetJustifyHorizontal(PointWorldTextJustifyHorizontal_t JH = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT)
        {
            if (JustifyHorizontal != JH)
            {
                JustifyHorizontal = JH;
                WorldText!.SetNetVar("m_nJustifyHorizontal", (uint)JustifyHorizontal);
            }
        }

        void SetJustifyVertical(PointWorldTextJustifyVertical_t JV = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_TOP)
        {
            if (JustifyVertical != JV)
            {
                JustifyVertical = JV;
                WorldText!.SetNetVar("m_nJustifyVertical", (uint)JustifyVertical);
            }
        }

        void SetReorientMode(PointWorldTextReorientMode_t RM = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE)
        {
            if (ReorientMode != RM)
            {
                ReorientMode = RM;
                WorldText!.SetNetVar("m_nReorientMode", (uint)ReorientMode);
            }
        }

        void SetBackgroundBorder(float BGBH = 0.0f, float BGBW = 0.0f)
        {
            if (BackgroundBorderHeight != BGBH)
            {
                BackgroundBorderHeight = BGBH;
                WorldText!.BackgroundBorderHeight = BackgroundBorderHeight;
            }
            if (BackgroundBorderWidth != BGBW)
            {
                BackgroundBorderWidth = BGBW;
                WorldText!.BackgroundBorderWidth = BackgroundBorderWidth;
            }
            WorldText!.DrawBackground = BackgroundBorderHeight != 0.0f || BackgroundBorderWidth != 0.0f;
        }

        void GetPositionTeleport()
        {
            if (HUDClient.GetPlayerController() is not { } HUDPlayer || !HUDPlayer.IsValid() || HUDPlayer.GetPawn() is not { } pawn) return;
            Vector Angle = pawn.GetNetVar<Vector>("v_angle");
            AngleVectors(Angle, out Vector vecForward, out Vector vecRight, out Vector vecUp);

            CurrentPosition = pawn.GetAbsOrigin();
            CurrentPosition += vecForward * Position.Z;
            CurrentPosition += vecRight * Position.X;
            CurrentPosition += vecUp * Position.Y;
            CurrentPosition += new Vector(0, 0, pawn.ViewOffset.Z);

            CurrentAngle.Y = Angle.Y + 270;
            CurrentAngle.Z = 90 - Angle.X;
        }
        void GetPositionOrient()
        {
            IBaseEntity? pointorient = GameHUD.g_HUD[HUDClient.Slot].GetPointOrient();
            if (pointorient == null || !pointorient.IsValid()) return;
            Vector Angle = pointorient!.GetAbsAngles();
            AngleVectors(Angle, out Vector vecForward, out Vector vecRight, out Vector vecUp);

            CurrentPosition = pointorient!.GetAbsOrigin();
            CurrentPosition += vecForward * Position.Z;
            CurrentPosition += vecRight * Position.X;
            CurrentPosition += vecUp * Position.Y;

            CurrentAngle.Y = Angle.Y + 270;
            CurrentAngle.Z = 90 - Angle.X;
        }

        static void AngleVectors(Vector angles, out Vector forward, out Vector right, out Vector up)
        {
            (float sy, float cy) = MathF.SinCos(angles.Y * MathF.PI / 180.0f);
            (float sp, float cp) = MathF.SinCos(angles.X * MathF.PI / 180.0f);

            forward = new(cp * cy, cp * sy, -sp);
            right = new(sy, -cy, 0);
            up = new(sp * cy, sp * sy, cp);
        }

        void OnTimer()
        {
            CloseTimer();
            Message = "";
            if (WTIsValid()) WorldText!.AcceptInput("SetMessage", null, null, "");
        }

        void CloseTimer()
        {
            if (timer != null)
            {
                GameHUD._modSharp!.StopTimer((Guid)timer);
                timer = null;
            }
        }
    }
}
