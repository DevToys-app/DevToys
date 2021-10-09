#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco.Editor
{
    public sealed class EditorScrollbarOptions
    {
        /// <summary>
        /// Always consume mouse wheel events (always call preventDefault() and stopPropagation() on
        /// the browser events).
        /// Defaults to true.
        /// </summary>
        [JsonProperty("alwaysConsumeMouseWheel", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AlwaysConsumeMouseWheel { get; set; }

        /// <summary>
        /// The size of arrows (if displayed).
        /// Defaults to 11.
        /// </summary>
        [JsonProperty("arrowSize", NullValueHandling = NullValueHandling.Ignore)]
        public int? ArrowSize { get; set; }

        /// <summary>
        /// Listen to mouse wheel events and react to them by scrolling.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("handleMouseWheel", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HandleMouseWheel { get; set; }

        /// <summary>
        /// Render horizontal scrollbar.
        /// Defaults to 'auto'.
        /// </summary>
        [JsonProperty("horizontal", NullValueHandling = NullValueHandling.Ignore)]
        public ScrollbarBehavior? Horizontal { get; set; }

        /// <summary>
        /// Render arrows at the left and right of the horizontal scrollbar.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("horizontalHasArrows", NullValueHandling = NullValueHandling.Ignore)]
        public bool? HorizontalHasArrows { get; set; }

        /// <summary>
        /// Height in pixels for the horizontal scrollbar.
        /// Defaults to 10 (px).
        /// </summary>
        [JsonProperty("horizontalScrollbarSize", NullValueHandling = NullValueHandling.Ignore)]
        public int? HorizontalScrollbarSize { get; set; }

        /// <summary>
        /// Height in pixels for the horizontal slider.
        /// Defaults to `horizontalScrollbarSize`.
        /// </summary>
        [JsonProperty("horizontalSliderSize", NullValueHandling = NullValueHandling.Ignore)]
        public int? HorizontalSliderSize { get; set; }

        /// <summary>
        /// Cast horizontal and vertical shadows when the content is scrolled.
        /// Defaults to true.
        /// </summary>
        [JsonProperty("useShadows", NullValueHandling = NullValueHandling.Ignore)]
        public bool? UseShadows { get; set; }

        /// <summary>
        /// Render vertical scrollbar.
        /// Defaults to 'auto'.
        /// </summary>
        [JsonProperty("vertical", NullValueHandling = NullValueHandling.Ignore)]
        public ScrollbarBehavior? Vertical { get; set; }

        /// <summary>
        /// Render arrows at the top and bottom of the vertical scrollbar.
        /// Defaults to false.
        /// </summary>
        [JsonProperty("verticalHasArrows", NullValueHandling = NullValueHandling.Ignore)]
        public bool? VerticalHasArrows { get; set; }

        /// <summary>
        /// Width in pixels for the vertical scrollbar.
        /// Defaults to 10 (px).
        /// </summary>
        [JsonProperty("verticalScrollbarSize", NullValueHandling = NullValueHandling.Ignore)]
        public int? VerticalScrollbarSize { get; set; }

        /// <summary>
        /// Width in pixels for the vertical slider.
        /// Defaults to `verticalScrollbarSize`.
        /// </summary>
        [JsonProperty("verticalSliderSize", NullValueHandling = NullValueHandling.Ignore)]
        public int? VerticalSliderSize { get; set; }
    }

}
