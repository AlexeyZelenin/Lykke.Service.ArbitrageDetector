﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Client.Models;

namespace Lykke.Service.ArbitrageDetector.Client
{
    /// <summary>
    /// HTTP client for arbitrage detector service.
    /// </summary>
    public interface IArbitrageDetectorService
    {
        /// <summary>
        /// Returns a collection of OrderBook entities by exchange and instrument.
        /// </summary>
        /// <param name="exchange">Name of an exchange.</param>
        /// <param name="assetPair">Name of an instrument</param>
        /// <returns>A collection of OrderBook entities.</returns>
        Task<IEnumerable<OrderBook>> OrderBooksAsync(string exchange, string assetPair);

        /// <summary>
        /// Returns a collection of CrossRate entities.
        /// </summary>
        /// <returns>A collection of CrossRate entities.</returns>
        Task<IEnumerable<CrossRateRow>> CrossRatesAsync();

        /// <summary> 
        /// Returns a collection of Arbitrage entities.
        /// </summary>
        /// <returns>A collection of Arbitrage entities.</returns>
        Task<IEnumerable<ArbitrageRow>> ArbitragesAsync();

        /// <summary>
        /// Returns best arbitrage by conversion path.
        /// </summary>
        /// <param name="conversionPath"></param>
        /// <returns></returns>
        Task<Arbitrage> ArbitrageAsync(string conversionPath);

        /// <summary>
        /// Returns a collection of ArbitrageHistory entities.
        /// </summary>
        /// <param name="since"></param>
        /// <param name="take"></param>
        /// <returns>A collection of Arbitrage entities.</returns>
        Task<IEnumerable<ArbitrageRow>> ArbitrageHistoryAsync(DateTime since, int take);

        /// <summary>
        /// Get settings.
        /// </summary>
        Task<Settings> GetSettingsAsync();

        /// <summary>
        /// Set new settings.
        /// </summary>
        /// <param name="settings"></param>
        Task SetSettingsAsync(Settings settings);
    }
}
