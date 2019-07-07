namespace MarginTrading.CommissionService.Core.Domain.Abstractions
{
    public interface IKeyedObject
    {
        string Key { get; }

        string GetFilterKey();
    }
}