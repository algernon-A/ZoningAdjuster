using ColossalFramework.UI;


namespace ZoningAdjuster
{
    /// <summary>
    /// Textures.
    /// </summary>
    internal static class Textures
    {
        // RON button icon texture atlas.
        private static UITextureAtlas toolButtonSprites;
        internal static UITextureAtlas ToolButtonSprites
        {
            get
            {
                if (toolButtonSprites == null)
                {
                    toolButtonSprites = TextureUtils.LoadSpriteAtlas("uui_zoning_adjuster");
                }

                return toolButtonSprites;
            }
        }
    }
}