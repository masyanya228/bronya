﻿using Bronya.Caching.Structure;
using Bronya.Dtos;
using Bronya.Services;

using System.Runtime.Caching;

namespace Bronya.Caching
{
    public class SmenaCacheService : InMemoryCacheService<SmenaDto>
    {
        public override object Remove(SmenaDto entity)
        {
            return Cache.Remove(GetKey(entity));
        }

        public override string GetKey(SmenaDto entity)
        {
            return "currentSmena";
        }
    }
}
