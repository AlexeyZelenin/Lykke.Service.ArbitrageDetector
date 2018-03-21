﻿using System;
using DomainArbitrage = Lykke.Service.ArbitrageDetector.Core.Domain.Arbitrage;

namespace Lykke.Service.ArbitrageDetector.Core.DataModel
{
    /// <summary>
    /// Represents an arbitrage situation.
    /// </summary>
    public sealed class Arbitrage
    {
        /// <summary>
        /// AssetPair.
        /// </summary>
        public AssetPair AssetPair { get; }

        /// <summary>
        /// Ask exchange name.
        /// </summary>
        public string AskSource { get; }

        /// <summary>
        /// Bid exchange name.
        /// </summary>
        public string BidSource { get; }

        /// <summary>
        /// Conversion path from ask.
        /// </summary>
        public string AskConversionPath { get; }

        /// <summary>
        /// Conversion path from bid.
        /// </summary>
        public string BidConversionPath { get; }

        /// <summary>
        /// Price and volume of low ask.
        /// </summary>
        public VolumePrice Ask { get; }

        /// <summary>
        /// Price and volume of high bid.
        /// </summary>
        public VolumePrice Bid { get; }

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
        public TimeSpan Lasted => EndedAt - StartedAt;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="assetPair"></param>
        /// <param name="askSource"></param>
        /// <param name="bidSource"></param>
        /// <param name="ask"></param>
        /// <param name="bid"></param>
        /// <param name="spread"></param>
        /// <param name="volume"></param>
        /// <param name="pnL"></param>
        /// <param name="startedAt"></param>
        /// <param name="endedAt"></param>
        public Arbitrage(AssetPair assetPair, string askSource, string bidSource, string askPath, string bidPath, VolumePrice ask, VolumePrice bid,
            decimal spread, decimal volume, decimal pnL, DateTime startedAt, DateTime endedAt)
        {
            AssetPair = assetPair;
            AskSource = string.IsNullOrWhiteSpace(askSource) ? throw new ArgumentNullException(nameof(askSource)) : askSource;
            BidSource = string.IsNullOrWhiteSpace(bidSource) ? throw new ArgumentNullException(nameof(bidSource)) : bidSource;
            AskConversionPath = string.IsNullOrWhiteSpace(askPath) ? throw new ArgumentNullException(nameof(askPath)) : askPath;
            BidConversionPath = string.IsNullOrWhiteSpace(bidPath) ? throw new ArgumentNullException(nameof(bidPath)) : bidPath;
            Ask = ask;
            Bid = bid;
            Spread = spread;
            Volume = volume;
            PnL = pnL;
            StartedAt = startedAt;
            EndedAt = endedAt;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domain"></param>
        public Arbitrage(DomainArbitrage domain)
        {
            AssetPair = new AssetPair(domain.AssetPair);
            AskSource = domain.AskCrossRate.Source;
            BidSource = domain.BidCrossRate.Source;
            AskConversionPath = domain.AskCrossRate.ConversionPath;
            BidConversionPath = domain.BidCrossRate.ConversionPath;
            Ask = new VolumePrice(domain.Ask);
            Bid = new VolumePrice(domain.Bid);
            Spread = domain.Spread;
            Volume = domain.Volume;
            PnL = domain.PnL;
            StartedAt = domain.StartedAt;
            EndedAt = domain.EndedAt;
        }
    }
}
