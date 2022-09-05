namespace Endor.MQ.Model.Config
{
  public class MQExchangeConfiguration
  {
	  public string Id { get; set; }

		public string Name { get; set; }

    public string Type { get; set; }

    public bool Durable { get; set; }

    public bool AutoDelete { get; set; }
  }
}
