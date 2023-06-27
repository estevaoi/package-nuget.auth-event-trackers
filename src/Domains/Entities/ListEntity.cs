using Newtonsoft.Json;

namespace AuthEventTrackers.Domains.Entities
{
    internal class ListEntity<T>
    {
        [JsonProperty("itens")] public List<T> Items { get; set; }
    }
}
