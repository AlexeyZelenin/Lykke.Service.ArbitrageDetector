﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.ArbitrageDetector.Core;
using Lykke.Service.ArbitrageDetector.Core.Utils;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Services;
using Lykke.Service.ArbitrageDetector.Services.Models;
using MoreLinq;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class ArbitrageDetectorService : TimerPeriod, IArbitrageDetectorService
    {
        private readonly ConcurrentDictionary<AssetPairSource, OrderBook> _orderBooks;
        private readonly ConcurrentDictionary<AssetPairSource, CrossRate> _crossRates;
        private readonly ConcurrentDictionary<string, Arbitrage> _arbitrages;
        private readonly ConcurrentDictionary<string, Arbitrage> _arbitrageHistory;
        private IEnumerable<string> _baseAssets;
        private string _quoteAsset;
        private int _expirationTimeInSeconds;
        private readonly int _historyMaxSize;
        private bool _restartNeeded;
        private int _minSpread;
        
        private readonly ILog _log;

        public ArbitrageDetectorService(StartupSettings settings, ILog log, IShutdownManager shutdownManager)
            : base(settings.ExecutionDelayInMilliseconds, log)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _baseAssets = settings.BaseAssets;
            _quoteAsset = settings.QuoteAsset;
            _expirationTimeInSeconds = settings.ExpirationTimeInSeconds;
            _historyMaxSize = settings.HistoryMaxSize;
            _minSpread = settings.MinSpread;

            _log = log;
            shutdownManager?.Register(this);

            _orderBooks = new ConcurrentDictionary<AssetPairSource, OrderBook>();
            _crossRates = new ConcurrentDictionary<AssetPairSource, CrossRate>();
            _arbitrages = new ConcurrentDictionary<string, Arbitrage>();
            _arbitrageHistory = new ConcurrentDictionary<string, Arbitrage>();
        }

        public IEnumerable<OrderBook> GetOrderBooks()
        {
            if (!_orderBooks.Any())
                return new List<OrderBook>();

            return _orderBooks.Select(x => x.Value)
                .OrderByDescending(x => x.Timestamp)
                .ToList();
        }

        public IEnumerable<OrderBook> GetOrderBooks(string exchange, string instrument)
        {
            if (!_orderBooks.Any())
                return new List<OrderBook>();

            var result = _orderBooks.Select(x => x.Value).ToList();

            if (!string.IsNullOrWhiteSpace(exchange))
                result = result.Where(x => x.Source.ToUpper().Trim().Contains(exchange.ToUpper().Trim())).ToList();

            if (!string.IsNullOrWhiteSpace(instrument))
                result = result.Where(x => x.AssetPairStr.ToUpper().Trim().Contains(instrument.ToUpper().Trim())).ToList();

            return result.OrderByDescending(x => x.Timestamp).ToList();
        }

        public IEnumerable<CrossRate> GetCrossRates()
        {
            if (!_crossRates.Any())
                return new List<CrossRate>();

            var result = _crossRates.Select(x => x.Value)
                .OrderByDescending(x => x.Timestamp)
                .ToList();

            return result;
        }

        public IEnumerable<Arbitrage> GetArbitrages()
        {
            if (!_arbitrages.Any())
                return new List<Arbitrage>();

            return _arbitrages.Select(x => x.Value)
                .OrderByDescending(x => x.PnL)
                .ToList();
        }

        public Arbitrage GetArbitrage(string conversionPath)
        {
            if (string.IsNullOrWhiteSpace(conversionPath))
                throw new ArgumentNullException(nameof(conversionPath));

            var bestArbitrage = _arbitrageHistory.FirstOrDefault(x => string.Equals(x.Value.ConversionPath, conversionPath, StringComparison.CurrentCultureIgnoreCase));

            return bestArbitrage.Value;
        }

        public IEnumerable<Arbitrage> GetArbitrageHistory(DateTime since, int take)
        {
            if (!_arbitrageHistory.Any())
                return new List<Arbitrage>();

            var result = new List<Arbitrage>();

            var arbitrages = _arbitrageHistory.Select(x => x.Value).ToList();
            var uniqueConversionPaths = arbitrages.Select(x => x.ConversionPath).Distinct().ToList();

            // Find only best arbitrage for path
            foreach (var conversionPath in uniqueConversionPaths)
            {
                var pathBestArbitrage = arbitrages.OrderByDescending(x => x.PnL).First(x => x.ConversionPath == conversionPath);
                result.Add(pathBestArbitrage);
            }

            return result
                .Where(x => x.EndedAt > since)
                .OrderByDescending(x => x.PnL)
                .Take(take)
                .ToList();
        }

        public Settings GetSettings()
        {
            return new Settings(_expirationTimeInSeconds, _baseAssets, _quoteAsset, _minSpread);
        }

        public void SetSettings(Settings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            var restartNeeded = false;

            if (settings.ExpirationTimeInSeconds > 0)
            {
                _expirationTimeInSeconds = settings.ExpirationTimeInSeconds;
                restartNeeded = true;
            }

            if (settings.BaseAssets != null && settings.BaseAssets.Any())
            {
                _baseAssets = settings.BaseAssets;
                restartNeeded = true;
            }

            if (!string.IsNullOrWhiteSpace(settings.QuoteAsset))
            {
                _quoteAsset = settings.QuoteAsset;
                restartNeeded = true;
            }

            _restartNeeded = restartNeeded;
        }



        public void Process(OrderBook orderBook)
        {
            // Update if contains base currency
            CheckForCurrencyAndUpdateOrderBooks(_quoteAsset, orderBook);

            // Update if contains wanted currency
            foreach (var wantedCurrency in _baseAssets)
            {
                CheckForCurrencyAndUpdateOrderBooks(wantedCurrency, orderBook);
            }
        }

        public override async Task Execute()
        {
            await CalculateCrossRates();
            await RefreshArbitrages();                                        

            RestartIfNeeded();
        }

        public async Task<IEnumerable<CrossRate>> CalculateCrossRates()
        {
            var watch = Stopwatch.StartNew();

            var newActualCrossRates = new SortedDictionary<AssetPairSource, CrossRate>();
            var actualOrderBooks = GetActualOrderBooks();

            foreach (var wantedCurrency in _baseAssets)
            {
                var wantedCurrencyKeys = actualOrderBooks.Keys.Where(x => x.AssetPair.ContainsAsset(wantedCurrency)).ToList();
                foreach (var wantedCurrencykey in wantedCurrencyKeys)
                {
                    var wantedOrderBook = actualOrderBooks[wantedCurrencykey];

                    // Trying to find wanted asset in current orderBook's asset pair
                    var wantedIntermediateAssetPair = AssetPair.FromString(wantedOrderBook.AssetPairStr, wantedCurrency);

                    // Get intermediate currency
                    var intermediateCurrency = wantedIntermediateAssetPair.Base == wantedCurrency
                        ? wantedIntermediateAssetPair.Quote
                        : wantedIntermediateAssetPair.Base;

                    // If original wanted/base or base/wanted pair then just save it
                    if (intermediateCurrency == _quoteAsset)
                    {
                        var intermediateWantedCrossRate = CrossRate.FromOrderBook(wantedOrderBook, new AssetPair(wantedCurrency, _quoteAsset));

                        var key = new AssetPairSource(intermediateWantedCrossRate.ConversionPath, intermediateWantedCrossRate.AssetPair);
                        newActualCrossRates.AddOrUpdate(key, intermediateWantedCrossRate);

                        continue;
                    }

                    // Trying to find intermediate/base or base/intermediate pair from any exchange
                    var intermediateBaseCurrencyKeys = actualOrderBooks.Keys
                        .Where(x => x.AssetPair.ContainsAsset(intermediateCurrency) && x.AssetPair.ContainsAsset(_quoteAsset))
                        .ToList();

                    foreach (var intermediateBaseCurrencyKey in intermediateBaseCurrencyKeys)
                    {
                        // Calculating cross rate for base/wanted pair
                        var wantedIntermediateOrderBook = wantedOrderBook;
                        var intermediateBaseOrderBook = actualOrderBooks[intermediateBaseCurrencyKey];

                        var targetBaseAssetPair = new AssetPair(wantedCurrency, _quoteAsset);
                        var crossRate = CrossRate.FromOrderBooks(wantedIntermediateOrderBook, intermediateBaseOrderBook, targetBaseAssetPair);

                        var key = new AssetPairSource(crossRate.ConversionPath, crossRate.AssetPair);
                        newActualCrossRates.AddOrUpdate(key, crossRate);
                    }
                }
            }

            _crossRates.AddOrUpdateRange(newActualCrossRates);

            watch.Stop();
            if (watch.ElapsedMilliseconds > 1000)
                await _log.WriteInfoAsync(GetType().Name, MethodBase.GetCurrentMethod().Name, $"{watch.ElapsedMilliseconds} ms for {_crossRates.Count} cross rates, {actualOrderBooks.Count} order books.");

            return _crossRates.Select(x => x.Value).ToList().AsReadOnly();
        }
        
        private IList<ArbitrageLine> CalculateArbitragesLines(IList<CrossRate> crossRates)
        {
            // TODO: can be improved.
            var lines = new List<ArbitrageLine>();

            // 1. Calculate minAsk and maxBid
            var minAsk = crossRates.SelectMany(x => x.Asks).Min(x => x.Price);
            var maxBid = crossRates.SelectMany(x => x.Bids).Max(x => x.Price);

            // No arbitrages
            if (minAsk >= maxBid)
                return lines;

            // 2. Collect only arbitrages lines
            foreach (var crossRate in crossRates)
            {
                crossRate.Asks.Where(x => x.Price < maxBid)
                    .ForEach(x => lines.Add(new ArbitrageLine
                    {
                        CrossRate = crossRate,
                        AskPrice = x.Price,
                        Volume = x.Volume
                    }));

                crossRate.Bids.Where(x => x.Price > minAsk)
                    .ForEach(x => lines.Add(new ArbitrageLine
                    {
                        CrossRate = crossRate,
                        BidPrice = x.Price,
                        Volume = x.Volume
                    }));
            }

            // 3. Order by Price
            lines = lines.OrderBy(x => x.Price).ThenBy(x => x.AskPrice).ToList();

            return lines;
        }

        public async Task<IEnumerable<Arbitrage>> CalculateArbitrages()
        {
            var watch = Stopwatch.StartNew();

            var newArbitrages = new SortedDictionary<string, Arbitrage>();
            var actualCrossRates = GetActualCrossRates();

            var totalItareations = 0;
            var totalLines = 0;
            // For each asset pair - for each cross rate make one line for every ask and bid, order that lines and find intersections
            var uniqueAssetPairs = actualCrossRates.Select(x => x.AssetPair).Distinct().ToList();
            foreach (var assetPair in uniqueAssetPairs)
            {
                var assetPairCrossRates = actualCrossRates.Where(x => x.AssetPair.Equals(assetPair)).ToList();

                var lines = CalculateArbitragesLines(assetPairCrossRates);

                totalLines = lines.Count;
                // Calculate arbitrage for every ask and every higher bid
                for (var a = 0; a < totalLines; a++)
                {
                    var askLine = lines[a];
                    if (askLine.AskPrice == 0)
                        continue;

                    for (var b = a + 1; b < totalLines; b++)
                    {
                        totalItareations++;

                        var bidLine = lines[b];
                        if (bidLine.BidPrice == 0)
                            continue;

                        var key = "(" + askLine.CrossRate.ConversionPath + ") * (" + bidLine.CrossRate.ConversionPath + ")";
                        if (newArbitrages.TryGetValue(key, out var existed))
                        {
                            var spread = (askLine.Price - bidLine.Price) / bidLine.Price * 100;
                            if (spread < _minSpread)
                                continue;

                            var volume = askLine.Volume < bidLine.Volume ? askLine.Volume : bidLine.Volume;
                            var pnL = (bidLine.Price - askLine.Price) * volume;
                            if (pnL <= existed.PnL)
                                continue;

                            var arbitrage = new Arbitrage(assetPair, askLine.CrossRate, askLine.VolumePrice, bidLine.CrossRate, bidLine.VolumePrice);
                            newArbitrages.AddOrUpdate(key, arbitrage);
                        }
                        else
                        {
                            var arbitrage = new Arbitrage(assetPair, askLine.CrossRate, askLine.VolumePrice, bidLine.CrossRate, bidLine.VolumePrice);
                            newArbitrages.Add(key, arbitrage);
                        }
                    }
                }
            }

            watch.Stop();
            if (watch.ElapsedMilliseconds > 1000)
                await _log.WriteInfoAsync(GetType().Name, MethodBase.GetCurrentMethod().Name, $"{watch.ElapsedMilliseconds} ms for {newArbitrages.Count} arbitrages, {totalLines} lines, {totalItareations} possible arbitrages.");

            return newArbitrages.Values;
        }

        public async Task RefreshArbitrages()
        {
            var watch = Stopwatch.StartNew();

            var newArbitragesList = await CalculateArbitrages(); // One per conversion path (with best PnL)
            var newArbitrages = new ConcurrentDictionary<string, Arbitrage>();

            // Form dictionary with new arbitrages
            foreach (var newArbitrage in newArbitragesList)
            {
                // Key must be unique for arbitrage in order to find when it started
                var key = newArbitrage.ConversionPath + newArbitrage.PnL;
                newArbitrages.AddOrUpdate(key, newArbitrage); // May be two arbitrages with the same path and the same PnL
            }

            // Remove every ended arbitrage and move it to the history
            var removed = 0;
            foreach (var oldArbitrage in _arbitrages)
            {
                if (!newArbitrages.Keys.Contains(oldArbitrage.Key))
                {
                    removed++;
                    // Remove from actual arbitrages
                    oldArbitrage.Value.EndedAt = DateTime.UtcNow;
                    _arbitrages.Remove(oldArbitrage.Key);

                    // Add it to the history
                    _arbitrageHistory.AddOrUpdate(oldArbitrage.Key, oldArbitrage.Value);
                }
            }

            // Add only new arbitrages, don't update existed to not change the StartedAt
            var added = 0;
            foreach (var newArbitrage in newArbitrages)
            {
                if (!_arbitrages.Keys.Contains(newArbitrage.Key))
                {
                    added++;
                    _arbitrages.Add(newArbitrage.Key, newArbitrage.Value);
                }
            }

            // If there are too many items
            var beforeCleaning = _arbitrageHistory.Count;
            CleanHistory();
            var afterCleaning = _arbitrageHistory.Count;

            watch.Stop();
            if (watch.ElapsedMilliseconds > 1000)
                await _log.WriteInfoAsync(GetType().Name, MethodBase.GetCurrentMethod().Name, $"{watch.ElapsedMilliseconds} ms for new {newArbitrages.Count} arbitrages, {removed} removed, {added} added, {beforeCleaning-afterCleaning} cleaned, {_arbitrages.Count} active, {_arbitrageHistory.Count} in history.");
        }


        private void CleanHistory()
        {
            var arbitrageHistory = _arbitrageHistory.Values;
            var extraCount = _arbitrageHistory.Count - _historyMaxSize;
            if (extraCount > 0)
            {
                // First try to delete extra arbitrages with the same conversion path
                var uniqueConversionPaths = arbitrageHistory.Select(x => x.ConversionPath).Distinct().ToList();
                foreach (var conversionPath in uniqueConversionPaths)
                {
                    var pathArbitrages = arbitrageHistory.OrderByDescending(x => x.PnL)
                        .Where(x => x.ConversionPath == conversionPath)
                        .Skip(1); // Leave 1 best for path
                    foreach (var arbitrage in pathArbitrages)
                        _arbitrageHistory.Remove(arbitrage.ToString());
                }
            }

            // If didn't help then delete extra with the oldest conversion path
            extraCount = _arbitrageHistory.Count - _historyMaxSize;
            if (extraCount > 0)
            {
                var arbitrages = arbitrageHistory.Take(extraCount).ToList();
                foreach (var arbitrage in arbitrages)
                {
                    _arbitrageHistory.Remove(arbitrage.ToString());
                }
            }
        }

        private void CheckForCurrencyAndUpdateOrderBooks(string currency, OrderBook orderBook)
        {
            if (!orderBook.AssetPairStr.Contains(currency))
                return;

            orderBook.SetAssetPair(currency);

            var key = new AssetPairSource(orderBook.Source, orderBook.AssetPair);
            _orderBooks.AddOrUpdate(key, orderBook);
        }

        private ConcurrentDictionary<AssetPairSource, OrderBook> GetActualOrderBooks()
        {
            var result = new ConcurrentDictionary<AssetPairSource, OrderBook>();

            foreach (var keyValue in _orderBooks)
            {
                if (DateTime.UtcNow - keyValue.Value.Timestamp < new TimeSpan(0, 0, 0, _expirationTimeInSeconds))
                {
                    result.Add(keyValue.Key, keyValue.Value);
                }
            }

            return result;
        }

        private IList<CrossRate> GetActualCrossRates()
        {
            var result = new List<CrossRate>();

            foreach (var crossRate in _crossRates)
            {
                if (DateTime.UtcNow - crossRate.Value.Timestamp < new TimeSpan(0, 0, 0, _expirationTimeInSeconds))
                {
                    result.Add(crossRate.Value);
                }
            }

            return result;
        }

        private async void RestartIfNeeded()
        {
            if (_restartNeeded)
            {
                _restartNeeded = false;

                _crossRates.Clear();
                _arbitrages.Clear();
                _arbitrageHistory.Clear();

                await _log.WriteInfoAsync(GetType().Name, MethodBase.GetCurrentMethod().Name, $"Restarted");
            }
        }
    }
}
