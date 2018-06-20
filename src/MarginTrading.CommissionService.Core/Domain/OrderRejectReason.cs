namespace MarginTrading.CommissionService.Core.Domain
{
    public enum OrderRejectReason
    {
        None = 1,
        NoLiquidity = 2,
        NotEnoughBalance = 3,
        LeadToStopOut = 4,
        AccountInvalidState = 5,
        InvalidExpectedOpenPrice = 6,
        InvalidVolume = 7,
        InvalidTakeProfit = 8,
        InvalidStoploss = 9,
        InvalidInstrument = 10,
        InvalidAccount = 11,
        TradingConditionError = 12,
        TechnicalError = 13,
    }
}