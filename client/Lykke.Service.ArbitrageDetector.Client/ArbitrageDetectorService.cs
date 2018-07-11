﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Client.Api;
using Lykke.Service.ArbitrageDetector.Client.Models;
using Microsoft.Extensions.PlatformAbstractions;
using Refit;

namespace Lykke.Service.ArbitrageDetector.Client
{
    /// <summary>
    /// Contains methods for work with arbitrage detector service.
    /// </summary>
    public sealed class ArbitrageDetectorService : IArbitrageDetectorService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly IArbitrageDetectorApi _arbitrageDetectorApi;
        private readonly ApiRunner _runner;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="url"></param>
        public ArbitrageDetectorService(string url) : this(new ArbitrageDetectorServiceClientSettings(url))
        {
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="settings"></param>
        public ArbitrageDetectorService(ArbitrageDetectorServiceClientSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (string.IsNullOrEmpty(settings.ServiceUrl))
                throw new ArgumentException("Service URL Required");

            _httpClient = new HttpClient
            { 
                BaseAddress = new Uri(settings.ServiceUrl),
                DefaultRequestHeaders =
                {
                    {
                        "User-Agent",
                        $"{PlatformServices.Default.Application.ApplicationName}/{PlatformServices.Default.Application.ApplicationVersion}"
                    }
                }
            };

            _arbitrageDetectorApi = RestService.For<IArbitrageDetectorApi>(_httpClient);

            _runner = new ApiRunner();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<OrderBookRow>> OrderBooksAsync(string exchange, string assetPair)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.OrderBooksAsync(exchange, assetPair));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<OrderBookRow>> NewOrderBooksAsync(string exchange, string assetPair)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.NewOrderBooksAsync(exchange, assetPair));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CrossRateRow>> CrossRatesAsync()
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.CrossRatesAsync());
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ArbitrageRow>> ArbitragesAsync()
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.ArbitragesAsync());
        }

        /// <inheritdoc />
        public async Task<Arbitrage> ArbitrageFromHistoryAsync(string conversionPath)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.ArbitrageFromHistoryAsync(conversionPath));
        }

        /// <inheritdoc />
        public async Task<Arbitrage> ArbitrageFromActiveOrHistoryAsync(string conversionPath)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.ArbitrageFromActiveOrHistoryAsync(conversionPath));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ArbitrageRow>> ArbitrageHistoryAsync(DateTime since, int take)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.ArbitrageHistory(since, take));
        }

        /// <inheritdoc />
        public async Task<Matrix> MatrixAsync(string assetPair)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.Matrix(assetPair));
        }

        /// <inheritdoc />
        public async Task<Matrix> PublicMatrixAsync(string assetPair)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.PublicMatrix(assetPair));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> PublicMatrixAssetPairsAsync()
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.PublicMatrixAssetPairs());
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LykkeArbitrageRow>> LykkeArbitragesAsync(string basePair, string crossPair)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.LykkeArbitrages(basePair, crossPair));
        }

        /// <inheritdoc />
        public async Task<Settings> GetSettingsAsync()
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.GetSettings());
        }

        /// <inheritdoc />
        public async Task SetSettingsAsync(Settings settings)
        {
            await _runner.RunAsync(() => _arbitrageDetectorApi.SetSettings(settings));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DateTime>> MatrixSnapshotStampsByDate(string assetPair, DateTime date)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.MatrixSnapshotStampsByDate(assetPair, date));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<DateTime>> MatrixSnapshotStampsByDateTimeRange(string assetPair, DateTime from, DateTime to)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.MatrixSnapshotStampsByDateTimeRange(assetPair, from, to));
        }

        /// <inheritdoc />
        public async Task<Matrix> MatrixSnapshot(string assetPair, DateTime dateTime)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.MatrixSnapshot(assetPair, dateTime));
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> MatrixSnapshotAssetPairs(DateTime date)
        {
            return await _runner.RunAsync(() => _arbitrageDetectorApi.MatrixSnapshotAssetPairs(date));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
