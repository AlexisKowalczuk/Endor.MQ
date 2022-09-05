namespace Endor.MQ.Model.Config
{
  public class MQQueueConfiguration
  {
	  public string Id { get; set; }

		public string Name { get; set; }

    public bool Durable { get; set; }

    public bool Exclusive { get; set; }

    public bool AutoDelete { get; set; }

    public string RoutingKey { get; set; }
  }
}