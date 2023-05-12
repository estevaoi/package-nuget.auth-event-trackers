using AuthEventTrackers.Domains.Enum;
using Newtonsoft.Json;
using System;

namespace AuthEventTrackers.Domains.Entities
{
    internal class ProfileModuleEntity
    {
        public Guid            ProfileId       { get; set; }
        public Guid            ModuleId        { get; set; }
        public string          ModuleCode      { get; set; }
        public AccessLevelEnum AccessLevel     { get; set; } = AccessLevelEnum.BASIC_ACCESS;
        public bool            Get             { get; set; }
        public bool            Post            { get; set; }
        public bool            Put             { get; set; }
        public bool            Delete          { get; set; }
    }
}
