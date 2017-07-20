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
//using FDBC_RabbitMQ.MessageHandlers;
using FDBC_RabbitMQ.ErrorStrategies;
using EasyNetQ.Consumer;
using FDBC_Nethereum.Services;
using Newtonsoft.Json;
using System.Linq;

namespace FDBC_RabbitMQ.MqServices
{
  public class EasyNetQService : IDisposable
  {
    private IBus _client;
    //private IAdvancedBus _advanced_client;
    private readonly RabbitMQSettings _settings;

    private IQueue _queue_intermediate2blockchain;
    private IQueue _queue_blockchain2intermdiate;

    private IWeb3GethService _web3geth_service;

    public void Dispose()
    {
      _client.Dispose();

      _web3geth_service.Dispose();
    }

    public EasyNetQService(IConfiguration configuration, IWeb3GethService web3geth_service)
    {
      _settings = configuration.GetSection("RabbitMQSettings").Get<RabbitMQSettings>();

      _client = RabbitHutch.CreateBus(configuration.GetConnectionString("RabbitMQConnectionString"));

      //_client = RabbitHutch.CreateBus(configuration.GetConnectionString("RabbitMQConnectionString"), 
      //  serviceRegister => serviceRegister.Register<IConsumerErrorStrategy, DeadLetterStrategy>());

      //_client.Receive<string>(queue: QueueNameFormatting("intermediate2blockchain"), onMessage: message => TestString(message));

      //_advanced_client = RabbitHutch.CreateBus(configuration.GetConnectionString("RabbitMQConnectionString")).Advanced;

      _queue_intermediate2blockchain = _client.Advanced.QueueDeclare(name: QueueNameFormatting("intermediate2blockchain"));

      _queue_blockchain2intermdiate = _client.Advanced.QueueDeclare(name: QueueNameFormatting("blockchain2intermediate"));

      _web3geth_service = web3geth_service;

      foreach (var i in Enumerable.Range(0, 10))
      {
        // test Flight.Create
        var request = new I2B_Request
        {
          task_uuid = $"test{i}",
          task = new I2B_Request_Task()
          {
            name = "createNewBlockchainFlight",
            payload = "{\"pid\":28,\"ufid\":72,\"flight_code\":\"CX564\",\"fs_flight_code\":\"CX564\",\"departure_airport\":\"HKG\",\"arrival_airport\":\"TPE\",\"departure_utc_offset_hours\":8,\"arrival_utc_offset_hours\":8,\"scheduled_departure_date\":20170719,\"scheduled_departure_date_time\":\"2017-07-19T05:10:00Z\",\"scheduled_departure_date_time_local\":\"2017-07-19T13:10:00+08:00\",\"scheduled_arrival_date_time\":\"2017-07-19T07:10:00Z\",\"scheduled_arrival_date_time_local\":\"2017-07-19T15:10:00+08:00\",\"hash\":\"0xad037ad2f98401ea9b02b8fa4373e444858836e1acbddf2cea73c126dca40083\"}"
          }
        };

        SendI2B_Request(request).Wait();
      }

      //CreateFlight flight = JsonConvert.DeserializeObject<CreateFlight>(request.task.payload);
      //string response_payload = _web3geth_service.Flight.Create(
      //  task_uuid: request.task_uuid,
      //  flight_id: flight.ufid.ToString(),
      //  pid: flight.pid.ToString(),
      //  ufid: flight.ufid.ToString(),
      //  flight_code: flight.flight_code,
      //  fs_flight_code: flight.fs_flight_code,
      //  departure_utc_offset_hours: flight.departure_utc_offset_hours.ToString(),
      //  arrival_utc_offset_hours: flight.arrival_utc_offset_hours.ToString(),
      //  departure_airport: flight.departure_airport,
      //  arrival_airport: flight.arrival_airport,
      //  scheduled_departure_date: flight.scheduled_departure_date.ToString(),
      //  scheduled_departure_date_time: flight.scheduled_departure_date_time.ToString(),
      //  scheduled_departure_date_time_local: flight.scheduled_departure_date_time_local.ToString(),
      //  scheduled_arrival_date_time: flight.scheduled_arrival_date_time.ToString(),
      //  scheduled_arrival_date_time_local: flight.scheduled_departure_date_time_local.ToString()
      //  ).Result;

      //var response = new B2I_Response()
      //{
      //  task_uuid = request.task_uuid,
      //  task = new B2I_Response_Task
      //  {
      //    name = request.task.name,
      //    payload = response_payload
      //  }
      //};
      //SendB2I_Response(response).Wait();

      //var request = new B2I_Response
      //{
      //  task_uuid = "1234",
      //  task = new B2I_Response_Task()
      //  {
      //    name = "createNewBlockchainFlight",
      //    payload = "test"
      //  }
      //};
      //SendB2I_Response(request).Wait();

      //_client.Advanced.Consume(_queue_intermediate2blockchain, (body, properties, info) => Task.Factory.StartNew(() =>
      //{
      //  var message = Encoding.UTF8.GetString(body);
      //  Console.WriteLine("Got message: '{0}'", message);
      //}));

      // 需要 Node.js 那邊配合在 Message Propertoes 加入 type: FDBC_Shared.DTO.I2B_Request:FDBC_Shared
      _client.Advanced.Consume<I2B_Request>(
        _queue_intermediate2blockchain, 
        (msg, info) => OnReceiving_I2B_Request(msg, info),
        configure => configure.WithPrefetchCount(1)
      );

      //var request = new B2I_Request
      //{
      //  text = "test"
      //};
      //SendB2I_Request(request).Wait();

      //_client.Receive<I2B_Request>(queue: QueueNameFormatting("intermediate2blockchain"), onMessage: message =>
      //  EasyNetQActions.OnReceiving_I2B_Request(message)
      //);
    }

    public async Task OnReceiving_I2B_Request(IMessage<I2B_Request> msg, MessageReceivedInfo info)
    {
      I2B_Request request = msg.Body;

      try
      {
        //await _web3geth_service.Flight.SetStatus(task_uuid: "test_uuid", input: "X");
        //var eveng_log = _web3geth_service.TestingContract.TestSha3().Result;

        //string response_payload = _web3geth_service.Flight.Create(
        //    task_uuid: "test",
        //    flight_id: "test",
        //    pid: "test",
        //    ufid: "test",
        //    flight_code: "test",
        //    fs_flight_code: "test",
        //    departure_utc_offset_hours: "test",
        //    arrival_utc_offset_hours: "test",
        //    departure_airport: "test",
        //    arrival_airport: "test",
        //    scheduled_departure_date: "test",
        //    scheduled_departure_date_time: "test",
        //    scheduled_departure_date_time_local: "test",
        //    scheduled_arrival_date_time: "test",
        //    scheduled_arrival_date_time_local: "test"
        //    ).Result;


        //var response_task = new B2I_Response_Task
        //{
        //  name = request.task.name,
        //  payload = response_payload
        //};
        //var response = new B2I_Response()
        //{
        //  task_uuid = request.task_uuid,
        //  task = response_task
        //};
        //SendB2I_Response(response).Wait();
      }
      catch (ArgumentException argumentException)
      {

        throw;
      }

      switch (request.task.name)
      {
        case "createNewBlockchainPolicy":
          CreatePolicy create_policy = JsonConvert.DeserializeObject<CreatePolicy>(request.task.payload);
          break;

        case "createNewBlockchainFlight":
          CreateFlight flight = JsonConvert.DeserializeObject<CreateFlight>(request.task.payload);
          string response_payload = await _web3geth_service.Flight.Create(
            task_uuid: request.task_uuid,
            flight_id: flight.ufid.ToString(),
            pid: flight.pid.ToString(),
            ufid: flight.ufid.ToString(),
            flight_code: flight.flight_code,
            fs_flight_code: flight.fs_flight_code,
            departure_utc_offset_hours: flight.departure_utc_offset_hours.ToString(),
            arrival_utc_offset_hours: flight.arrival_utc_offset_hours.ToString(),
            departure_airport: flight.departure_airport,
            arrival_airport: flight.arrival_airport,
            scheduled_departure_date: flight.scheduled_departure_date.ToString(),
            scheduled_departure_date_time: flight.scheduled_departure_date_time.ToString(),
            scheduled_departure_date_time_local: flight.scheduled_departure_date_time_local.ToString(),
            scheduled_arrival_date_time: flight.scheduled_arrival_date_time.ToString(),
            scheduled_arrival_date_time_local: flight.scheduled_departure_date_time_local.ToString()
            );

          var response = new B2I_Response()
          {
            task_uuid = request.task_uuid,
            task = new B2I_Response_Task
            {
              name = request.task.name,
              payload = response_payload
            }
          };
          await SendB2I_Response(response);
          break;

        case "deleteBlockchainFlight":
          DeleteFlight delete_flight = JsonConvert.DeserializeObject<DeleteFlight>(request.task.payload);
          break;

        case "updateBlockchainPolicy":
          UpdatePolicy update_policy = JsonConvert.DeserializeObject<UpdatePolicy>(request.task.payload);
          break;

        default:
          break;
      }
    }

    private void TestString(string message)
    {
      Console.WriteLine(message);
    }

    public async Task SendI2B_Request(I2B_Request request)
    {
      var msg = new Message<I2B_Request>(request);
      _client.Advanced.Publish(Exchange.GetDefault(), QueueNameFormatting("intermediate2blockchain"), false, msg);
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
      var msg = new Message<B2I_Response>(response);
      _client.Advanced.Publish(Exchange.GetDefault(), QueueNameFormatting("blockchain2intermediate"), false, msg);

    }

    private string QueueNameFormatting(string queue_name)
    {
      return $"{_settings.queue_prefix}{queue_name}{_settings.queue_postfix}";
    }
  }
}
