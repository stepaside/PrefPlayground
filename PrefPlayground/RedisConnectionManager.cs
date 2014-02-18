using BookSleeve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrefPlayground
{
    public class RedisConnectionManager
    {
        private RedisConnection _writer = null;
        private IList<RedisConnection> _readers = null;
        private RedisConnection _manager = null;

        public RedisConnectionManager(string connection)
        {
            _manager = RedisConnectionPool.Current.GetConnection(connection);
        }

        public RedisConnectionManager(string writer, string[] readers)
        {
            _writer = RedisConnectionPool.Current.GetConnection(writer);
            _readers = readers.Select(r => RedisConnectionPool.Current.GetConnection(r)).ToArray();
        }

        public RedisConnection Writer
        {
            get
            {
                return _writer;
            }
        }

        public RedisConnection Reader
        {
            get
            {
                return _readers[new Random().Next(0, _readers.Count)];
            }
        }
    }
}