using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace PrefPlayground
{

    public class SimplePreferenceManager
    {
        private PreferenceManager _pref = new PreferenceManager();
        private string _prefix = "simple:";

        public async Task<bool> Set(string name, string value, long userId, long appId = 0)
        {
            return await _pref.Set(_prefix + name, value, GetBusinessAxis(userId), GetSystemAxis(appId));
        }

        public async Task<bool> Add(string name, string value, long userId, long appId = 0)
        {
            return await _pref.Add(_prefix + name, value, GetBusinessAxis(userId), GetSystemAxis(appId));
        }

        public async Task<bool> Remove(string name, string value, long userId, long appId = 0)
        {
            return await _pref.Remove(_prefix + name, value, GetBusinessAxis(userId), GetSystemAxis(appId));
        }

        public async Task<IDictionary<string, string>> Get(string[] names, long userId, long appId = 0)
        {
            return await _pref.Get(names.Select(n => _prefix + n).ToArray(), GetBusinessAxis(userId), GetSystemAxis(appId));
        }

        public async Task<string> Get(string name, long userId, long appId = 0)
        {
            return await _pref.Get(_prefix + name, GetBusinessAxis(userId), GetSystemAxis(appId));
        }

        private Node GetBusinessAxis(long userId)
        {
            return new Node { Id = userId, Name = "signon" }.Link(1, "asi").Done();
        }

        private Node GetSystemAxis(long appId)
        {
            if (appId > 0)
            {
                return new Node { Id = appId, Name = "application_version" }.Link(1, "global").Done();
            }
            else
            {
                return new Node { Id = 1, Name = "global" };
            }
        }
    }
}