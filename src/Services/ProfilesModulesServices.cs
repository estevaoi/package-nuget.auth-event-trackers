using AuthEventTrackers.Domains.Entities;
using Newtonsoft.Json;
using RestSharp;

namespace AuthEventTrackers.Infra
{
    internal class ProfilesModulesServices
    {
        private RouterApiProfilesModulesEntity GetRouterApi()
        {
            var valueVariable = Environment.GetEnvironmentVariable("URL_BASE_PROFILES_MODULES") ?? throw new Exception("The route for the profiles and modules API has not been defined.");
            return JsonConvert.DeserializeObject<RouterApiProfilesModulesEntity>(valueVariable) ?? throw new Exception("The route for the profiles and modules API has not been defined.");
        }

        public List<ProfileModuleEntity> GetProfilesModulues(string token)
        {
            try
            {
                var routerApi = GetRouterApi();

                var client = new RestClient(routerApi.Url);
                var request = new RestRequest(routerApi.Uri);
                request.AddHeader("Authorization", token);

                foreach (var item in routerApi.Parameters)
                {
                    request.AddParameter((string)item.Name, (string)item.Value);
                }

                var response = client.ExecuteGet<List<ProfileModuleEntity>>(request);

                if (!response.IsSuccessful)
                {
                    throw new Exception($"ERROR: {response.ErrorException?.Message}. Content: {response?.Content!}");
                }

                var data = JsonConvert.DeserializeObject<List<ProfileModuleEntity>>(response.Content!);

                return data!;

            }
            catch (Exception)
            {
                throw new Exception("Unable to get list of profiles and modules");
            }
        }

    }
}
