using System.Collections.Generic;
using System.Diagnostics;

namespace Lykke.Service.BlockchainApi.Client.Models
{
    internal class EnumerationStatisticsBuilder
    {
        private int _itemsCount;
        private int _batchesCount;
        private readonly Stopwatch _stopwatch;

        public EnumerationStatisticsBuilder()
        {
            _stopwatch = Stopwatch.StartNew();
        }

        public void IncludeBatch<T>(IReadOnlyList<T> batch)
        {
            _itemsCount += batch.Count;
            ++_batchesCount;
        }

        public EnumerationStatistics Build()
        {
            _stopwatch.Stop();

            return new EnumerationStatistics(_itemsCount, _batchesCount, _stopwatch.Elapsed);
        }
    }
}
