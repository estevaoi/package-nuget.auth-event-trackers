using AuthEventTrackers.Domains.Enum;
using System;
using System.Collections.Generic;

namespace AuthEventTrackers.Domains.Response
{
    public class AuthorizationResponse
    {
        private readonly Guid _correlationId;
        public AuthorizationResponse(Guid correlationId)
        {
            _correlationId = correlationId;
        }

        public AuthorizationResponse() { }

        public Guid             CorrelationId    { get; set; }
        public Guid             PersonId         { get; set; }
        public Guid             UserId           { get; set; }
        public List<Guid>       ProfilesId       { get; set; }
        public bool             IsAccessAllowed  { get; set; }
        public AccessLevelEnum? AccessLevel      { get; set; } = 0;
        public string           ModuleName       { get; set; }
        public string           Method           { get; set; }
    }

}
