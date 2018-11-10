﻿using Microsoft.Extensions.Caching.Memory;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MemoryCacheServiceCollectionExtensions
    {
        public static ILinFxBuilder AddDistributedMemoryCache(this ILinFxBuilder builder, Action<MemoryDistributedCacheOptions> setupAction = default)
        {
            if(setupAction == default)
            {
                builder.Services.AddDistributedMemoryCache();
            }
            else
            {
                builder.Services.AddDistributedMemoryCache(setupAction);
            }

            return builder;
        }
    }
}
