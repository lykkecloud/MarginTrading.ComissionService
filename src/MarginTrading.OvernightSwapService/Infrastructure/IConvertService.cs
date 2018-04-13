using System;
using AutoMapper;

namespace MarginTrading.OvernightSwapService.Infrastructure
{
    public interface IConvertService
    {
        TResult Convert<TSource, TResult>(TSource source, Action<IMappingOperationOptions<TSource, TResult>> opts);
        TResult Convert<TSource, TResult>(TSource source);
    }
}