namespace Endor.MQ.Model
{
  public class MQReceptionEventArgs
  {
    public bool Ack { get; set; }
    public string RoutingKLey { get; set; }
    public bool Redelivered { get; set; }
  }
}