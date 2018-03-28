﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Client.Models;
using Refit;

namespace Lykke.Service.ArbitrageDetector.Client.Api
{
    internal interface IArbitrageDetectorApi
    {
        [Get("/orderBooks")]
        Task<IReadOnlyList<OrderBook>> OrderBooksAsync(string exchange, string instrument);

        [Get("/crossRates")]
        Task<IReadOnlyList<CrossRate>> CrossRatesAsync();

        [Get("/arbitrages")]
        Task<IReadOnlyList<Arbitrage>> ArbitragesAsync();

        [Get("/arbitrageHistory")]
        Task<IReadOnlyList<Arbitrage>> ArbitrageHistory(DateTime since, int take);

        [Get("/getSettings")]
        Task<Settings> GetSettings();

        [Post("/setSettings")]
        Task SetSettings(Settings settings);
    }
}
