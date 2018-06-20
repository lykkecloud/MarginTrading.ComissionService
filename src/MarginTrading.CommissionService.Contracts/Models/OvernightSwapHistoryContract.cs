using System;
using System.Collections.Generic;

namespace Lykke.MarginTrading.CommissionService.Contracts.Models
{
    public class OvernightSwapHistoryContract
    {
        string ClientId { get; }
        string AccountId { get; }
        string Instrument { get; }
        string Direction { get; }
        DateTime Time { get; }
        decimal Volume { get; }
        decimal Value { get; }
        decimal SwapRate { get; }
        List<string> OpenOrderIds { get; }
        
        bool IsSuccess { get; }
        Exception Exception { get; }
    }
}