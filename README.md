# MarginTrading.CommissionService API #

API for commission management.

## How to use in prod env? ##

1. Pull "mt-commission-service" docker image with a corresponding tag.
2. Configure environment variables according to "Environment variables" section.
3. Put secrets.json with endpoint data including the certificate:
```json
"Kestrel": {
  "EndPoints": {
    "HttpsInlineCertFile": {
      "Url": "https://*:5150",
      "Certificate": {
        "Path": "<path to .pfx file>",
        "Password": "<certificate password>"
      }
    }
}
```
4. Initialize all dependencies.
5. Run.

## How to run for debug? ##

1. Clone repo to some directory.
2. In MarginTrading.CommissionService root create a appsettings.dev.json with settings.
3. Add environment variable "SettingsUrl": "appsettings.dev.json".
4. VPN to a corresponding env must be connected and all dependencies must be initialized.
5. Run.

### Dependencies ###

TBD

### Configuration ###

Kestrel configuration may be passed through appsettings.json, secrets or environment.
All variables and value constraints are default. For instance, to set host URL the following env variable may be set:
```json
{
    "Kestrel__EndPoints__Http__Url": "http://*:5050"
}
```

### Environment variables ###

* *RESTART_ATTEMPTS_NUMBER* - number of restart attempts. If not set int.MaxValue is used.
* *RESTART_ATTEMPTS_INTERVAL_MS* - interval between restarts in milliseconds. If not set 10000 is used.
* *SettingsUrl* - defines URL of remote settings or path for local settings.

### Settings ###

Settings schema is:

```json
{
  "CommissionService": {
    "Db": {
      "StorageMode": "SqlServer",
      "StateConnString": "state connection string",
      "LogsConnString": "logs connection string"
    },
    "RabbitMq": {
      "Publishers": {
        "RateSettingsChanged": {
          "ConnectionString": "amqp://login:pwd@rabbit-mt.mt.svc.cluster.local:5672",
          "ExchangeName": "CommissionRateSettingsChanged"
        }
      },
      "Consumers": {
        "FxRateRabbitMqSettings": {
          "ConnectionString": "amqp://login:pwd@rabbit-mt.mt.svc.cluster.local:5672",
          "ExchangeName": "lykke.stpexchangeconnector.fxRates"
        },
        "OrderExecutedSettings": {
          "ConnectionString": "amqp://login:pwd@rabbit-mt.mt.svc.cluster.local:5672",
          "ExchangeName": "lykke.mt.orderhistory"
        },
        "SettingsChanged": {
          "ConnectionString": "amqp://login:pwd@rabbit-mt.mt.svc.cluster.local:5672",
          "ExchangeName": "MtCoreSettingsChanged"
        }
      }
    },
    "Services": {
      "Backend": {
        "Url": "http://mt-trading-core.mt.svc.cluster.local",
        "ApiKey": "api key"
      },
      "TradingHistory": {
        "Url": "http://mt-tradinghistory.mt.svc.cluster.local",
        "ApiKey": "api key"
      },
      "AccountManagement": {
        "Url": "http://mt-account-management.mt.svc.cluster.local",
        "ApiKey": "api key"
      },
      "SettingsService": {
        "Url": "http://mt-settings-service.mt.svc.cluster.local",
        "ApiKey": "api key"
      }
    },
    "Cqrs": {
      "ConnectionString": "amqp://login:pwd@rabbit-mt.mt.svc.cluster.local:5672",
      "RetryDelay": "00:01:00",
      "EnvironmentName": "env name"
    },
    "DefaultRateSettings": {
      "DefaultOrderExecutionSettings": {
        "CommissionCap": 100,
        "CommissionFloor": 10,
        "CommissionRate": 0.001,
        "CommissionAsset": "EUR",
        "LegalEntity": "Default"
      },
      "DefaultOvernightSwapSettings": {
        "RepoSurchargePercent": 0,
        "FixRate": 0.05,
        "VariableRateBase": "",
        "VariableRateQuote": ""
      },
      "DefaultOnBehalfSettings": {
        "Commission": 10,
        "CommissionAsset": "EUR",
        "LegalEntity": "Default"
      }
    },
    "RedisSettings": {
      "Configuration": "redis connection string"
    },
    "ChaosKitty": {
      "StateOfChaos": 0
    },
    "OvernightSwapsChargingTimeoutSec": 600,
    "DailyPnlsChargingTimeoutSec": 600,
    "UseSerilog": false
  }
}
```
