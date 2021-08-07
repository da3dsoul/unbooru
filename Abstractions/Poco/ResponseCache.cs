using System;
using System.Net;
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace ImageInfrastructure.Abstractions.Poco
{
    public class ResponseCache
    {
        public int ResponseCacheId { get; set; }
        public string Uri { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Response { get; set; }
        public HttpStatusCode? StatusCode { get; set; }
    }
}