using JetBrains.Annotations;

namespace MarginTrading.CommissionService.Infrastructure
{
    [UsedImplicitly]
    public class LykkeErrorResponse
    {
        public string ErrorMessage { get; set; }

        public override string ToString() => ErrorMessage;
    }
}