using BookSleeve;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Web;

namespace PrefPlayground
{
    public sealed class RedisConnectionPool
    {
        private const string RedisConnectionFailed = "Redis connection failed.";
        private ConcurrentDictionary<string, RedisConnection> _connections;
        private static Lazy<RedisConnectionPool> _pool = new Lazy<RedisConnectionPool>(() => new RedisConnectionPool(), true);
        
        private RedisConnectionPool()
        {
            _connections = new ConcurrentDictionary<string, RedisConnection>();
        }

        private static RedisConnection CreateConnection(string host)
        {
            return new RedisConnection(host, syncTimeout: 5000, ioTimeout: 5000);
        }

        public static RedisConnectionPool Current
        {
            get
            {
                return _pool.Value;
            }
        }

        public RedisConnection GetConnection(string host)
        {
           return _connections.AddOrUpdate(host.ToLower(), key => CreateConnection(key), (key, current) =>
            {
                var connection = current;
                if (connection == null)
                {
                    connection = CreateConnection(key);
                }

                if (connection.State == RedisConnectionBase.ConnectionState.Opening)
                {
                    return connection;
                }

                if (connection.State == RedisConnectionBase.ConnectionState.Closing || connection.State == RedisConnectionBase.ConnectionState.Closed)
                {
                    try
                    {
                        connection = CreateConnection(key);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(RedisConnectionFailed, ex);
                    }
                }

                if (connection.State == RedisConnectionBase.ConnectionState.New)
                {
                    try
                    {
                        var openAsync = connection.Open();
                        connection.Wait(openAsync);
                    }
                    catch (SocketException ex)
                    {
                        throw new Exception(RedisConnectionFailed, ex);
                    }
                }

                return connection;
            });
        }
    }
}