using System.Collections.Generic;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    internal static class PaginationResult
    {
        public static PaginationResult<TItem> From<TItem>(string continuation, IReadOnlyList<TItem> items)
        {
            return new PaginationResult<TItem>(continuation, items);
        }
    }

    internal class PaginationResult<TItem>
    {
        public string Continuation { get; }

        public IReadOnlyList<TItem> Items { get; }

        public bool HasMoreItems => Continuation != null;

        public PaginationResult(string continuation, IReadOnlyList<TItem> items)
        {
            Continuation = string.IsNullOrEmpty(continuation) ? null : continuation;
            Items = items ?? throw new ResultValidationException("Items should be not empty array");
        }
    }
}
