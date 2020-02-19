using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract
{
    public class IsAliveResponse
    {

        [JsonProperty("name")]
        public string Name
        {
            get;
            set;
        }

        [JsonProperty("version")]
        public string Version
        {
            get;
            set;
        }

        [JsonProperty("env")]
        public string Env
        {
            get;
            set;
        }

        [JsonProperty("isDebug")]
        public bool IsDebug { get; set; }
    }
}
