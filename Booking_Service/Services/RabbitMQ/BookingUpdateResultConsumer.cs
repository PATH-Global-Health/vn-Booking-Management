using Data.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Services.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.RabbitMQ
{
    public class BookingUpdateResultConsumer : BackgroundService
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string syncQueue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;
        private EventingBasicConsumer consumer;

        public BookingUpdateResultConsumer(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            var factory = new ConnectionFactory();
            _configuration.Bind("RabbitMqConnection", factory);
            syncQueue = _configuration.GetValue<string>("UpdateBookingResult");
            factory.ClientProvidedName = syncQueue + " | Consumer";
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            channel.QueueDeclare(queue: syncQueue, durable: false,
              exclusive: false, autoDelete: false, arguments: null);
            channel.BasicQos(0, 1, false);
            consumer = new EventingBasicConsumer(channel);
            channel.BasicConsume(queue: syncQueue,
              autoAck: false, consumer: consumer);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            consumer.Received += (model, ea) =>
            {
                string response = null;

                var body = ea.Body.ToArray();
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                try
                {
                    var message = Encoding.UTF8.GetString(body);
                    var result = UpdateFromExamination(message);

                    response = JsonConvert.SerializeObject(result);
                }
                catch (Exception e)
                {
                    var result = new ResultModel();
                    result.Succeed = false;
                    result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
                    response = JsonConvert.SerializeObject(result);
                }
                finally
                {
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                      basicProperties: replyProps, body: responseBytes);
                    channel.BasicAck(deliveryTag: ea.DeliveryTag,
                      multiple: false);
                }

            };
            return Task.CompletedTask;
        }

        private ResultModel UpdateFromExamination(string message)
        {
            var result = new ResultModel();
            try
            {
                // get service scope
                using (var scope = _scopeFactory.CreateScope())
                {
                    // get service instance
                    var examService = scope.ServiceProvider.GetRequiredService<IExaminationService>();

                    // 
                    var model = JsonConvert.DeserializeObject<ExaminationUpdateResultModel>(message);
                    // sync
                    result = examService.UpdateResult(model).Result;
                }
            }
            catch (Exception e)
            {
                result.Succeed = false;
                result.ErrorMessage = e.InnerException != null ? e.InnerException.Message + "\n" + e.StackTrace : e.Message + "\n" + e.StackTrace;
            }
            return result;
        }
    }
}
