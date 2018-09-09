﻿using System.Threading.Tasks;
using Lykke.Service.ArbitrageDetector.Core.Domain;
using Lykke.Service.ArbitrageDetector.Core.Repositories;
using Lykke.Service.ArbitrageDetector.Core.Services;

namespace Lykke.Service.ArbitrageDetector.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly object _sync = new object();
        private Settings _settings;
        private Settings Settings { get { lock (_sync) { return _settings; } } set { lock (_sync) { _settings = value; } } }
        private readonly ISettingsRepository _settingsRepository;

        public SettingsService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public async Task<Settings> GetAsync()
        {
            if (Settings == null)
                Settings = await _settingsRepository.GetAsync();

            if (Settings == null)
                await _settingsRepository.InsertOrReplaceAsync(new Settings());

            return Settings;
        }

        public async Task SetAsync(Settings settings)
        {
            await _settingsRepository.InsertOrReplaceAsync(settings);
        }
    }
}
