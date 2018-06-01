﻿using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Core
{
    public interface ISettings
    {
        int ExecutionDelayInMilliseconds { get; set; }

        int HistoryMaxSize { get; set; }


        int ExpirationTimeInSeconds { get; set; }

        decimal MinimumPnL { get; set; }

        decimal MinimumVolume { get; set; }

        int MinSpread { get; set; }


        IEnumerable<string> BaseAssets { get; set; }

        IEnumerable<string> IntermediateAssets { get; set; }

        string QuoteAsset { get; set; }


        IEnumerable<string> Exchanges { get; set; }


        IEnumerable<string> PublicMatrixAssetPairs { get; set; }

        IDictionary<string, string> PublicMatrixExchanges { get; set; }
    }
}
