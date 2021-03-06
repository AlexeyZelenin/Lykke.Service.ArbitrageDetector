﻿using System.Collections.Generic;

namespace Lykke.Service.ArbitrageDetector.Client.Models
{
    /// <summary>
    /// Represents an arbitrage matrix.
    /// </summary>
    public sealed class Matrix
    {
        /// <summary>
        /// Asset pair.
        /// </summary>
        public string AssetPair { get; set; }

        /// <summary>
        /// Exchanges.
        /// </summary>
        public IList<Exchange> Exchanges { get; set; } = new List<Exchange>();

        /// <summary>
        /// Asks.
        /// </summary>
        public IList<decimal?> Asks { get; set; } = new List<decimal?>();

        /// <summary>
        /// Bids.
        /// </summary>
        public IList<decimal?> Bids { get; set; } = new List<decimal?>();

        /// <summary>
        /// Сells.
        /// </summary>
        public IList<IList<MatrixCell>> Cells { get; set; }
    }
}
