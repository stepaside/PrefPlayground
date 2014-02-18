using BookSleeve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;

namespace PrefPlayground
{
    public class PreferenceManager
    {
        private RedisConnectionManager _client = new RedisConnectionManager("localhost", new[] { "localhost" });

        public async Task<bool> Add(string name, string value, Node business, Node system)
        {
            using (var tran = _client.Writer.CreateTransaction())
            {
                var nodes = new[] { business, system };
                for (int i = 0; i < 2; i++)
                {
                    var node = nodes[i];
                    do
                    {
                        tran.Sets.Add(0, "names:" + node.Name + "." + node.Id, name);
                        tran.Sets.Add(0, name + ":" + node.Name + "." + node.Id, value);
                        node = node.Next;
                    } while (node != null);
                }
                return await tran.Execute();
            }
        }

        public async Task<bool> Set(string name, string value, Node business, Node system)
        {
            using (var tran = _client.Writer.CreateTransaction())
            {
                var nodes = new[] { business, system };
                for (int i = 0; i < 2; i++)
                {
                    var node = nodes[i];
                    do
                    {
                        tran.Sets.Remove(0, "names:" + node.Name + "." + node.Id, name);
                        tran.Sets.Remove(0, name + ":" + node.Name + "." + node.Id, value);
                        tran.Sets.Add(0, "names:" + node.Name + "." + node.Id, name);
                        tran.Sets.Add(0, name + ":" + node.Name + "." + node.Id, value);
                        node = node.Next;
                    } while (node != null);
                }
                return await tran.Execute();
            }
        }

        public async Task<bool> Remove(string name, string value, Node business, Node system)
        {
            using (var tran = _client.Writer.CreateTransaction())
            {
                var nodes = new[] { business, system };
                for (int i = 0; i < 2; i++)
                {
                    var node = nodes[i];
                    do
                    {
                        tran.Sets.Remove(0, "names:" + node.Name + "." + node.Id, name);
                        tran.Sets.Remove(0, name + ":" + node.Name + "." + node.Id, value);
                        node = node.Next;
                    } while (node != null);
                }
                return await tran.Execute();
            }
        }

        public async Task<string> Get(string name, Node business, Node system)
        {
            using (var tran = _client.Reader.CreateTransaction())
            {
                var rng = new RNGCryptoServiceProvider();
                var data = new byte[8];
                rng.GetBytes(data);
                var suffix = BitConverter.ToUInt64(data, 0);
                
                var nodes = new Dictionary<Node, Tuple<string, List<string>>> { { business, Tuple.Create("BUSINESS:" + suffix, new List<string>())}, {system, Tuple.Create("SYSTEM:" + suffix, new List<string>())} };

                foreach (var item in nodes)
                {
                    var node = item.Key;
                    do
                    {
                        item.Value.Item2.Add(name + ":" + node.Name + "." + node.Id);
                        node = node.Next;
                    } while (node != null);
                }

                foreach (var item in nodes.Values)
                {
                    tran.Sets.IntersectAndStore(0, item.Item1, item.Item2.ToArray());
                }

                var value = tran.Sets.UnionString(0, nodes.Values.Select(i => i.Item1).ToArray());

                foreach (var item in nodes.Values)
                {
                    tran.Keys.Remove(0, item.Item1);
                }

                await tran.Execute();
                return string.Join(", ", value.Result);
            }
        }

        public async Task<IDictionary<string, string>> Get(string[] names, Node business, Node system)
        {
            var result = new Dictionary<string, Task<string[]>>();
            using (var tran = _client.Reader.CreateTransaction())
            {
                var rng = new RNGCryptoServiceProvider();
                var variables = new List<string>();

                foreach (var name in names)
                {
                    var data = new byte[8];

                    rng.GetBytes(data);
                    var suffix = BitConverter.ToUInt64(data, 0);
                    var variable = name + ":" + suffix;

                    var nodes = new Dictionary<Node, Tuple<string, List<string>>> { { business, Tuple.Create("BUSINESS:" + suffix, new List<string>()) }, { system, Tuple.Create("SYSTEM:" + suffix, new List<string>()) } };

                    foreach (var item in nodes)
                    {
                        var node = item.Key;
                        do
                        {
                            item.Value.Item2.Add(name + ":" + node.Name + "." + node.Id);
                            node = node.Next;
                        } while (node != null);
                    }

                    foreach (var item in nodes.Values)
                    {
                        tran.Sets.IntersectAndStore(0, item.Item1, item.Item2.ToArray());
                    }

                    tran.Sets.UnionAndStore(0, variable, nodes.Values.Select(i => i.Item1).ToArray());

                    foreach (var item in nodes.Values)
                    {
                        tran.Keys.Remove( 0, item.Item1);
                    }

                    variables.Add(variable);
                }

                foreach (var variable in variables)
                {
                    result.Add(variable, tran.Sets.GetAllString(0, variable));
                    tran.Keys.Remove(0, variable);
                }

                await tran.Execute();
            }

            var newResult = result.GroupBy(k => k.Key.Substring(0, k.Key.IndexOf(":"))).Select(g => new { Key = g.Key, Value = g.Where(k => k.Value.IsCompleted && k.Value.Result.Length > 0).Select(k => string.Join(", ", k.Value.Result)).FirstOrDefault() }).Where(k => k.Value != null).ToDictionary(k => k.Key, k => k.Value);
            return newResult;
        }

        //public async Task<IDictionary<string, string>> Get(Node business, Node system)
        //{
        //    return null;
        //}
    }
}