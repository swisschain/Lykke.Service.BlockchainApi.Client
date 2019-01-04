using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.BlockchainApi.Sdk.Models
{
    public class CreateAssetRequest
    {
        [Required]     public string AssetId  { get; set; }
                       public string Address  { get; set; }
        [Required]     public string Name     { get; set; }
        [Range(0, 28)] public int    Accuracy { get; set; }
    }
}