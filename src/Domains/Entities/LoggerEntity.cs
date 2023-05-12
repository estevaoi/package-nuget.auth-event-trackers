using System.Collections.Generic;

namespace AuthEventTrackers.Domains.Entities
{
    internal class RabbitEntity
    {
        public string         Host     { get; set; }
        public int            Port     { get; set; }
        public string         Username { get; set; }
        public string         Password { get; set; }
        public bool?          IsEnable { get; set; } = false;
        public QueueEntity    Queue    { get; set; }
        public LogFlagsEntity LogFlags { get; set; }
    }
}
