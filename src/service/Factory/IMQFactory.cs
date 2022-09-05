namespace Endor.MQ.Factory
{
	public interface IMQFactory
	{
		MQConnection GetMQConnection(string exchangeId, string queueId);
	}
}