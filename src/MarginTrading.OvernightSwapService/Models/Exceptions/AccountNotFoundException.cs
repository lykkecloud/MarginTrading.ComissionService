using System;

namespace MarginTrading.OvernightSwapService.Models.Exceptions
{
    public class AccountNotFoundException : Exception
    {
        public string AccountId { get; private set; }

        public AccountNotFoundException(string accountId, string message):base(message)
        {
            AccountId = accountId;
        }
    }
}