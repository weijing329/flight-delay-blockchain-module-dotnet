using System;
using System.Collections.Generic;
using System.Text;

using EasyNetQ;
using EasyNetQ.Consumer;

namespace FDBC_RabbitMQ.ErrorStrategies
{
  public sealed class AlwaysRequeueErrorStrategy : IConsumerErrorStrategy
  {
    public void Dispose()
    {
    }

    public AckStrategy HandleConsumerError(ConsumerExecutionContext context, Exception exception)
    {
      return AckStrategies.NackWithRequeue;
    }

    public AckStrategy HandleConsumerCancelled(ConsumerExecutionContext context)
    {
      return AckStrategies.NackWithRequeue;
    }
  }

}
