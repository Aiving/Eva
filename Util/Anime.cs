using Newtonsoft.Json;

namespace Eva.Util.Anime
{
    public class WaifuPics
    {
        [JsonProperty("NSFW")] public string[] NSFW { get; set; }
        [JsonProperty("SFW")] public string[] SFW { get; set; }
    }

    public class Configuration
    {
        [JsonProperty("WaifuPics")] public WaifuPics WaifuPics { get; set; }
    }

    public class Picture
    {
        [JsonProperty("url")] public string File { get; set; }
    }
}