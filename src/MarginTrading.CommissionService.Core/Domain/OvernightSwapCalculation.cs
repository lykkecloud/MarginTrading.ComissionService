using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Domain
{
    public class OvernightSwapCalculation : IOvernightSwapCalculation
	{
		public string Key => GetKey(OperationId);

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
		
		public static string GetKey(string operationId) => $"{operationId}";

		public OvernightSwapCalculation([NotNull] string operationId, [NotNull] string accountId,
			[NotNull] string instrument, [NotNull] PositionDirection? direction, DateTime time, decimal volume, 
			decimal swapValue,
			[NotNull] string positionId, bool isSuccess, [CanBeNull] Exception exception = null)
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
		}
	}
}