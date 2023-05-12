using AuthEventTrackers.Domains.Entities;
using AuthEventTrackers.Domains.Response;
using AuthEventTrackers.Infra;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AuthEventTrackers
{
    public class AuthorizationClient
    {
        private HttpRequest           _httpRequest;
        private List<Claim>           _userClaims = new List<Claim>();
        private string                _tokenAccess = "";

        private readonly IMemoryCache _memoryCache;
        private readonly HttpClient   _httpClient;
        private readonly string       _cacheName = "AllProfilesModules";
        private readonly LoggerClient _log       = new LoggerClient();

        public AuthorizationClient(IMemoryCache memoryCache, HttpClient httpClient)
        {
            _memoryCache = memoryCache;
            _httpClient = httpClient;
        }

        private string GetHeader(string key)
        {
            _httpRequest.Headers.TryGetValue(key, out var value);
            return value;
        }

        private Guid GetCorrelationId()
        {
            if (Guid.TryParse(GetHeader("X-Correlation-Id"), out Guid correlationId))
            {
                return correlationId;
            }
            else
            {
                return Guid.NewGuid();
            }
        }

        private async Task<ProfileModuleEntity> GetProfileModule(string moduleCode, List<Guid> profileId)
        {
            if (!_memoryCache.TryGetValue(_cacheName, out List<ProfileModuleEntity> list))
            {
                var httpClient = new HttpClientApi(_httpClient);

                list = await httpClient.GetAsyncProfilesModulues(_tokenAccess);
                _memoryCache.Set(_cacheName, list, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) });
            }

            return list.FirstOrDefault(x => x.ModuleCode == moduleCode && profileId.Contains(x.ProfileId));
        }

        private bool HasMethodAccess(ProfileModuleEntity entity, string method)
        {
            if (entity == null) return false;

            switch (method.ToUpperInvariant())
            {
                case "GET":
                    return entity.Get;

                case "POST":
                    return entity.Post;

                case "PUT":
                case "PATCH":
                    return entity.Put;

                case "DELETE":
                    return entity.Delete;

                default:
                    return false;
            }
        }

        private Guid GetClaimGuid(string type)
        {
            var claim = _userClaims.FirstOrDefault(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase));
            return claim != null && Guid.TryParse(claim.Value, out Guid claimGuid) ? claimGuid : Guid.Empty;
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

        public void DestroyCache()
        {
            _memoryCache.Remove(_cacheName);
        }

        public async Task<AuthorizationResponse> Authorization(
            ClaimsPrincipal         user,
            HttpRequest             httpRequest,
            bool                    flagPermissao  = true,
            [CallerFilePath] string sourceFilePath = "")
        {
            _httpRequest        = httpRequest;
            _userClaims         = user.Claims.ToList();
            _tokenAccess        = GetHeader("Authorization");

            var method          = httpRequest.Method;
            var module          = Path.GetFileNameWithoutExtension(sourceFilePath).Replace("Controller", "");
            var profilesId      = GetClaimListGuid("Perfis");
            var profileModule   = await GetProfileModule(module, profilesId);
            var isAccessAllowed = HasMethodAccess(profileModule, method);

            var authorizationResponse = new AuthorizationResponse
            {
                CorrelationId   = GetCorrelationId(),
                UserId          = GetClaimGuid("UsuarioId"),
                PersonId        = GetClaimGuid("PessoaId"),
                ProfilesId      = profilesId,
                IsAccessAllowed = isAccessAllowed,
                AccessLevel     = profileModule?.AccessLevel,
                ModuleName      = module,
                Method          = method
            };

            if (profileModule == null && flagPermissao)
            {
                _log.Warning("access", authorizationResponse, response: $"The module {method}/{module} is not associated with any user profile.");
                throw new UnauthorizedAccessException();
            }

            if (!isAccessAllowed && flagPermissao)
            {
                _log.Warning("access", authorizationResponse, response: $"None of the user's profiles have permission to access module {method}/{module}.");
                throw new UnauthorizedAccessException();
            }

            return authorizationResponse;
        }
    }
}
