using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Rates;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings.Rates;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MarginTrading.CommissionService.Services
{
    public class RateSettingsService : IRateSettingsService
    {
        private readonly IMarginTradingBlobRepository _blobRepository;
        private readonly IDatabase _redisDatabase;
        private readonly IEventSender _eventSender;
        private readonly ILog _log;
        private readonly DefaultRateSettings _defaultRateSettings;

        private readonly IGenericCrudServiceFacade<OrderExecutionRate> _orderExecutionFacade;
        private readonly IGenericCrudServiceFacade<OvernightSwapRate> _overnightSwapFacade;
        private readonly IGenericCrudServiceFacade<OnBehalfRate> _onBehalfFacade;

        public const string TradingProfile = "";
        
        public RateSettingsService(
            IMarginTradingBlobRepository blobRepository,
            IDatabase redisDatabase,
            IEventSender eventSender,
            ILog log,
            DefaultRateSettings defaultRateSettings)
        {
            _blobRepository = blobRepository;
            _redisDatabase = redisDatabase;
            _eventSender = eventSender;
            _log = log;
            _defaultRateSettings = defaultRateSettings;

            _orderExecutionFacade = new GenericCrudServiceFacade<OrderExecutionRate>(blobRepository, redisDatabase, log,
                s => OrderExecutionRate.FromDefault(defaultRateSettings, s),
                LykkeConstants.RateSettingsBlobContainer, GetKey(LykkeConstants.OrderExecutionKey),
                _eventSender.SendRateSettingsChanged(CommissionType.OrderExecution),
                Serialize, Deserialize<OrderExecutionRate>,
                key => IsTradingProfile(OrderExecutionRate.GetTradingConditionFromKey(key)), 
                key => IsTradingProfile(OrderExecutionRate.GetTradingConditionFromKey(key)));
            _overnightSwapFacade = new GenericCrudServiceFacade<OvernightSwapRate>(blobRepository, redisDatabase, log,
                s => OvernightSwapRate.FromDefault(defaultRateSettings.DefaultOvernightSwapSettings, s), 
                LykkeConstants.RateSettingsBlobContainer, GetKey(LykkeConstants.OvernightSwapKey),
                _eventSender.SendRateSettingsChanged(CommissionType.OvernightSwap),
                Serialize, Deserialize<OvernightSwapRate>);
            _onBehalfFacade = new GenericCrudServiceFacade<OnBehalfRate>(blobRepository, redisDatabase, log,
                s => OnBehalfRate.FromDefault(defaultRateSettings.DefaultOnBehalfSettings, s),
                LykkeConstants.RateSettingsBlobContainer, GetKey(LykkeConstants.OnBehalfKey),
                _eventSender.SendRateSettingsChanged(CommissionType.OnBehalf),
                Serialize, Deserialize<OnBehalfRate>,
                IsTradingProfile, IsTradingProfile);
        }

        #region Order Execution

        public async Task<OrderExecutionRate> GetOrderExecutionRate(string tradingConditionId, string assetPairId)
            => await _orderExecutionFacade.Get(new OrderExecutionRate
            {
                TradingConditionId = tradingConditionId,
                AssetPairId = assetPairId
            }.Key);

        public async Task<IReadOnlyList<OrderExecutionRate>> GetOrderExecutionRates(string tradingConditionId = null)
            => await _orderExecutionFacade.GetMany(tradingConditionId);

        public async Task ReplaceOrderExecutionRates(List<OrderExecutionRate> rates)
            => await _orderExecutionFacade.Replace(rates);
        
        public async Task DeleteOrderExecutionRates(List<OrderExecutionRate> rates)
            => await _orderExecutionFacade.Delete(rates);

        #endregion Order Execution

        #region Overnight Swaps

        public async Task<OvernightSwapRate> GetOvernightSwapRate(string assetPair)
            => await _overnightSwapFacade.Get(assetPair);

        public async Task<IReadOnlyList<OvernightSwapRate>> GetOvernightSwapRatesForApi()
            => await _overnightSwapFacade.GetMany();

        public async Task ReplaceOvernightSwapRates(List<OvernightSwapRate> rates)
            => await _overnightSwapFacade.Replace(rates);

        public async Task DeleteOvernightSwapRates(List<OvernightSwapRate> rates)
            => await _overnightSwapFacade.Delete(rates);

        #endregion Overnight Swaps
        
        #region On Behalf

        public async Task<OnBehalfRate> GetOnBehalfRate(string tradingConditionId)
            => await _onBehalfFacade.Get(tradingConditionId);

        public async Task<IReadOnlyList<OnBehalfRate>> GetOnBehalfRates()
            => await _onBehalfFacade.GetMany();

        public async Task ReplaceOnBehalfRates(List<OnBehalfRate> rates)
            => await _onBehalfFacade.Replace(rates);

        public async Task DeleteOnBehalfRates(List<OnBehalfRate> rates)
            => await _onBehalfFacade.Delete(rates);

        #endregion On Behalf
        
        public static bool IsTradingProfile(string tradingConditionId) => tradingConditionId == TradingProfile;

        public static string GetOrderExecId(string tradingConditionId, string assetPairId) =>
            $"{tradingConditionId}_{assetPairId}";

        public static string GetKey(string key) => $"CommissionService:RateSettings:{key}";

        public static string Serialize<TMessage>(TMessage obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static TMessage Deserialize<TMessage>(string data)
        {
            return JsonConvert.DeserializeObject<TMessage>(data);
        }
    }
}