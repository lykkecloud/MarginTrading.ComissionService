// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Repositories;
using MoreLinq;
using Newtonsoft.Json;

namespace MarginTrading.CommissionService.SqlRepositories.Repositories
{
    public class SqlBlobRepository : IMarginTradingBlobRepository
    {
        private const string TableName = "BlobData";
        private const string CreateTableScript = "CREATE TABLE [{0}](" +
                                                 "[BlobKey] [nvarchar] (64) NOT NULL PRIMARY KEY, " +
                                                 "[Data] [nvarchar] (MAX) NULL, " +
                                                 "[Timestamp] [DateTime] NULL " +
                                                 ");";
        
        private readonly string _connectionString;
        
        public SqlBlobRepository(string connectionString)
        {
            _connectionString = connectionString;
            
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.CreateTableIfDoesntExists(CreateTableScript, TableName);
            }
        }
        
        public T Read<T>(string blobContainer, string key)
        {
            return ReadAsync<T>(blobContainer, key).GetAwaiter().GetResult();
        }

        public async Task MergeListAsync<T>(string blobContainer, string key, List<T> objects, Func<T, string> selector)
        {
            var existing = Read<IEnumerable<T>>(blobContainer, key)?.ToList() ?? new List<T>();

            await WriteAsync(blobContainer, key, objects.Concat(existing.ExceptBy(objects, selector)));
        }

        public async Task<T> ReadAsync<T>(string blobContainer, string key)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                var data = (await conn.QueryAsync<string>(
                    $"SELECT Data FROM {TableName} WHERE BlobKey=@blobKey",
                    new {blobKey = $"{blobContainer}_{key}"})).SingleOrDefault();

                if (string.IsNullOrEmpty(data))
                    return default(T);
                
                return JsonConvert.DeserializeObject<T>(data); 
            }
        }

        public async Task WriteAsync<T>(string blobContainer, string key, T obj)
        {
            var request = new
            {
                data = JsonConvert.SerializeObject(obj),
                blobKey = $"{blobContainer}_{key}",
                timestamp = DateTime.Now
            };
            
            using (var conn = new SqlConnection(_connectionString))
            {
                if (await conn.ExecuteScalarAsync<int>($"select count(*) from {TableName} where BlobKey=@blobKey",
                    request) == 0)
                {
                    await conn.ExecuteAsync(
                        $"insert into {TableName} (BlobKey, Data, Timestamp) values (@blobKey, @data, @timestamp)",
                        request);
                }
                else
                {
                    await conn.ExecuteAsync(
                        $"update {TableName} set Data=@data, Timestamp = @timestamp where BlobKey=@blobKey",
                        request);
                }
            }
        }
    }
}
