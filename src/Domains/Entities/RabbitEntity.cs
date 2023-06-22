using RabbitMQ.Client;

namespace AuthEventTrackers.Domains.Entities
{
    public class RabbitEntity
    {
        public bool               IsEnable     { get; set; } = false;
        public string             VirtualHost  { get; set; }
        public string             Exchange     { get; set; }
        public ConnectionFactory  Connection   { get; set; }
        public List<QueueEntity>  Queue        { get; set; }
        public LogFlagsEntity     LogFlags     { get; set; }
    }
}
