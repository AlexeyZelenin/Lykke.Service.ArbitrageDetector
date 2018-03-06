﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Client.Models;
using Refit;

namespace Lykke.Service.ArbitrageDetector.Client.Api
{
    internal interface IArbitrageDetectorApi
    {
        [Get("/api/orderBooks")]
        Task<IReadOnlyList<OrderBook>> GetOrderBooksAsync();

        [Get("orderBooks/exchange/{exchange}")]
        Task<IReadOnlyList<OrderBook>> GetOrderBooksByExchangeAsync(string exchange);

        [Get("orderBooks/instrument/{instrument}")]
        Task<IReadOnlyList<OrderBook>> GetOrderBooksByInstrumentAsync(string instrument);

        [Get("orderBooks/exchange/{exchange}/instrument/{instrument}")]
        Task<IReadOnlyList<OrderBook>> GetOrderBooksAsync(string exchange, string instrument);

        [Get("/api/crossRates")]
        Task<IReadOnlyList<CrossRate>> GetCrossRatesAsync();

        [Get("/api/arbitrages")]
        Task<IReadOnlyList<Arbitrage>> GetArbitragesAsync();
    }
}
