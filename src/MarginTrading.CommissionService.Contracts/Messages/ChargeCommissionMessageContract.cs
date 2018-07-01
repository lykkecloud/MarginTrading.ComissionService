using System;
using Lykke.MarginTrading.CommissionService.Contracts.Models;

namespace Lykke.MarginTrading.CommissionService.Contracts.Messages
{
    public class ChargeCommissionMessageContract
    {
        public string OperationId { get; set; }
        public DateTime CalculationTime { get; set; }
        public string AccountId { get; set; }
        public CommissionTypeContract CommissionType { get; set; }
        public decimal Amount { get; set; }
    }
}