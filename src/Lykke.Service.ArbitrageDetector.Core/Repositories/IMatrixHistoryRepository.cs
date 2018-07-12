﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.Core.Repositories
{
    public interface IMatrixHistoryRepository
    {
        Task<Matrix> GetAsync(string assetPair, DateTime dateTime);

        Task<IEnumerable<DateTime>> GetDateTimeStampsAsync(string assetPair, DateTime date);

        Task<IEnumerable<DateTime>> GetDateTimeStampsAsync(string assetPair, DateTime from, DateTime to);

        Task<IEnumerable<string>> GetAssetPairsAsync(DateTime date);

        Task InsertAsync(Matrix matrix);

        Task<bool> DeleteAsync(string assetPair, DateTime dateTime);
    }
}