using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Domain
{
    public class OvernightSwapCalculation : IOvernightSwapCalculation
	{
		public string Id => GetId(OperationId, PositionId);
		
		public string OperationId { get; }
		public string AccountId { get; }
		public string Instrument { get; }
		public PositionDirection? Direction { get; }
		public DateTime Time { get; }
		public decimal Volume { get; }
		public decimal SwapValue { get; }
		public string PositionId { get; }

		public bool IsSuccess { get; }
		public Exception Exception { get; }
		
		public bool WasCharged { get; }

		public OvernightSwapCalculation([NotNull] string operationId, [NotNull] string accountId,
			[NotNull] string instrument, [NotNull] PositionDirection? direction, DateTime time, decimal volume, 
			decimal swapValue, [NotNull] string positionId, bool isSuccess, [CanBeNull] Exception exception = null, 
			bool wasCharged = false)
		{
			OperationId = operationId ?? throw new ArgumentNullException(nameof(operationId));
			AccountId = accountId ?? throw new ArgumentNullException(nameof(accountId));
			Instrument = instrument ?? throw new ArgumentNullException(nameof(instrument));
			Direction = direction ?? throw new ArgumentNullException(nameof(direction));
			Time = time;
			Volume = volume;
			SwapValue = swapValue;
			PositionId = positionId ?? throw new ArgumentNullException(nameof(positionId));
			IsSuccess = isSuccess;
			Exception = exception;
			WasCharged = wasCharged;
		}

		public static string GetId(string operationId, string positionId) => $"{operationId}_{positionId}";
		

		public static (string OperationId, string PositionId) ExtractKeysFromId(string operationPositionId)
		{
			var arr = operationPositionId.Split('_');
			return (arr[0], arr[1]);
		}
	}
}