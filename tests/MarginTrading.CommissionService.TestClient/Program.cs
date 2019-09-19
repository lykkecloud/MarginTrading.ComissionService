// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AsyncFriendlyStackTrace;
using Common;
using JetBrains.Annotations;
using Lykke.HttpClientGenerator;
using Lykke.MarginTrading.CommissionService.Contracts;
using Lykke.Snow.Common.Startup;
using Newtonsoft.Json;
using Refit;

namespace MarginTrading.CommissionService.TestClient
{
    /// <summary>
    /// Simple way to check api clients are working.
    /// In future this could be turned into a functional testing app.
    /// </summary>
    internal static class Program
    {
        private static int _counter;

        static async Task Main()
        {
            try
            {
                await Run();
            }
            catch (ApiException e)
            {
                var str = e.Content;
                if (str.StartsWith('"'))
                {
                    str = TryDeserializeToString(str);
                }

                Console.WriteLine(str);
                Console.WriteLine(e.ToAsyncString());
            }
        }

        private static string TryDeserializeToString(string str)
        {
            try
            {
                return JsonConvert.DeserializeObject<string>(str);
            }
            catch
            {
                return str;
            }
        }

        private static async Task Run()
        {
            var clientGenerator = HttpClientGenerator
                .BuildForUrl("http://localhost:5050")
                .WithServiceName<MtCoreHttpErrorResponse>("MT Core Commission Service")
                .WithOptionalApiKey("margintrading")
                .WithoutRetries()
                .Create(); 
                //new HttpClientGenerator("http://localhost:5050", null, null);//"TestClient");
            
            await CheckOvernightSwapApiWorking(clientGenerator);
            await CheckCostAndChargesWorking(clientGenerator);
            // todo check other apis

            Console.WriteLine("Successfuly finished");
        }

        private static async Task CheckCostAndChargesWorking(HttpClientGenerator clientGenerator)
        {
            var client = clientGenerator.Generate<ICostsAndChargesApi>();
            var result = await client.GetByIds("AA2012", new List<string>().ToArray());
            
            Console.WriteLine($"Cost and charges number: {result.Length}, data: {result.ToJson()}");
        }

        private static async Task CheckOvernightSwapApiWorking(HttpClientGenerator clientGenerator)
        {
            /*var client = clientGenerator.Generate<IAccountsApi>();
            await client.List().Dump();
            await client.Insert(new AccountContract {Id = "smth"}).Dump();
            await client.GetByClientAndId(TODO, "smth").Dump();
            await client.Update("smth", new AccountContract {Id = "smth", ClientId = "some client"}).Dump();
            await client.Delete(TODO, "smth").Dump();*/
        }

        [CanBeNull]
        public static T Dump<T>(this T o)
        {
            var str = o is string s ? s : JsonConvert.SerializeObject(o);
            Console.WriteLine("{0}. {1}", ++_counter, str);
            return o;
        }

        [ItemCanBeNull]
        public static async Task<T> Dump<T>(this Task<T> t)
        {
            var obj = await t;
            obj.Dump();
            return obj;
        }

        public static async Task Dump(this Task o)
        {
            await o;
            "ok".Dump();
        }
    }
}