using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using CommonUtils = Common.Utils;

namespace Lykke.Service.BlockchainApi.Sdk.Models
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
    public class AzureContinuationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }

            try
            {
                JsonConvert.DeserializeObject<TableContinuationToken>(CommonUtils.HexToString((string)value));
                return true;
            }
            catch 
            {
                return false;
            }
        }
    }
}
