using StackExchange.Redis.Extensions.Core;
using StackExchange.Redis.Extensions.Newtonsoft;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StocksDecision.Models
{
    public class StackExchangeRedis
    {
        static StackExchangeRedis()
        {
            StackExchangeRedis.lazyConnection = new Lazy<StackExchangeRedisCacheClient>(() =>
            
                new StackExchangeRedisCacheClient(new NewtonsoftSerializer()) //.; //.Connect("localhost");
            );
        }
        private static Lazy<StackExchangeRedisCacheClient> lazyConnection;

        public StackExchangeRedisCacheClient Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }

    }
}