using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// <see cref="PaginationResult{TItem}"/> factory
    /// </summary>
    [PublicAPI]
    public static class PaginationResult
    {
        /// <summary>
        /// Creates <see cref="PaginationResult{TItem}"/> from the given 
        /// <paramref name="continuation"/> and <paramref name="items"/>
        /// </summary>
        public static PaginationResult<TItem> From<TItem>(string continuation, IReadOnlyList<TItem> items)
        {
            return new PaginationResult<TItem>(continuation, items);
        }
    }

    /// <summary>
    /// Generic pagination result
    /// </summary>
    [PublicAPI]
    public class PaginationResult<TItem>
    {
        /// <summary>
        /// Continuation token, that
        /// can be used to continue data reading
        /// from the current position.
        /// Is null if no more data to read.
        /// </summary>
        public string Continuation { get; }

        /// <summary>
        /// Current batch of the items.
        /// Should be empty array if there are no items
        /// </summary>
        public IReadOnlyList<TItem> Items { get; }

        public bool HasMoreItems => Continuation != null;

        public PaginationResult(string continuation, IReadOnlyList<TItem> items)
        {
            Continuation = string.IsNullOrEmpty(continuation) ? null : continuation;
            Items = items ?? throw new ResultValidationException("Items should be not empty array");
        }
    }
}
