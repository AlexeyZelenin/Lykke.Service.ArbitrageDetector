﻿using System;
using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents a synthetic order book.
    /// </summary>
    public class SynthOrderBook : OrderBook
    {
        /// <summary>
        /// Conversion path.
        /// </summary>
        public string ConversionPath { get; }

        /// <summary>
        /// Original order books.
        /// </summary>
        public IList<OrderBook> OriginalOrderBooks { get; }

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="assetPair"></param>
        /// <param name="bids"></param>
        /// <param name="asks"></param>
        /// <param name="conversionPath"></param>
        /// <param name="originalOrderBooks"></param>
        /// <param name="timestamp"></param>
        public SynthOrderBook(string source, AssetPair assetPair,
            IReadOnlyCollection<VolumePrice> bids, IReadOnlyCollection<VolumePrice> asks,
            string conversionPath, IList<OrderBook> originalOrderBooks, DateTime timestamp)
            : base(source, new AssetPair(assetPair.Base, assetPair.Quote), bids, asks, timestamp)
        {
            if (assetPair.IsEmpty())
                throw new ArgumentOutOfRangeException($"{nameof(assetPair)}. Base: {assetPair.Base}, Quote: {assetPair.Quote}.");

            AssetPair = assetPair;

            ConversionPath = string.IsNullOrEmpty(conversionPath)
                ? throw new ArgumentException(nameof(conversionPath))
                : conversionPath;

            OriginalOrderBooks = originalOrderBooks ?? throw new ArgumentNullException(nameof(originalOrderBooks));
        }

        /// <summary>
        /// Formats conversion path.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        public static string GetConversionPath(OrderBook left, OrderBook right)
        {
            return left + " * " + right;
        }

        /// <summary>
        /// Formats conversion path.
        /// </summary>
        /// <param name="leftSource"></param>
        /// <param name="leftAssetPair"></param>
        /// <param name="rightSource"></param>
        /// <param name="rightAssetPair"></param>
        public static string GetConversionPath(string leftSource, string leftAssetPair, string rightSource, string rightAssetPair)
        {
            return leftSource + "-" + leftAssetPair + " * " + rightSource + "-" + rightAssetPair;
        }

        /// <summary>
        /// Formats source - source path.
        /// </summary>
        /// <param name="leftSource"></param>
        /// <param name="rightSource"></param>
        /// <returns></returns>]
        public static string GetSourcesPath(string leftSource, string rightSource)
        {
            return leftSource + "-" + rightSource;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }
    }
}