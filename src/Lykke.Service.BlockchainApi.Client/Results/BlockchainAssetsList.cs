using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Responses;

namespace Lykke.Service.BlockchainApi.Client.Results
{
    [PublicAPI]
    public class BlockchainAssetsList
    {
        public IReadOnlyList<BlockchainAsset> Assets { get; }

        public BlockchainAssetsList(AssetsListResponse apiResponse)
        {
            Assets = apiResponse?.Assets == null
                ? Array.Empty<BlockchainAsset>()
                : apiResponse.Assets
                    .Select(a => new BlockchainAsset(a))
                    .ToArray();
        }
    }
}
