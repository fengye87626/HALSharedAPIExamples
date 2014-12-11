using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web;
using Newtonsoft.Json;
using SharedAPIClient.Models;
using WebApi.Hal;
using WebApi.Hal.Interfaces;
using WebApi.Hal.JsonConverters;


namespace SharedAPIClient.Helpers
{
    /// <summary>
    /// Formatter for JsonHall data
    /// </summary>
    public class JsonHalMediaTypeFormatter : JsonMediaTypeFormatter
    {
#pragma warning disable 618
        private readonly ResourceListConverter resourceListConverter = new ResourceListConverter();
#pragma warning restore 618
        private readonly ResourceConverter resourceConverter = new ResourceConverter();
        private readonly LinksConverter linksConverter = new LinksConverter();

        public JsonHalMediaTypeFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/hal+json"));
            SerializerSettings.Converters.Add(linksConverter);
            SerializerSettings.Converters.Add(resourceListConverter);
            SerializerSettings.Converters.Add(resourceConverter);
        }

        public override bool CanReadType(Type type)
        {
            return typeof(Representation).IsAssignableFrom(type);
        }

        public override bool CanWriteType(Type type)
        {
            return typeof(Representation).IsAssignableFrom(type);
        }
    }
}
