# Lykke.Service.ArbitrageDetector

Service for detecting arbitrage situations.

Client: [Nuget](https://www.nuget.org/packages/Lykke.Service.ArbitrageDetector.Client)

# Client usage

Register client service in container:

```cs
...
ContainerBuilder.RegisterArbitrageDetectorClient({urlToService}, null);
```