﻿using System.Collections.Generic;
using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.Service.ArbitrageDetector.Core;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories
{
    public class Settings : AzureTableEntity, ISettings
    {
        public int ExecutionDelayInMilliseconds { get; set; }

        public int HistoryMaxSize { get; set; }

        public int ExpirationTimeInSeconds { get; set; }

        public decimal MinimumPnL { get; set; }

        public decimal MinimumVolume { get; set; }

        public int MinSpread { get; set; }

        [JsonValueSerializer]
        public IEnumerable<string> BaseAssets { get; set; }

        [JsonValueSerializer]
        public IEnumerable<string> IntermediateAssets { get; set; }

        public string QuoteAsset { get; set; }

        [JsonValueSerializer]
        public IEnumerable<string> Exchanges { get; set; }

        [JsonValueSerializer]
        public IEnumerable<string> PublicMatrixAssetPairs { get; set; }

        [JsonValueSerializer]
        public IDictionary<string, string> PublicMatrixExchanges { get; set; }

        public Settings()
        {
        }

        public Settings(ISettings domain)
        {
            PartitionKey = "";
            RowKey = "";
            ExecutionDelayInMilliseconds = domain.ExecutionDelayInMilliseconds;
            HistoryMaxSize = domain.HistoryMaxSize;
            ExpirationTimeInSeconds = domain.ExpirationTimeInSeconds;
            MinimumPnL = domain.MinimumPnL;
            MinimumVolume = domain.MinimumVolume;
            MinSpread = domain.MinSpread;
            BaseAssets = domain.BaseAssets;
            IntermediateAssets = domain.IntermediateAssets;
            QuoteAsset = domain.QuoteAsset;
            Exchanges = domain.Exchanges;
            PublicMatrixAssetPairs = domain.PublicMatrixAssetPairs;
            PublicMatrixExchanges = domain.PublicMatrixExchanges;
        }
    }
}
