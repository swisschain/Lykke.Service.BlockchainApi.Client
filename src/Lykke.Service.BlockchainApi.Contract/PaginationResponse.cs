using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Lykke.Service.BlockchainApi.Contract
{
    /// <summary>
    /// <see cref="PaginationResponse{TItem}"/> factory
    /// </summary>
    [PublicAPI]
    public static class PaginationResponse
    {
        /// <summary>
        /// Creates <see cref="PaginationResponse{TItem}"/> from the given 
        /// <paramref name="continuation"/> and <paramref name="items"/>
        /// </summary>
        public static PaginationResponse<TItem> From<TItem>(string continuation, IReadOnlyList<TItem> items)
        {
            return new PaginationResponse<TItem>
            {
                Continuation = continuation,
                Items = items
            };
        }
    }

    /// <summary>
    /// Generic pagination response
    /// </summary>
    [PublicAPI]
    public class PaginationResponse<TItem>
    {
        /// <summary>
        /// Continuation token, that
        /// can be used to continue data reading
        /// from the current position.
        /// Should be null or empty string if no more data
        /// to read.
        /// </summary>
        [JsonProperty("continuation")]
        public string Continuation { get; set; }

        /// <summary>
        /// Current batch of the items.
        /// Should be empty array if there are no items
        /// </summary>
        [JsonProperty("items")]
        public IReadOnlyList<TItem> Items { get; set; }
    }
}
