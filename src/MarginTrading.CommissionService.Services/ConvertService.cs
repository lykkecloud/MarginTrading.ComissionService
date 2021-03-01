﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using AutoMapper;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Contracts.Models.KidScenarios;
using MarginTrading.CommissionService.Core.Domain.KidScenarios;
using MarginTrading.CommissionService.Core.Services;
using Asset = MarginTrading.CommissionService.Core.Domain.Asset;

namespace MarginTrading.CommissionService.Services
{
    [UsedImplicitly]
    public class ConvertService : IConvertService
    {
        private readonly IMapper _mapper = CreateMapper();

        private static IMapper CreateMapper()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MarginTrading.AssetService.Contracts.LegacyAsset.Asset, Asset>()
                    .ConstructUsing(x =>
                        new Asset(x.AssetId, x.Name, x.DisplayPrecision, x.Underlying.AvailableClientProfiles));
                
                // kid scenarios

                cfg.CreateMap<AddKidScenarioRequest, KidScenario>()
                    .ForMember(x => x.Timestamp, o => o.Ignore());
                
                cfg.CreateMap<UpdateKidScenarioRequest, KidScenario>()
                    .ForMember(x => x.Isin, o => o.Ignore())
                    .ForMember(x => x.Timestamp, o => o.Ignore());
            }).CreateMapper();
        }

        public TResult Convert<TSource, TResult>(TSource source,
            Action<IMappingOperationOptions<TSource, TResult>> opts)
        {
            return _mapper.Map(source, opts);
        }

        public TResult Convert<TSource, TResult>(TSource source)
        {
            return _mapper.Map<TSource, TResult>(source);
        }
    }
}