using AuthEventTrackers.Domains.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace AuthEventTrackers.Infra
{
    internal class HttpClientApi
    {
        private HttpClient _httpClient;

        public HttpClientApi(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public List<ProfileModuleEntity> GetAsyncProfilesModulues( string token)
        {
            string requestUri = Environment.GetEnvironmentVariable("URL_BASE_PROFILES_MODULES");

            using HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);

            httpRequestMessage.Headers.Add("Authorization", token);

            try
            {
                var response =  _httpClient.Send(httpRequestMessage);
                //var content  =  response.Content.Re;

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<List<ProfileModuleEntity>>("");
                }
                else
                {
                    throw new Exception("");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
