#pragma warning disable 1591

namespace Lykke.MarginTrading.CommissionService.Email
{
    public interface ITemplateGenerator
    {
        string Generate<T>(string templateName, T model);
    }
}
