using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Common
{
    /// <summary>
    /// Blockchain capabilities contract.
    /// Response for the:
    /// 	[GET] /api/capabilities
    /// </summary>
    public class CapabilitiesResponse
    {
        /// <summary>
        /// Should be true, if [PUT] /api/transactions call
        /// is supported
        /// </summary>
        [JsonProperty("isTransactionsRebuildingSupported")]
        public bool IsTransactionsRebuildingSupported { get; set; }

        /// <summary>
        /// Should be true if 
        /// [POST] /api/transactions/many-inputs and
        /// [GET] /api/transactions/broadcasted/many-inputs calls
        /// are supported
        /// </summary>
        [JsonProperty("areManyInputsSupported")]
        public bool AreManyInputsSupported { get; set; }

        /// <summary>
        /// Should be true if 
        /// [POST] /api/transactions/many-outputs and
        /// [GET] /api/transactions/broadcasted/many-outputs calls
        /// are supported
        /// </summary>
        [JsonProperty("areManyOutputsSupported")]
        public bool AreManyOutputsSupported { get; set; }

    }
}
