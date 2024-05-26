using RabbitMQ.Client;
using System.Text;

namespace OnlineBankingApplication.Services
{
    public class MessageQueueService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageQueueService(string hostname, string queueName)
        {
            var factory = new ConnectionFactory() { HostName = hostname, Port = 5672 };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        }

        public void Publish(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: "transactions", basicProperties: null, body: body);
        }

        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }
    }
}
