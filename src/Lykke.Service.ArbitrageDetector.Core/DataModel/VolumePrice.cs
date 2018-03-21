﻿using System;
using DomainVolumePrice = Lykke.Service.ArbitrageDetector.Core.Domain.VolumePrice;

namespace Lykke.Service.ArbitrageDetector.Core.DataModel
{
    /// <summary>
    /// Represents a pair of price and volume.
    /// </summary>
    public struct VolumePrice
    {
        /// <summary>
        /// Price.
        /// </summary>
        public decimal Price { get; }

        /// <summary>
        /// Volume.
        /// </summary>
        public decimal Volume { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="domain"></param>
        public VolumePrice(DomainVolumePrice domain)
        {
            Price = domain.Price;
            Volume = domain.Volume;
        }

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="price"></param>
        /// <param name="volume"></param>
        public VolumePrice(decimal price, decimal volume)
        {
            Price = price;
            Volume = Math.Abs(volume);
        }
    }
}
