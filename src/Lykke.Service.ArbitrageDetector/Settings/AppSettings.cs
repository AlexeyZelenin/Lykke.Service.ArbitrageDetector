﻿using Lykke.Service.ArbitrageDetector.Settings.DependenciesSettings;
using Lykke.Service.ArbitrageDetector.Settings.ServiceSettings;
using Lykke.Service.ArbitrageDetector.Settings.SlackNotifications;

namespace Lykke.Service.ArbitrageDetector.Settings
{
    public class AppSettings
    {
        public ArbitrageDetectorSettings ArbitrageDetector { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
        public RateCalculatorServiceClientSettings RateCalculatorServiceClient { get; set; }
    }
}
