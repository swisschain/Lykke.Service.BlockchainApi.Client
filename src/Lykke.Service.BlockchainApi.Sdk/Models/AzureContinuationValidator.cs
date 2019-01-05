using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using CommonUtils = Common.Utils;

namespace Lykke.Service.BlockchainApi.Sdk.Models
{
    public static class AzureContinuationValidator
    {
        public static bool IsValid(string value)
        {
            if (value == null)
            {
                return true;
            }

            try
            {
                JsonConvert.DeserializeObject<TableContinuationToken>(CommonUtils.HexToString(value));
                return true;
            }
            catch 
            {
                return false;
            }
        }
    }
}
