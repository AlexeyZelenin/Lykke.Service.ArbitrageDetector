﻿using System;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using Common;
using Lykke.Service.ArbitrageDetector.AzureRepositories.Models;
using Lykke.Service.ArbitrageDetector.Core.Domain;

namespace Lykke.Service.ArbitrageDetector.AzureRepositories.Repositories
{
    public class MatrixHistoryBlobRepository : BlobRepository
    {
        private const string ContainerName = "matrix-history";

        public MatrixHistoryBlobRepository(IBlobStorage storage) : base(storage, ContainerName)
        {
        }

        public Task SaveAsync(MatrixBlob matrix)
        {
            var domain = Mapper.Map<Matrix>(matrix);
            return SaveBlobAsync(MatrixEntity.GenerateBlobId(domain), matrix.ToJson());
        }

        public async Task<MatrixBlob> GetAsync(string assetPair, DateTime dateTime)
        {
            var blobId = MatrixEntity.GenerateBlobId(assetPair, dateTime);
            if (!await BlobExistsAsync(blobId))
                return null;

            return (await GetBlobStringAsync(blobId)).DeserializeJson<MatrixBlob>();
        }

        public async Task DeleteIfExistsAsync(string assetPair, DateTime dateTime)
        {
            var blobId = MatrixEntity.GenerateBlobId(assetPair, dateTime);
            if (await BlobExistsAsync(blobId))
                await DeleteBlobAsync(blobId);
        }
    }
}
