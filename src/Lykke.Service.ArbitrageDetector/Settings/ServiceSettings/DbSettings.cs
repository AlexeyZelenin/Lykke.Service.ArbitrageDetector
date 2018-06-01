﻿using Lykke.SettingsReader.Attributes;

namespace Lykke.Service.ArbitrageDetector.Settings.ServiceSettings
{
    public class DbSettings
    {
        [AzureTableCheck]
        public string LogsConnectionString { get; set; }

        [AzureTableCheck]
        public string DataConnectionString { get; set; }
    }
}
