using Newtonsoft.Json;
using System.Collections.Generic;

namespace AuthEventTrackers.Domains.Entities
{
    internal class ListEntity<T>
    {
        [JsonProperty("itens")] public List<T> Items { get; set; }
    }
}
