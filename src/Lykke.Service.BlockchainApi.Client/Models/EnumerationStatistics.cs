using System;
using JetBrains.Annotations;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    /// <summary>
    /// Statistics of the items enumeration
    /// </summary>
    [PublicAPI]
    public class EnumerationStatistics
    {
        /// <summary>
        /// Items count that was enumerated
        /// </summary>
        public int ItemsCount { get; }

        /// <summary>
        /// Batches count of the items count that was enumerated
        /// </summary>
        public int BatchesCount { get; }

        /// <summary>
        /// Time that was elapsed for the enumeration
        /// </summary>
        public TimeSpan Elapsed { get; }

        public EnumerationStatistics(int itemsCount, int batchesCount, TimeSpan elapsed)
        {
            ItemsCount = itemsCount;
            BatchesCount = batchesCount;
            Elapsed = elapsed;
        }
    }
}
