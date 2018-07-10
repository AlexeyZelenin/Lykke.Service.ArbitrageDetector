﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.ArbitrageDetector.AzureRepositories.Models;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Repositories
{
    public class MatrixRepository : IMatrixRepository
    {
        private readonly MatrixBlobRepository _blobRepository;
        private readonly INoSQLTableStorage<MatrixEntity> _storage;

        public MatrixRepository(MatrixBlobRepository blobRepository, INoSQLTableStorage<MatrixEntity> storage)
        {
            _blobRepository = blobRepository ?? throw new ArgumentNullException(nameof(blobRepository));
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public async Task<Matrix> GetAsync(string assetPair, DateTime dateTime)
        {
            var result = await _blobRepository.GetAsync(assetPair, dateTime);
            
            return result.Matrix();
        }

        public async Task<IEnumerable<DateTime>> GetDateTimeStampsAsync(string assetPair, DateTime date)
        {
            if (string.IsNullOrWhiteSpace(assetPair)) { throw new ArgumentException(nameof(assetPair)); }
            if (date == default || date.TimeOfDay.Ticks != 0) { throw new ArgumentOutOfRangeException(nameof(date)); }

            return await GetDateTimeStampsAsync(assetPair, date.Date, date.AddDays(1).Date);
        }

        public async Task<IEnumerable<DateTime>> GetDateTimeStampsAsync(string assetPair, DateTime from, DateTime to)
        {
            if (string.IsNullOrWhiteSpace(assetPair)) { throw new ArgumentException(nameof(assetPair)); }

            var pKeyFrom = MatrixEntity.GeneratePartitionKey(assetPair, from);
            var pKeyTo = MatrixEntity.GeneratePartitionKey(assetPair, to);
            var rowKeyFrom = MatrixEntity.GenerateRowKey(from);
            var rowKeyTo = MatrixEntity.GenerateRowKey(to);

            var query = new TableQuery<MatrixEntity>();

            var pkeyCondFrom = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThanOrEqual, pKeyFrom);
            var pkeyCondTo = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.LessThanOrEqual, pKeyTo);
            var pkeyFilter = TableQuery.CombineFilters(pkeyCondFrom, TableOperators.And, pkeyCondTo);

            var rowkeyCondFrom = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, rowKeyFrom);
            var rowkeyCondTo = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, rowKeyTo);
            var rowkeyFilter = TableQuery.CombineFilters(rowkeyCondFrom, TableOperators.And, rowkeyCondTo);

            query.FilterString = TableQuery.CombineFilters(pkeyFilter, TableOperators.And, rowkeyFilter);

            var references = await _storage.WhereAsync(query);

            return references.Select(x => x.DateTime);
        }

        public async Task InsertAsync(Matrix matrix)
        {
            await _storage.InsertAsync(new MatrixEntity(matrix));
            await _blobRepository.SaveAsync(new MatrixBlob(matrix));
        }

        public async Task<bool> DeleteAsync(string assetPair, DateTime dateTime)
        {
            var pkey = MatrixEntity.GeneratePartitionKey(assetPair, dateTime);
            var rowkey = MatrixEntity.GenerateRowKey(dateTime);

            var result = await _storage.DeleteIfExistAsync(pkey, rowkey);
            await _blobRepository.DeleteIfExistsAsync(assetPair, dateTime);

            return result;
        }
    }
}
