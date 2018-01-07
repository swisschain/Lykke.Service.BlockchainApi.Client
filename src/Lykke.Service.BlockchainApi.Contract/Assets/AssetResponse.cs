using JetBrains.Annotations;

namespace Lykke.Service.BlockchainApi.Contract.Assets
{
    /// <summary>
    /// Blockchain asset.
    /// Response for the:
    /// - [GET] /api/assets/{assetId}
    ///     Errors:
    ///         - 204 No Content: specified asset not found
    /// </summary>
    [PublicAPI]
    public class AssetResponse : AssetContract
    {
    }
}
