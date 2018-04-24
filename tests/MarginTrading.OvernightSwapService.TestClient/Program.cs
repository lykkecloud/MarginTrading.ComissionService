using System;
using System.Threading.Tasks;
using AsyncFriendlyStackTrace;
using JetBrains.Annotations;
using MarginTrading.OvernightSwapService.Contracts.Client;
using Newtonsoft.Json;
using Refit;

namespace MarginTrading.OvernightSwapService.TestClient
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
            var clientGenerator = new HttpClientGenerator("http://localhost:5007", "TestClient");
            
            await CheckOvernightSwapApiWorking(clientGenerator);
            // todo check other apis

            Console.WriteLine("Successfuly finished");
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