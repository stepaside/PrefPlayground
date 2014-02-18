using Nancy;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;

namespace PrefPlayground
{
    public class PreferenceModule : NancyModule
    {
        PreferenceManager pref = new PreferenceManager();

        public PreferenceModule()
        {
            Post["/add/{name}/{value}", true] = async (parameters, cancel) =>
            {
                var values = GetRequestValues();   
                var system = GetSystemAxis(values);
                var business = GetBusinessAxis(values);

                var result = await pref.Add((string)parameters.name, (string)parameters.value, business, system);

                return result ? HttpStatusCode.OK : HttpStatusCode.BadRequest;        
            };

            Put["/{name}/{value}", true] = async (parameters, cancel) =>
            {
                var values = GetRequestValues();
                var system = GetSystemAxis(values);
                var business = GetBusinessAxis(values);

                var result = await pref.Set((string)parameters.name, (string)parameters.value, business, system);

                return result ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
            };

            Delete["/{name}/{value}", true] = async (parameters, cancel) =>
            {
                var values = GetRequestValues();
                var system = GetSystemAxis(values);
                var business = GetBusinessAxis(values);

                var result = await pref.Remove((string)parameters.name, (string)parameters.value, business, system);

                return result ? HttpStatusCode.OK : HttpStatusCode.BadRequest;
            };

            Get["/{name}", true] = async (parameters, cancel) =>
            {
                var system = GetSystemAxis(Request.Query);
                var business = GetBusinessAxis(Request.Query);
                return await pref.Get(parameters.name, business, system);
            };

            Get["/(?<names>\\w+(,\\w+)+)", true] = async (parameters, cancel) =>
            {
                var system = GetSystemAxis(Request.Query);
                var business = GetBusinessAxis(Request.Query);
                return await pref.Get(((string)parameters.names).Split(','), business, system);
            };
        }

        private dynamic GetRequestValues()
        {
            dynamic values;
            if (Request.Form.Count > 0)
            {
                values = Request.Form;
            }
            else if (Request.Query.Count > 0)
            {
                values = Request.Query;
            }
            else
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    var dd = new DynamicDictionary();
                    foreach (var pair in reader.ReadToEnd().Split('&').Select(t => t.Split('=')).Where(t => t.Length == 2))
                    {
                        long value;
                        if (long.TryParse(pair[1], out value))
                        {
                            dd.Add(pair[0], new DynamicDictionaryValue(value));
                        }
                    }
                    values = dd;
                }
            }
            return values;
        }

        private Node GetSystemAxis(dynamic parameters)
        {
            Node system = null;
            if (parameters.instance.HasValue)
            {
                system = new Node { Id = (long)parameters.instance, Name = "instance" }
                    .Link(parameters.module_version ? (long)parameters.module_version : 0, "module_version")
                        .Link(parameters.module.HasValue ? (long)parameters.module : 0, "module")
                        .Link(parameters.application_version.HasValue ? (long)parameters.application_version : 0, "application_version")
                        .Link(parameters.application.HasValue ? (long)parameters.application : 0, "application")
                        .Link(parameters.suite.HasValue ? (long)parameters.suite : 0, "suite")
                        .Link(1, "global").Done();
            }
            else if (parameters.module_version.HasValue)
            {
                system = new Node { Id = (long)parameters.module_version, Name = "module_version" }
                        .Link(parameters.module.HasValue ? (long)parameters.module : 0, "module")
                        .Link(parameters.application_version.HasValue ? (long)parameters.application_version : 0, "application_version")
                        .Link(parameters.application.HasValue ? (long)parameters.application : 0, "application")
                        .Link(parameters.suite.HasValue ? (long)parameters.suite : 0, "suite")
                        .Link(1, "global").Done();
            }
            else if (parameters.module.HasValue)
            {
                system = new Node { Id = (long)parameters.module, Name = "module" }
                        .Link(parameters.application_version.HasValue ? (long)parameters.application_version : 0, "application_version")
                        .Link(parameters.application.HasValue ? (long)parameters.application : 0, "application")
                        .Link(parameters.suite.HasValue ? (long)parameters.suite : 0, "suite")
                        .Link(1, "global").Done();
            }
            else if (parameters.application_version.HasValue)
            {
                system = new Node { Id = (long)parameters.application_version, Name = "application_version" }
                        .Link(parameters.application.HasValue ? (long)parameters.application : 0, "application")
                        .Link(parameters.suite.HasValue ? (long)parameters.suite : 0, "suite")
                        .Link(1, "global").Done();
            }
            else if (parameters.application.HasValue)
            {
                system = new Node { Id = (long)parameters.application, Name = "application" }
                        .Link(parameters.suite.HasValue ? (long)parameters.suite : 0, "suite")
                        .Link(1, "global").Done();
            }
            else if (parameters.suite.HasValue)
            {
                system = new Node { Id = (long)parameters.suite, Name = "suite" }
                         .Link(1, "global").Done();
            }
            else
            {
                system = new Node { Id = 1, Name = "global" };
            }
            return system;
        }

        private Node GetBusinessAxis(dynamic parameters)
        {
            Node business = null;
            if (parameters.profile.HasValue)
            {
                business = new Node { Id = (long)parameters.profile, Name = "profile" }
                        .Link(parameters.signon.HasValue ? (long)parameters.signon : 0, "signon")
                        .Link(parameters.company.HasValue ? (long)parameters.company : 0, "company")
                        .Link(parameters.corporate.HasValue ? (long)parameters.corporate : 0, "corporate")
                        .Link(1, "asi").Done();
            }
            else if (parameters.signon.HasValue)
            {
                business = new Node { Id = (long)parameters.signon, Name = "signon" }
                        .Link(parameters.company.HasValue ? (long)parameters.company : 0, "company")
                        .Link(parameters.corporate.HasValue ? (long)parameters.corporate : 0, "corporate")
                        .Link(1, "asi").Done();
            }
            else if (parameters.company.HasValue)
            {
                business = new Node { Id = (long)parameters.company, Name = "company" }
                        .Link(parameters.corporate.HasValue ? (long)parameters.corporate : 0, "corporate")
                        .Link(1, "asi").Done();
            }
            else if (parameters.corporate.HasValue)
            {
                business = new Node { Id = (long)parameters.corporate, Name = "corporate" }
                        .Link(1, "asi").Done();
            }
            else
            {
                business = new Node { Id = 1, Name = "asi" };
            }
            return business;
        }
    }
}