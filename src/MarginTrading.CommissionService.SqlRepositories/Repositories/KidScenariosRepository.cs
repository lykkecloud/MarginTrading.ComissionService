// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Common.MsSql;
using Lykke.Snow.Common.Model;
using MarginTrading.CommissionService.Core.Domain.KidScenarios;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.SqlRepositories.Entities;
using MarginTrading.CommissionService.SqlRepositories.Extensions;
using Microsoft.EntityFrameworkCore;

namespace MarginTrading.CommissionService.SqlRepositories.Repositories
{
    public class KidScenariosRepository : IKidScenariosRepository
    {
        private readonly MsSqlContextFactory<CommissionDbContext> _contextFactory;
        private readonly IConvertService _mapper;

        private const string DoesNotExistException =
            "Database operation expected to affect 1 row(s) but actually affected 0 row(s).";

        public KidScenariosRepository(MsSqlContextFactory<CommissionDbContext> contextFactory, IConvertService mapper)
        {
            _contextFactory = contextFactory;
            _mapper = mapper;
        }

        public async Task<Result<KidScenariosErrorCodes>> InsertAsync(KidScenario kidScenario)
        {
            var entity = _mapper.Convert<KidScenario, KidScenarioEntity>(kidScenario);
            entity.Timestamp = DateTime.UtcNow;

            using (var context = _contextFactory.CreateDataContext())
            {
                await context.KidScenarios.AddAsync(entity);

                try
                {
                    await context.SaveChangesAsync();
                    return new Result<KidScenariosErrorCodes>();
                }
                catch (DbUpdateException e)
                {
                    if (e.ValueAlreadyExistsException())
                    {
                        return new Result<KidScenariosErrorCodes>(KidScenariosErrorCodes.AlreadyExists);
                    }

                    throw;
                }
            }
        }

        public async Task<Result<KidScenariosErrorCodes>> UpdateAsync(KidScenario kidScenario)
        {
            var entity = _mapper.Convert<KidScenario, KidScenarioEntity>(kidScenario);
            entity.Timestamp = DateTime.UtcNow;

            using (var context = _contextFactory.CreateDataContext())
            {
                context.Update(entity);

                try
                {
                    await context.SaveChangesAsync();
                    return new Result<KidScenariosErrorCodes>();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    if (e.Message.Contains(DoesNotExistException))
                        return new Result<KidScenariosErrorCodes>(KidScenariosErrorCodes.DoesNotExist);

                    throw;
                }
            }
        }

        public async Task<Result<KidScenariosErrorCodes>> DeleteAsync(string isin)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = new KidScenarioEntity() {Isin = isin};

                context.Attach(entity);
                context.KidScenarios.Remove(entity);

                try
                {
                    await context.SaveChangesAsync();
                    return new Result<KidScenariosErrorCodes>();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    if (e.Message.Contains(DoesNotExistException))
                        return new Result<KidScenariosErrorCodes>(KidScenariosErrorCodes.DoesNotExist);

                    throw;
                }
            }
        }
        
        public async Task<Result<KidScenario, KidScenariosErrorCodes>> GetByIdAsync(string isin)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = await context.KidScenarios.FindAsync(isin);

                if (entity == null)
                    return new Result<KidScenario, KidScenariosErrorCodes>(KidScenariosErrorCodes.DoesNotExist);

                return new Result<KidScenario, KidScenariosErrorCodes>(_mapper.Convert<KidScenarioEntity, KidScenario>(entity));
            }
        }
        
        public async Task<Result<List<KidScenario>, KidScenariosErrorCodes>> GetAllAsync(string[] isins, int? skip, int? take)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var query = context.KidScenarios.AsNoTracking();

                if (isins != null && isins.Any())
                    query = query.Where(x => isins.Contains(x.Isin));

                if (skip.HasValue && take.HasValue)
                {
                    skip = Math.Max(0, skip.Value);
                    take = take < 0 ? 20 : Math.Min(take.Value, 100);
                    
                    query = query
                        .Skip(skip.Value)
                        .Take(take.Value);
                }

                var entities = await query
                    .OrderBy(x => x.Isin)
                    .ToListAsync();

                return new Result<List<KidScenario>, KidScenariosErrorCodes>(entities.Select(x => _mapper.Convert<KidScenarioEntity, KidScenario>(x)).ToList());
            }
        }
    }
}