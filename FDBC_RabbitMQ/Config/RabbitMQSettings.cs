using System;
using System.Collections.Generic;
using System.Text;

namespace FDBC_RabbitMQ.Config
{
  public class RabbitMQSettings
  {
    public string queue_prefix { get; set; }
    public string queue_postfix { get; set; }
  }
}
