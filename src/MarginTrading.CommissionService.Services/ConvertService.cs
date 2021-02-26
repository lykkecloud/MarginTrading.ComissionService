// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using AutoMapper;
using JetBrains.Annotations;
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
                    .ForMember(dest => dest.AvailableClientProfiles,
                        opt => opt.MapFrom(x => x.Underlying.AvailableClientProfiles))
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(x => x.AssetId))
                    .ForMember(dest => dest.Name, opt => opt.MapFrom(x => x.Name))
                    .ForMember(dest => dest.Accuracy, opt => opt.MapFrom(x => x.DisplayPrecision));
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