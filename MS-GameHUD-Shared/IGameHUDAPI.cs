using Sharp.Shared.GameEntities;

namespace MS_GameHUD_Shared
{
    public interface IGameHUDAPI
    {
        const string Identity = nameof(IGameHUDAPI);

        /**
         * Initializes hud with the specified parameters
         *
         * @param Player				IPlayerController for whom the hud will be initialized
         * @param channel				Channel number to initialize
         * @param vec					Vector where the hud will be located relative to the player
         * @param color					Color of hud
         * @param fontsize				Hud font size
         * @param fontname				Hud font name
         * @param units					Hud world units per px
         * @param justifyhorizontal		Horizontal alignment of hud
         * @param justifyvertical		Vertical alignment of hud
         * @param reorientmode			Reorient mode for hud
         * @param bgborderheight		Background border height if needed (to disable both must be equal to 0.0f)
         * @param bgborderwidth			Background border width if needed
         * 
         *
         * On error/errors:				Invalid player, Invalid channel
         */
        void Native_GameHUD_SetParams(IPlayerController Player, byte channel, Sharp.Shared.Types.Vector vec, Sharp.Shared.Types.Color32 color, int fontsize = 18, string fontname = "Verdana", float units = 0.25f, PointWorldTextJustifyHorizontal_t justifyhorizontal = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT, PointWorldTextJustifyVertical_t justifyvertical = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_TOP, PointWorldTextReorientMode_t reorientmode = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE, float bgborderheight = 0.0f, float bgborderwidth = 0.0f);

        /**
		 * Initializes hud with the specified parameters
		 *
		 * @param Player				IPlayerController for whom the hud will be initialized
		 * @param channel				Channel number to initialize
		 * @param vec					Vector where the hud will be located relative to the player
		 * @param color					Color of hud
		 * @param fontsize				Hud font size
		 * @param fontname				Hud font name
		 * @param units					Hud world units per px
		 * @param justifyhorizontal		Horizontal alignment of hud
		 * @param justifyvertical		Vertical alignment of hud
		 * @param reorientmode			Reorient mode for hud
		 * @param bgborderheight		Background border height if needed (to disable both must be equal to 0.0f)
		 * @param bgborderwidth			Background border width if needed
		 * 
		 *
		 * On error/errors:				Invalid player, Invalid channel
		 */
        void Native_GameHUD_UpdateParams(IPlayerController Player, byte channel, Sharp.Shared.Types.Vector vec, Sharp.Shared.Types.Color32 color, int fontsize = 18, string fontname = "Verdana", float units = 0.25f, PointWorldTextJustifyHorizontal_t justifyhorizontal = PointWorldTextJustifyHorizontal_t.POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT, PointWorldTextJustifyVertical_t justifyvertical = PointWorldTextJustifyVertical_t.POINT_WORLD_TEXT_JUSTIFY_VERTICAL_TOP, PointWorldTextReorientMode_t reorientmode = PointWorldTextReorientMode_t.POINT_WORLD_TEXT_REORIENT_NONE, float bgborderheight = 0.0f, float bgborderwidth = 0.0f);

        /**
		 * Shows a message to the player
		 *
		 * @param Player				IPlayerController for whom the message is displayed
		 * @param channel				Channel number to display
		 * @param message				Message to display
		 * @param time					Show time
		 * 
		 *
		 * On error/errors:				Invalid player, Invalid channel
		 */
        void Native_GameHUD_Show(IPlayerController Player, byte channel, string message, float time = 1.0f);

        /**
		 * Shows a message to the player, until another command is used
		 *
		 * @param Player				IPlayerController for whom the message is displayed
		 * @param channel				Channel number to display
		 * @param message				Message to display
		 * 
		 *
		 * On error/errors:				Invalid player, Invalid channel
		 */
        void Native_GameHUD_ShowPermanent(IPlayerController Player, byte channel, string message);

        /**
		 * Deletes the displayed channel
		 *
		 * @param Player				IPlayerController for whom the channel is being deleted
		 * @param channel				Channel number to remove
		 * 
		 *
		 * On error/errors:				Invalid player, Invalid channel
		 */
        void Native_GameHUD_Remove(IPlayerController Player, byte channel);

        /**
		 * Sets a key-value pair on the HUD entity, typically used for further customization.
		 *
		 * @param Player				IPlayerController for whom this request is made
		 * @param channel				Channel number on which the key-value pair is set
		 * @param key					The key to be set
		 * @param value					The value to assign to the key
		 *
		 *
		 * On error/errors:				Invalid player, Invalid channel
		 */
        void Native_GameHUD_SetKeyValue(IPlayerController Player, byte channel, string key, string value);

        /**
		 * Gets a key-value pair from the HUD entity.
		 *
		 * @param Player				IPlayerController for whom this request is made
		 * @param channel				Channel number to query
		 * @param key					The key to retrieve
		 *
		 * @return						The value assigned to the key, or null if not set
		 */
        string? Native_GameHUD_GetKeyValue(IPlayerController Player, byte channel, string key);

        enum PointWorldTextJustifyHorizontal_t : uint
        {
            POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_LEFT = 0x0,
            POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_CENTER = 0x1,
            POINT_WORLD_TEXT_JUSTIFY_HORIZONTAL_RIGHT = 0x2,
        }

        enum PointWorldTextJustifyVertical_t : uint
        {
            POINT_WORLD_TEXT_JUSTIFY_VERTICAL_BOTTOM = 0x0,
            POINT_WORLD_TEXT_JUSTIFY_VERTICAL_CENTER = 0x1,
            POINT_WORLD_TEXT_JUSTIFY_VERTICAL_TOP = 0x2,
        }

        enum PointWorldTextReorientMode_t : uint
        {
            POINT_WORLD_TEXT_REORIENT_NONE = 0x0,
            POINT_WORLD_TEXT_REORIENT_AROUND_UP = 0x1,
        }
    }
}