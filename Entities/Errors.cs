using Newtonsoft.Json;

namespace Eva.Entities
{
    public class ErrorVariants
    {
        [JsonProperty("Errors")] public string[] Errors;
    }
}
