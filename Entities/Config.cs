using Newtonsoft.Json;
using System.Collections.Generic;

namespace Eva.Entities
{

    public class Config
    {
        [JsonProperty("Token")]
        public string Token { get; set; }

        [JsonProperty("Owners")]
        public List<ulong> Owners { get; } = new List<ulong>();

        [JsonProperty("WaifuPics")]
        public WaifuPics WaifuPics { get; set; }
    }

}
