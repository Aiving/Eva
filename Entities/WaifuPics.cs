using Newtonsoft.Json;
using System.Collections.Generic;


namespace Eva.Entities
{
    public class WaifuPics
    {
        [JsonProperty("NSFW")]
        public List<string> NSFW { get; } = new List<string>();

        [JsonProperty("SFW")]
        public List<string> SFW { get; } = new List<string>();
    }
}
