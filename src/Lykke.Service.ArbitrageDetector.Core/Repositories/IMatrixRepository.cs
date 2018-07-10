﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Repositories
{
    public interface IMatrixRepository
    {
        Task<Matrix> GetAsync(string assetPair, DateTime dateTime);

        Task<IEnumerable<Matrix>> GetByAssetPairAndDateAsync(string assetPair, DateTime date);

        Task<IEnumerable<Matrix>> GetDateTimesOnlyByAssetPairAndDateAsync(string assetPair, DateTime date);

        Task InsertAsync(Matrix matrix);

        Task<bool> DeleteAsync(string assetPair, DateTime dateTime);
    }
}