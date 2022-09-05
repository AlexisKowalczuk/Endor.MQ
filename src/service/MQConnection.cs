using System;
using System.Threading;
using System.Threading.Tasks;
using Endor.MQ.Helper;
using Endor.MQ.Model;
using Endor.MQ.Model.Config;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.MessagePatterns;

namespace Endor.MQ
{
  public class MQConnection
  {
    private readonly ConnectionFactory _factory;

    private MQExchangeConfiguration _exchange;

    private MQQueueConfiguration _queue;

    private ILogger logger;

		public MQConnection(ILogger logger, ConnectionFactory factory, MQExchangeConfiguration exchange, MQQueueConfiguration queue)
    {
			this.logger = logger;

			this._factory = factory;
      this._exchange = exchange;
      this._queue = queue;
	      
		}
		
    public void SendMessage<T>(T message, string routingKey)
    {
	    logger.LogInformation($"Sending new message routing key [{routingKey}]");
			
			using (var connection = _factory.CreateConnection())
      {
        using (var channel = connection.CreateModel())
        {
					channel.ExchangeDeclare(_exchange.Name, _exchange.Type, _exchange.Durable, _exchange.AutoDelete);

					if (_queue != null)
					{
						channel.QueueDeclare(_queue.Name, _queue.Durable, _queue.Exclusive, _queue.AutoDelete, null);
						channel.QueueBind(_queue.Name, _exchange.Name, _queue.RoutingKey);
					}

					channel.BasicPublish(_exchange.Name, routingKey, null, message.Serialize());

					logger.LogInformation($"Data has been sent");
					channel.Close();
        }
      }

    }

    public async Task ProcessMessagesAsync<T>(Func<T, MQReceptionEventArgs, CancellationToken, Task> action, CancellationToken ct)
    {
	    logger.LogInformation($"Processing new message queue [{_queue.Name}] routing key [{_queue.RoutingKey}]");
	    using (var connection = _factory.CreateConnection())
	    {
		    using (var channel = connection.CreateModel())
		    {
			    channel.ExchangeDeclare(_exchange.Name, _exchange.Type, _exchange.Durable, _exchange.AutoDelete);
			    channel.QueueDeclare(_queue.Name, _queue.Durable, _queue.Exclusive, _queue.AutoDelete, null);
			    channel.QueueBind(_queue.Name, _exchange.Name, _queue.RoutingKey);

			    channel.BasicQos(0, 10, false);

			    var subscription = new Subscription(channel, _queue.Name, false);

			    await Task.Run(async ()  =>
			    {
						while (!ct.IsCancellationRequested)
						{

							try
							{
								BasicDeliverEventArgs deliveryArguments = subscription.Next();

								logger.LogDebug($"Message Receive. Data [{deliveryArguments.Body}]");
								var message = (T)deliveryArguments.Body.DeSerialize(typeof(T));
								var eventArgs = new MQReceptionEventArgs() { Ack = true, RoutingKLey = deliveryArguments.RoutingKey, Redelivered = deliveryArguments.Redelivered };

								//await action(message, eventArgs, ct);
								await action(message, eventArgs, ct);

								if (eventArgs.Ack)
								{
									subscription.Ack(deliveryArguments);
								}
								else
								{
									subscription.Nack(true);
								}
							}
							catch (Exception ex)
							{
								logger.LogError("Error receiving messages", ex);
							}

						}
					}, ct);
					
		    }
	    }
    }

		public void ProcessMessages<T>(Action<T, MQReceptionEventArgs> action, CancellationToken ct)
    {
	    logger.LogInformation($"Processing new message queue [{_queue.Name}] routing key [{_queue.RoutingKey}]");
			using (var connection = _factory.CreateConnection())
      {
        using (var channel = connection.CreateModel())
        {
	        channel.ExchangeDeclare(_exchange.Name, _exchange.Type, _exchange.Durable, _exchange.AutoDelete);
	        channel.QueueDeclare(_queue.Name, _queue.Durable, _queue.Exclusive, _queue.AutoDelete, null);
					channel.QueueBind(_queue.Name, _exchange.Name, _queue.RoutingKey);

          channel.BasicQos(0, 10, false);

          var subscription = new Subscription(channel, _queue.Name, false);
					while (!ct.IsCancellationRequested)
          {
            try
            {
              BasicDeliverEventArgs deliveryArguments = subscription.Next();
              
							logger.LogDebug($"Message Receive. Data [{deliveryArguments.Body}]");
							var message = (T)deliveryArguments.Body.DeSerialize(typeof(T));
              var eventArgs = new MQReceptionEventArgs() { Ack = true, RoutingKLey = deliveryArguments.RoutingKey, Redelivered = deliveryArguments.Redelivered };

							action(message, eventArgs);

              if (eventArgs.Ack)
              {
                subscription.Ack(deliveryArguments);
              }
              else
              {
	              subscription.Nack(true);
              }
            }
            catch (Exception ex)
            {
	            logger.LogError("Error receiving messages", ex);
            }

          }
        }
      }
    }
    
  }
}