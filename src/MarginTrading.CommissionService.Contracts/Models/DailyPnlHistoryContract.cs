using System;

namespace Lykke.MarginTrading.CommissionService.Contracts.Models
{
    public class DailyPnlHistoryContract
    {
        public string Id { get; set; }
        
        public string OperationId { get; set; }
        public string AccountId { get; set; }
        public string Instrument { get; set; }
        public DateTime Time { get; set; }
        public DateTime TradingDay { get; set; }
        public decimal Volume { get; set; }
        public decimal FxRate { get; set; }
        public string PositionId { get; set; }
        public decimal Pnl { get; set; }
        
        /// <summary>
        /// Null - not charged yet, False - charging failed, True - charged
        /// </summary>
        public bool? WasCharged { get; set; }
    }
}