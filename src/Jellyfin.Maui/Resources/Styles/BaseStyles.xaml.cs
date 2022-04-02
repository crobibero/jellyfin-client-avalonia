namespace Jellyfin.Maui.Resources.Styles
{
    /// <summary>
    /// Base resources style helpers.
    /// </summary>
    public static class BaseStyles
    {
        /// <summary>
        /// The header basis for a FlexLayout.
        /// </summary>
        public const int HeaderBasis = 100;

        /// <summary>
        /// Gets the label header style.
        /// </summary>
        public static readonly Style? LabelHeader = Application.Current?.Resources["LabelHeader"] as Style;

        /// <summary>
        /// Gets the frame login card style.
        /// </summary>
        public static readonly Style? FrameLoginCard = Application.Current?.Resources["FrameLoginCard"] as Style;
    }
}
