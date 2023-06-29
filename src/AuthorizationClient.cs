using AuthEventTrackers.Domains.Entities;
using AuthEventTrackers.Domains.Response;
using AuthEventTrackers.Infra;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System.Security.Claims;

namespace AuthEventTrackers
{
    public class AuthorizationClient
    {
        private List<Claim> _userClaims = new List<Claim>();
        private string _tokenAccess = "";
        private string _cacheName = "AllProfilesModules";
        private HttpRequest _httpRequest;

        private readonly IMemoryCache _memoryCache;

        #region Private methods 

        private string GetHeaderValue(string headerKey)
        {
            try
            {
                _httpRequest.Headers.TryGetValue(headerKey, out var headerValue);
                return headerValue;
            }
            catch (Exception ex)
            {
                throw new Exception($"Não foi possível obter o cabeçalho {headerKey}. Erro: {ex.Message}");
            }
        }

        private Guid GetCorrelationId()
        {
            if (Guid.TryParse(GetHeaderValue("X-Correlation-Id"), out Guid correlationId))
            {
                return correlationId;
            }
            else
            {
                return Guid.NewGuid();
            }
        }

        private Guid GetClaimGuid(string type)
        {
            var claim = _userClaims.FirstOrDefault(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase));
            return claim != null && Guid.TryParse(claim.Value, out Guid claimGuid) ? claimGuid : Guid.Empty;
        }

        private ProfileModuleEntity GetProfileModule(string moduleCode, List<Guid> profileId)
        {
            try
            {
                if (!_memoryCache.TryGetValue(_cacheName, out List<ProfileModuleEntity> list))
                {
                    list = new ProfilesModulesServices().GetProfilesModulues(_tokenAccess);
                    _memoryCache.Set(_cacheName, list, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
                }

                return list.FirstOrDefault(x => x.ModuleCode == moduleCode && profileId.Contains(x.ProfileId));
            }
            catch (Exception ex)
            {
                throw new Exception($"Não foi possível obter o perfil/modulo. Erro: {ex.Message}");
            }
        }

        private bool HasMethodAccess(ProfileModuleEntity entity, string method)
        {
            if (entity == null) return false;

            return (method ?? "").ToUpperInvariant() switch
            {
                "GET" => entity.Get,
                "POST" => entity.Post,
                "PUT" => entity.Put,
                "PATCH" => entity.Put,
                "DELETE" => entity.Delete,
                _ => false
            };
        }

        private List<Guid> GetClaimListGuid(string type)
        {
            var claim = _userClaims.FirstOrDefault(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase));

            if (claim != null)
            {
                if (Guid.TryParse(claim.Value, out Guid claimGuid))
                {
                    return new List<Guid>() { claimGuid };
                }
                else if (JsonConvert.DeserializeObject<List<Guid>>(claim.Value) is List<Guid> guidList)
                {
                    return guidList;
                }
            }

            return new List<Guid>();
        }

        #endregion

        public AuthorizationClient(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void DestroyCache()
        {
            _memoryCache.Remove(_cacheName);
        }

        public AuthorizationResponse Authorization(HttpRequest httpRequest, ClaimsPrincipal user)
        {
            _httpRequest = httpRequest;
            _userClaims = user.Claims.ToList();
            _tokenAccess = GetHeaderValue("Authorization");

            var routes = (((dynamic)_httpRequest).RouteValues as IReadOnlyDictionary<string, object>).Values.ToList();

            var metodo = routes[0];
            var module = routes[1].ToString().Replace("Controller", "");

            var method = _httpRequest.Method;

            var profilesId = GetClaimListGuid("Perfis");
            var profileModule = GetProfileModule(module, profilesId);
            var isAccessAllowed = HasMethodAccess(profileModule, method);

            var authorizationResponse = new AuthorizationResponse
            {
                CorrelationId = GetCorrelationId(),
                UserId = GetClaimGuid("UsuarioId"),
                PersonId = GetClaimGuid("PessoaId"),
                ProfilesId = profilesId,
                IsAccessAllowed = isAccessAllowed,
                AccessLevel = profileModule?.AccessLevel,
                ModuleName = module,
                Method = method
            };

            if (!isAccessAllowed && profileModule != null)
            {
                throw new UnauthorizedAccessException();
            }

            return authorizationResponse;
        }
    }
}
