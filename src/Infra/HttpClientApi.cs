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

        public async Task<List<ProfileModuleEntity>> GetAsyncProfilesModulues( string token)
        {
            string requestUri = Environment.GetEnvironmentVariable("URL_BASE_PROFILES_MODULES");

            using HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);

            httpRequestMessage.Headers.Add("Authorization", token);

            try
            {
                var response = await _httpClient.SendAsync(httpRequestMessage);
                var content  = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<List<ProfileModuleEntity>>(content);
                }
                else
                {
                    throw new Exception(content);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
