using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using RestSharp;
using SharedAPIClient.Models;

namespace SharedAPIClient.Helpers
{
    /// <summary>
    /// Helper uses for performing requests to SharedAPI
    /// </summary>
    public class RestClient
    {
        internal RetryPolicy RetryPolicy;
        private static RetryPolicy SetRetryPolicy(int count)
        {
            ITransientErrorDetectionStrategy strategy = new HttpTransientErrorDetectionStrategy(false);
            var maxBackoff = TimeSpan.FromSeconds(1024d);
            var deltaBackoff = TimeSpan.FromSeconds(2d);
            var minBackoff = TimeSpan.FromSeconds(0);

            var exponentialBackoff = new ExponentialBackoff(count, minBackoff, maxBackoff, deltaBackoff);
            return new RetryPolicy(strategy, exponentialBackoff);
        }

        private string RequestBaseUrl { get; set; }
        private static string TokenGrantType { get; set; }
        private static string TokenContextType { get; set; }

        static RestClient()
        {
            TokenGrantType = ConfigurationManager.AppSettings[Constants.TokenGrantTypeConfigurationKey];
            TokenContextType = ConfigurationManager.AppSettings[Constants.TokenContextTypeConfigurationKey];
        }
        public RestClient(string requestBaseUrl)
        {
            
            if (GlobalConfiguration.Configuration.Formatters.All(formatter => formatter as JsonHalMediaTypeFormatter == null))
                GlobalConfiguration.Configuration.Formatters.Add(new JsonHalMediaTypeFormatter());    
            
            RetryPolicy =  SetRetryPolicy(10);
            RequestBaseUrl = requestBaseUrl;
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        /// <summary>
        /// Authenticate user
        /// </summary>
        public async Task<Token> GetAccessToken(string rootApiUrl, string clientId, string securePair, string userName, string password)
        {
            
            return await MakeRequest<Token>(string.Concat(rootApiUrl, "/", Constants.TokenAction), Method.POST, string.Empty,
                new[]
                {
                    new Parameter {Name = Constants.TokenGrantTypeName, Value = TokenGrantType},
                    new Parameter {Name = Constants.TokenUserName, Value = userName},
                    new Parameter {Name = Constants.TokenPassword, Value = password}
                },
                null,
                new[]
                {
                    new HeaderParameter
                    {
                        Name = Constants.AuthorizationHeaderName,
                        Value = FormAuthorizationString(clientId, securePair)
                            
                    },
                    new HeaderParameter {Name = Constants.ContentTypeHeaderName, Value = TokenContextType}
                }
                , false
                );
        }

        private string FormAuthorizationString(string clientId, string secretPair)
        {
            string paintText = string.Concat(clientId, ":", secretPair);
            return string.Concat(Constants.AuthBasic, " ", Base64Encode(paintText));
        }

        /// <summary>
        /// Perform request to SharedAPI based on parameters
        /// </summary>
        /// <typeparam name="TJsonResponseType">Type for deserializing SharedAPI response</typeparam>
        /// <param name="resource">ex.: /api/v1/vehicle</param>
        /// <param name="httpMethod">HTTP method: ex.: GET, POST, PUT</param>
        /// <param name="token">Authentication token</param>
        /// <param name="parameters">POST or URL parameter based on Method</param>
        /// <param name="urlParameters">Parameters intented to replaces matching token in request.Resource</param>
        /// <param name="headerParameters">HTTP Header parameters (ex.: Accept, Content-Type)</param>
        /// <param name="addAcceptHeader">if this flag is true JsonHalMediaTypeFormatter formatter will use to deserialize response</param>
        /// <param name="body">body for POST data</param>
        /// <returns>Returns response according to generic type</returns>
        public async Task<TJsonResponseType> MakeRequest<TJsonResponseType>(string resource,
            Method httpMethod, string token, IEnumerable<Parameter> parameters, IEnumerable<UrlParameter> urlParameters,
            IEnumerable<HeaderParameter> headerParameters, bool addAcceptHeader = true, object body = null)
            where TJsonResponseType : class
        {
            var client = new RestSharp.RestClient(RequestBaseUrl);

            var request = new RestRequest(resource, httpMethod);

            SetupRequest<TJsonResponseType>(token, parameters, urlParameters, headerParameters, body, addAcceptHeader, request);

            if (!addAcceptHeader )
            {
                CancellationToken cancellationToken = new CancellationToken();
                var result = await RetryPolicy.ExecuteAsync(() => client.ExecuteTaskAsync<TJsonResponseType>(request, cancellationToken))
                                       .ConfigureAwait(false);
                CheckResponse(result);
                return result.Data;
            }

            IRestResponse data = await client.ExecuteTaskAsync(request);
            CheckResponse(data);

            var formatter = new JsonHalMediaTypeFormatter();
            var memoryStream = new MemoryStream(data.RawBytes) { Position = 0 };
            var representation = await formatter.ReadFromStreamAsync(typeof (TJsonResponseType), memoryStream, null, null);
            return representation as TJsonResponseType;

        }

        /// <summary>
        /// Checks returned response
        /// </summary>
        /// <param name="result"></param>
        private static void CheckResponse(IRestResponse result)
        {
            if (result == null || result.StatusCode != HttpStatusCode.OK)
            {
                throw result == null
                    ? new HttpException((int) HttpStatusCode.BadRequest, string.Empty)
                    : new HttpException((int) result.StatusCode, result.Content);
            }
        }

        /// <summary>
        /// Setup request to SharedAPI
        /// </summary>
        private static void SetupRequest<TJsonResponseType>(string token, IEnumerable<Parameter> parameters, IEnumerable<UrlParameter> urlParameters,
            IEnumerable<HeaderParameter> headerParameters, object body, bool addAcceptHeader, RestRequest request) 
            where TJsonResponseType : class
        {
            if (parameters != null)
                // adds to POST or URL querystring based on Method
                parameters.ToList().ForEach(p => request.AddParameter(p.Name, p.Value));

            if (urlParameters != null)
                // replaces matching token in request.Resource
                urlParameters.ToList().ForEach(p => request.AddUrlSegment(p.Name, p.Value));

            if (body != null)
            {
                request.RequestFormat = DataFormat.Json;
                request.AddBody(body);
            }

            if (headerParameters != null)
               headerParameters.ToList().ForEach(p => request.AddHeader(p.Name, p.Value));

            if (!string.IsNullOrWhiteSpace(token) && !IsHeaderExist(headerParameters, Constants.AuthorizationHeaderName))
                   request.AddHeader(Constants.AuthorizationHeaderName , string.Concat(Constants.AuthorizationType, " ", token));

            if (IsHeaderExist(headerParameters, Constants.ContentTypeHeaderName))
                request.AddHeader(Constants.ContentTypeHeaderName, Constants.DefaultContentTypeHeaderValue);

            if (addAcceptHeader && !IsHeaderExist(headerParameters, Constants.AcceptHeaderName))
                request.AddHeader(Constants.AcceptHeaderName, Constants.DefaultAcceptHeaderValue);
        }
        /// <summary>
        /// Checks if HTTP header exist
        /// </summary>
        private static bool IsHeaderExist(IEnumerable<HeaderParameter> headers, string headerName)
        {
            if (headers == null)
                return false;

            return headers.ToList().Any(h => String.Compare(h.Name.ToLower(CultureInfo.InvariantCulture), headerName.ToLower(CultureInfo.InvariantCulture),
                StringComparison.Ordinal) == 0);
        }
    }
}
