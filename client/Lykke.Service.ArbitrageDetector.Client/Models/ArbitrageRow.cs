﻿using System;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an arbitrage situation.
    /// </summary>
    public sealed class ArbitrageRow
    {
        /// <summary>
        /// AssetPair.
        /// </summary>
        public AssetPair AssetPair { get; }

        /// <summary>
        /// Bid exchange name.
        /// </summary>
        public string BidSource { get; }

        /// <summary>
        /// Ask exchange name.
        /// </summary>
        public string AskSource { get; }

        /// <summary>
        /// Conversion path from bid.
        /// </summary>
        public string BidConversionPath { get; }

        /// <summary>
        /// Conversion path from ask.
        /// </summary>
        public string AskConversionPath { get; }

        /// <summary>
        /// Price and volume of high bid.
        /// </summary>
        public VolumePrice Bid { get; }

        /// <summary>
        /// Price and volume of low ask.
        /// </summary>
        public VolumePrice Ask { get; }

        /// <summary>
        /// Spread between ask and bid.
        /// </summary>
        public decimal Spread { get; }

        /// <summary>
        /// The smallest volume of ask or bid.
        /// </summary>
        public decimal Volume { get; }

        /// <summary>
        /// Potential profit or loss.
        /// </summary>
        public decimal PnL { get; }

        /// <summary>
        /// The time when it first appeared.
        /// </summary>
        public DateTime StartedAt { get; }

        /// <summary>
        /// The time when it disappeared.
        /// </summary>
        public DateTime EndedAt { get; set; }

        /// <summary>
        /// How log the arbitrage lasted.
        /// </summary>
        public TimeSpan Lasted => EndedAt == default ? DateTime.UtcNow - StartedAt : EndedAt - StartedAt;

        /// <summary>
        /// Conversion path.
        /// </summary>
        public string ConversionPath => Arbitrage.FormatConversionPath(BidConversionPath, AskConversionPath);

        /// <summary>
        /// Constructor.
        /// </summary>
        public ArbitrageRow(AssetPair assetPair, string bidSource, string askSource, string bidConversionPath, string askConversionPath, VolumePrice bid, VolumePrice ask,
            decimal spread, decimal volume, decimal pnL, DateTime startedAt, DateTime endedAt)
        {
            AssetPair = assetPair;
            BidSource = string.IsNullOrWhiteSpace(bidSource) ? throw new ArgumentNullException(nameof(bidSource)) : bidSource;
            AskSource = string.IsNullOrWhiteSpace(askSource) ? throw new ArgumentNullException(nameof(askSource)) : askSource;
            BidConversionPath = string.IsNullOrWhiteSpace(bidConversionPath) ? throw new ArgumentNullException(nameof(bidConversionPath)) : bidConversionPath;
            AskConversionPath = string.IsNullOrWhiteSpace(askConversionPath) ? throw new ArgumentNullException(nameof(askConversionPath)) : askConversionPath;
            Bid = bid;
            Ask = ask;
            Spread = spread;
            Volume = volume;
            PnL = pnL;
            StartedAt = startedAt;
            EndedAt = endedAt;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return ConversionPath;
        }
    }
}
