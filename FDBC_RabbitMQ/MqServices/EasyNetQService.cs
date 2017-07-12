using System;
using System.Collections.Generic;
using System.Text;
using EasyNetQ;
using Microsoft.Extensions.Configuration;
using FDBC_Shared.DTO;
using System.Threading.Tasks;
using FDBC_RabbitMQ.Actions;
using FDBC_RabbitMQ.Config;
using EasyNetQ.Topology;

namespace FDBC_RabbitMQ.MqServices
{
  public class EasyNetQService : IDisposable
  {
    private IBus _client;
    //private IAdvancedBus _advanced_client;
    private readonly RabbitMQSettings _settings;

    private IQueue _queue_intermediate2blockchain;
    private IQueue _queue_blockchain2intermdiate;

    public void Dispose()
    {
      _client.Dispose();
    }

    public EasyNetQService(IConfigurationRoot configuration)
    {
      _settings = configuration.GetSection("RabbitMQSettings").Get<RabbitMQSettings>();

      _client = RabbitHutch.CreateBus(configuration.GetConnectionString("RabbitMQConnectionString"));

      //_client.Receive<string>(queue: QueueNameFormatting("intermediate2blockchain"), onMessage: message => TestString(message));

      //_advanced_client = RabbitHutch.CreateBus(configuration.GetConnectionString("RabbitMQConnectionString")).Advanced;

      _queue_intermediate2blockchain = _client.Advanced.QueueDeclare(name: QueueNameFormatting("intermediate2blockchain"));

      _queue_blockchain2intermdiate = _client.Advanced.QueueDeclare(name: QueueNameFormatting("blockchain2intermediate"));

      _client.Advanced.Consume(_queue_intermediate2blockchain, (body, properties, info) => Task.Factory.StartNew(() =>
      {
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine("Got message: '{0}'", message);
      }));

      var request = new B2I_Request
      {
        text = "test"
      };
      SendB2I_Request(request).Wait();

      //_client.Receive<I2B_Request>(queue: QueueNameFormatting("intermediate2blockchain"), onMessage: message =>
      //  EasyNetQActions.OnReceiving_I2B_Request(message)
      //);
    }

    private void TestString(string message)
    {
      Console.WriteLine(message);
    }

    public async Task SendI2B_Response(I2B_Response response)
    {
      //await _client.SendAsync(queue: QueueNameFormatting("intermediate2blockchain"), message: response);
    }

    public async Task SendB2I_Request(B2I_Request request)
    {
      //await _client.SendAsync(queue: QueueNameFormatting("blockchain2intermdiate"), message: request);

      var msg = new Message<B2I_Request>(request);
      _client.Advanced.Publish(Exchange.GetDefault(), QueueNameFormatting("blockchain2intermediate"), false, msg);

    }

    public async Task SendB2I_Response(B2I_Response response)
    {
      //await _client.SendAsync(queue: QueueNameFormatting("blockchain2intermdiate"), message: response);
    }

    private string QueueNameFormatting(string queue_name)
    {
      return $"{_settings.queue_prefix}{queue_name}{_settings.queue_postfix}";
    }
  }
}
