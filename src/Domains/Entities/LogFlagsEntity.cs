namespace AuthEventTrackers.Domains.Entities
{
    internal class LogFlagsEntity
    {
        public bool? Error   { get; set; }
        public bool? Info    { get; set; }
        public bool? Success { get; set; }
        public bool? Warning { get; set; }
    }
}
