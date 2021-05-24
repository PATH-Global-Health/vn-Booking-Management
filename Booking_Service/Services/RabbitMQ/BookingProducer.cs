using Data.ViewModels;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;

namespace Services.RabbitMQ
{
    public interface IBookingProducer : IDisposable
    {
        string Call(string message, string queue);
        void Close();
    }

    public class BookingProducer : IBookingProducer
    {
        private IConnection connection;
        private IModel channel;
        //private readonly string replyQueueName;
        private string queueName;
        private readonly int timeout;
        //private readonly EventingBasicConsumer consumer;
        private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
        //private readonly IBasicProperties props;
        private readonly IConfiguration _configuration;
        private ConnectionFactory _factory;

        public BookingProducer(IConfiguration configuration)
        {
            _configuration = configuration;
            _factory = new ConnectionFactory();
            _configuration.Bind("RabbitMqConnectionV2", _factory);
            
            timeout = _configuration.GetValue<int>("RabbitMQRequestTimeout");
        }

        public string Call(string message, string queue)
        {
            try
            {
                queueName = _configuration.GetValue<string>(queue);
                _factory.ClientProvidedName = queueName + " | Booking";
                connection = _factory.CreateConnection();
                channel = connection.CreateModel();
                var replyQueueName = "amq.rabbitmq.reply-to";
                //replyQueueName = channel.QueueDeclare().QueueName;
                var consumer = new EventingBasicConsumer(channel);
                var props = channel.CreateBasicProperties();
                //
                var correlationId = Guid.NewGuid().ToString();
                props.CorrelationId = correlationId;
                props.ReplyTo = replyQueueName;

                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var response = Encoding.UTF8.GetString(body);
                    if (ea.BasicProperties.CorrelationId == correlationId)
                    {
                        respQueue.Add(response);
                    }
                };

                //
                var messageBytes = Encoding.UTF8.GetBytes(message);
                channel.BasicConsume(
                    consumer: consumer,
                    queue: replyQueueName,
                    autoAck: true);
                channel.BasicPublish(
                    exchange: "",
                    routingKey: queueName,
                    basicProperties: props,
                    body: messageBytes);

                respQueue.TryTake(out string item, timeout);

                if (string.IsNullOrEmpty(item))
                {
                    var result = new ResultModel();
                    result.ErrorMessage = "RabbitMQ request timeout.";
                    var responseMessage = JsonConvert.SerializeObject(result);
                    return responseMessage;
                }

                return item;
            }
            catch (Exception e)
            {
                var result = new ResultModel();
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message : e.Message;
                return JsonConvert.SerializeObject(result);
            }
            finally
            {
                Close();
            }
        }

        public void Close()
        {
            if (channel != null)
                channel.Close();
            if (connection != null)
                connection.Close();
        }

        public void Dispose()
        {
            Close();
        }
    }
}
