using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
//using RawRabbit;
//using RawRabbit.vNext;
//using RawRabbit.Configuration;
//using RawRabbit.Channel;
//using RawRabbit.Context;
//using RawRabbit.Operations;
//using RawRabbit.Consumer;
//using RawRabbit.Common;
//using RawRabbit.Extensions;
//using RawRabbit.Extensions.Client;
using Microsoft.Extensions.Configuration;
using FDBC_Shared.DTO;

namespace FDBC_RabbitMQ.MqServices
{
  public class RawRabbitService : IDisposable
  {
    //private IBusClient _client;

    //private RawRabbit.Extensions.Client.IBusClient _client;

    public void Dispose()
    {
      //_client.ShutdownAsync();
    }

    public RawRabbitService(IConfigurationRoot configuration)
    {
      //_client = BusClientFactory.CreateDefault(configuration.GetSection("RawRabbitConfiguration").Get<RawRabbitConfiguration>());

      ////_client = RawRabbitFactory.Create();

      //_client.SubscribeAsync<I2B_Request>(async (msg, AdvancedMessageContext) =>
      //    {
      //      return;
      //    },
      //    cfg => cfg
      //      .WithQueue(q => q.WithName(""))
      //  );

      //_client.RespondAsync<I2B_Request, I2B_Response>(async (request, context) =>
      //{
      //  return new I2B_Response();
      //}
      //,
      //configuration: cfg => cfg
      //  .WithQueue(q => q.WithName(""))
      //);
    }

    public void SendTransactionResult(B2I_Request msg)
    {
      //_client.PublishAsync<B2I_Request>(message: msg, 
      //  configuration: cfg => cfg.WithProperties(p => p.)

      //_client.RequestAsync<B2I_Request, B2I_Response>(async (request) => 
      //{
      //  return new I2B_Response();
      //},
      //configuration: cfg => cfg
      //  .WithReplyQueue
      //  .WithQueue(q => q.WithName(""))
      //);
    }
  }
}
