using AuthEventTrackers.Domains.Entities;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace AuthEventTrackers
{
    public static class RabbitClient
    {
        public static RabbitEntity GetRabbitConfig()
        {
            var rabbitEnv = Environment.GetEnvironmentVariable("MQ_RABBITMQ_DEFAULT") ?? "";
            return JsonConvert.DeserializeObject<RabbitEntity>(rabbitEnv) ?? new RabbitEntity();
        }

        public static string GetQueueRabbit(string key)
        {
            var rabbitConfig = GetRabbitConfig();
            return rabbitConfig.Queue.Find(x => x.Key == key).Name;
        }

        public static void SendMessage(object message, string routingKey)
        {
            var rabbitConfig = GetRabbitConfig();

            if (!rabbitConfig.IsEnable) return;

            try
            {
                var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                using var connection = rabbitConfig.Connection.CreateConnection();
                using var channel = connection.CreateModel();

                channel.BasicPublish(
                    exchange: rabbitConfig.Exchange,
                    routingKey: routingKey,
                    basicProperties: null,
                    body: messageBody
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to post message. Exception: {ex}");
            }
        }
    }
}
