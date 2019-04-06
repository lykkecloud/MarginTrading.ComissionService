using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Extensions;

namespace MarginTrading.CommissionService.Core.Domain
{
    public class OvernightSwapCalculation : IOvernightSwapCalculation
    {
	    private const string Separator = "_";
		
		public string Id => GetId(OperationId, PositionId);
		
		public string ProcessId { get; set; }

		public string OperationId { get; }
		public string AccountId { get; }
		public string Instrument { get; }
		public PositionDirection? Direction { get; }
		public DateTime Time { get; }
		public decimal Volume { get; }
		public decimal SwapValue { get; }
		public string PositionId { get; }
		public string Details { get; }
		public DateTime TradingDay { get; }

		public bool IsSuccess { get; }
		public Exception Exception { get; }
		
		public bool? WasCharged { get; }

		public OvernightSwapCalculation([NotNull] string operationId, [NotNull] string accountId,
			[NotNull] string instrument, [NotNull] PositionDirection? direction, DateTime time, decimal volume, 
			decimal swapValue, [NotNull] string positionId, [CanBeNull] string details, DateTime tradingDay, 
			bool isSuccess, [CanBeNull] Exception exception = null, bool? wasCharged = null)
		{
			OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
			AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
			Instrument = instrument ?? throw new ArgumentNullException(nameof(instrument));
			Direction = direction ?? throw new ArgumentNullException(nameof(direction));
			Time = time;
			Volume = volume;
			SwapValue = swapValue;
			PositionId = positionId ?? throw new ArgumentNullException(nameof(positionId));
			Details = details;
			TradingDay = tradingDay;
			IsSuccess = isSuccess;
			Exception = exception;
			WasCharged = wasCharged;
		}

		public static string GetId(string operationId, string positionId) => $"{operationId}{Separator}{positionId}";

		public static (string OperationId, string PositionId) ExtractKeysFromId(string operationPositionId)
		{
			var separatorIndex = operationPositionId.LastIndexOf(Separator, StringComparison.InvariantCulture)
				.RequiredGreaterThan(-1, nameof(operationPositionId));
			return (operationPositionId.Substring(0, separatorIndex), 
					operationPositionId.Substring(separatorIndex + 1));
		}

		public static string ExtractOperationId(string id)
		{
			var separatorIndex = id.LastIndexOf(Separator, StringComparison.InvariantCulture);

			return separatorIndex == -1 ? id : id.Substring(0, separatorIndex);
		}
	}
}