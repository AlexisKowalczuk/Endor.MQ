using RabbitMQ.Client;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Endor.MQ.Model.Config;

namespace Endor.MQ.Factory
{
  public class MQFactory : IMQFactory
  {
	  #region Fields
	  /// <summary>
	  /// The custom logger
	  /// </summary>
	  private ILogger<MQFactory> logger;
		#endregion

		#region Properties
		public MQProvider MQConnection { get; set; }

		public IList<MQExchangeConfiguration> Exchanges { get; set; }

		public IList<MQQueueConfiguration> Queues { get; set; }
		#endregion

		public MQFactory(ILogger<MQFactory> log, IConfiguration config)
	  {
			logger = log;


			MQConnection = new MQProvider();
		  config.GetSection("MQProvider").Bind(MQConnection);

		  Exchanges = new List<MQExchangeConfiguration>();
		  config.GetSection("MQExchangeConfigurations").Bind(Exchanges);

		  Queues = new List<MQQueueConfiguration>();
		  config.GetSection("MQQueueConfigurations").Bind(Queues);
	  }

    public MQConnection GetMQConnection(string exchangeId, string queueId)
    {
	    var factory = new ConnectionFactory();
      factory.HostName = MQConnection.HostName;
      factory.UserName = MQConnection.UserName;
      factory.Password = MQConnection.Password;

      var exchange = Exchanges.FirstOrDefault(x => x.Id == exchangeId);

      var queue = Queues.FirstOrDefault(x => x.Id == queueId);

			return new MQConnection(logger, factory, exchange, queue);
    }

  }
}
