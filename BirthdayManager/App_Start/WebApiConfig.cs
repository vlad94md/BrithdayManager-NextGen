using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BirthdayManager
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var setttings = config.Formatters.JsonFormatter.SerializerSettings;
            setttings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            setttings.Formatting = Formatting.Indented;

            config.MapHttpAttributeRoutes();
            config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("multipart/form-data"));

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
