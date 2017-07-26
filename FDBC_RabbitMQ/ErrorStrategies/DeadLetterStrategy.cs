﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using EasyNetQ;
using EasyNetQ.Consumer;

namespace FDBC_RabbitMQ.ErrorStrategies
{
  public class DeadLetterStrategy : DefaultConsumerErrorStrategy
  {
    public DeadLetterStrategy(IConnectionFactory connectionFactory, ISerializer serializer, IEasyNetQLogger logger, IConventions conventions, ITypeNameSerializer typeNameSerializer, IErrorMessageSerializer errorMessageSerializer)
    : base(connectionFactory, serializer, logger, conventions, typeNameSerializer, errorMessageSerializer)
    {
    }

    public override AckStrategy HandleConsumerError(ConsumerExecutionContext context, Exception exception)
    {
      object deathHeaderObject;
      if (!context.Properties.Headers.TryGetValue("x-death", out deathHeaderObject))
        return AckStrategies.NackWithoutRequeue;

      var deathHeaders = deathHeaderObject as IList;

      if (deathHeaders == null)
        return AckStrategies.NackWithoutRequeue;

      var retries = 0;
      foreach (IDictionary header in deathHeaders)
      {
        var count = int.Parse(header["count"].ToString());
        retries += count;
      }

      if (retries < 3)
        return AckStrategies.NackWithoutRequeue;

      return base.HandleConsumerError(context, exception);
    }
  }
}
