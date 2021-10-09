#nullable enable

using Newtonsoft.Json;

namespace DevToys.MonacoEditor.Monaco
{
    /// <summary>
    /// Uniform Resource Identifier (Uri) http://tools.ietf.org/html/rfc3986.
    /// This class is a simple parser which creates the basic component parts
    /// (http://tools.ietf.org/html/rfc3986#section-3) with minimal validation
    /// and encoding.///       foo://example.com:8042/over/there?name=ferret#nose
    ///       \_/   \______________/\_________/ \_________/ \__/
    ///        |           |            |            |        |
    ///     scheme     authority       path        query   fragment
    ///        |   _____________________|__
    ///       / \ /                        \
    ///       urn:example:animal:ferret:nose
    /// 
    /// </summary>
    public sealed class Uri : IUriComponents
    {
        /// <summary>
        /// authority is the 'www.msft.com' part of 'http://www.msft.com/some/path?query#fragment'.
        /// The part between the first double slashes and the next slash.
        /// </summary>
        [JsonProperty("authority", NullValueHandling = NullValueHandling.Ignore)]
        public string? Authority { get; set; }

        /// <summary>
        /// fragment is the 'fragment' part of 'http://www.msft.com/some/path?query#fragment'.
        /// </summary>
        [JsonProperty("fragment", NullValueHandling = NullValueHandling.Ignore)]
        public string? Fragment { get; set; }

        /// <summary>
        /// path is the '/some/path' part of 'http://www.msft.com/some/path?query#fragment'.
        /// </summary>
        [JsonProperty("path", NullValueHandling = NullValueHandling.Ignore)]
        public string? Path { get; set; }

        /// <summary>
        /// query is the 'query' part of 'http://www.msft.com/some/path?query#fragment'.
        /// </summary>
        [JsonProperty("query", NullValueHandling = NullValueHandling.Ignore)]
        public string? Query { get; set; }

        /// <summary>
        /// scheme is the 'http' part of 'http://www.msft.com/some/path?query#fragment'.
        /// The part before the first colon.
        /// </summary>
        [JsonProperty("scheme", NullValueHandling = NullValueHandling.Ignore)]
        public string? Scheme { get; set; }
    }
}
