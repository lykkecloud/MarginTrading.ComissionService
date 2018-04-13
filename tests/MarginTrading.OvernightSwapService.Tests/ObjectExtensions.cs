using System;
using System.Threading.Tasks;
using Common;
using FluentAssertions;
using FluentAssertions.Equivalency;
using Moq;

namespace MarginTrading.OvernightSwapService.Tests
{
    internal static class ObjectExtensions
    {
        public static Task<T> ToTask<T>(this T obj)
        {
            return Task.FromResult(obj);
        }

        public static T Equivalent<T>(this T o)
        {
            return o.Equivalent(op => op);
        }

        public static T Equivalent<T>(this T o, Func<EquivalencyAssertionOptions<T>, EquivalencyAssertionOptions<T>> config)
        {
            return Match.Create<T>(s =>
            {
                try
                {
                    s.Should().BeEquivalentTo(o, config);
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return false;
                }
            });
        }

        public static T Trace<T>(this T o)
        {
            Console.WriteLine(o.ToJson());
            return o;
        }
    }
}
