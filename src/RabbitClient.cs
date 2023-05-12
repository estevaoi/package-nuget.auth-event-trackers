using AuthEventTrackers.Domains.Entities;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Text;

namespace AuthEventTrackers
{
    public static class RabbitClient
    {
        private static readonly RabbitEntity _configRabbit = JsonConvert.DeserializeObject<RabbitEntity>(Environment.GetEnvironmentVariable("MQ_RABBITMQ_GERAL"));

        public static bool SendMessage(object request, string queueName, string exchangeName = "")
        {

            if (!(bool)_configRabbit.IsEnable) return true;            

            try
            {
                var factory = new ConnectionFactory()
                {
                    HostName = _configRabbit.Host,
                    Port     = _configRabbit.Port,
                    UserName = _configRabbit.Username,
                    Password = _configRabbit.Password
                };

                using (var connection = factory.CreateConnection())
                using (var channel    = connection.CreateModel())
                {
                    channel.QueueDeclare(queue:      queueName,
                                         durable:    true,
                                         exclusive:  false,
                                         autoDelete: false,
                                         arguments:  null);

                    channel.BasicPublish(exchange:        exchangeName,
                                         routingKey:      queueName,
                                         basicProperties: null,
                                         body:            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request)));
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to post message. Exception: {ex}");
                return false;
            }
        }
    }
}
