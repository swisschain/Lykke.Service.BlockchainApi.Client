using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract.Common
{
    /// <summary>
    /// Blockchain capabilities contract.
    /// Response for the:
    /// 	[GET] /api/capabilities
    /// </summary>
    [PublicAPI]
    public class CapabilitiesResponse
    {
        /// <summary>
        /// Should be true, if [PUT] /api/transactions call
        /// is supported
        /// </summary>
        [CanBeNull]
        [JsonProperty("isTransactionsRebuildingSupported")]
        public bool? IsTransactionsRebuildingSupported { get; set; }

        /// <summary>
        /// Should be true if 
        /// [POST] /api/transactions/many-inputs and
        /// [GET] /api/transactions/broadcasted/many-inputs calls
        /// are supported
        /// </summary>
        [CanBeNull]
        [JsonProperty("areManyInputsSupported")]
        public bool? AreManyInputsSupported { get; set; }

        /// <summary>
        /// Should be true if 
        /// [POST] /api/transactions/many-outputs and
        /// [GET] /api/transactions/broadcasted/many-outputs calls
        /// are supported
        /// </summary>
        [CanBeNull]
        [JsonProperty("areManyOutputsSupported")]
        public bool? AreManyOutputsSupported { get; set; }

        /// <summary>
        /// Should be true if 
        /// [POST] /api/testing/transfers call is supported.
        /// </summary>
        [CanBeNull]
        [JsonProperty("isTestingTransfersSupported")]
        public bool? IsTestingTransfersSupported { get; set; }

        /// <summary>
        /// If blockchain requires additional field to represent
        /// public address to use it as a deposit destination, 
        /// then this flag should be true.
        /// publicAddressExtension section in the 
        /// [GET] /api/constants response should be non empty,
        /// if this flag is true.
        /// For example: Address Tag in the Ripple.
        /// </summary>
        [CanBeNull]
        [JsonProperty("isPublicAddressExtensionRequired")]
        public bool? IsPublicAddressExtensionRequired { get; set; }

        /// <summary>
        /// If blockchain requires broadcasting of the “receive”
        /// transaction in order to accomplish funds transferring
        /// to the destination address, then this flag should be
        /// true and [POST] /api/transactions/single/receive
        /// endpoint should be implemented.
        /// </summary>
        [CanBeNull]
        [JsonProperty("isReceiveTransactionRequired")]
        public bool? IsReceiveTransactionRequired { get; set; }

        /// <summary>
        /// Should be true if
        /// [GET] /api/addresses/{address}/explorer-url
        /// call is supported.
        /// </summary>
        [CanBeNull]
        [JsonProperty("canReturnExplorerUrl")]
        public bool? CanReturnExplorerUrl { get; set; }
    }
}
