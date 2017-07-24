using System;
using System.Collections.Generic;
using System.Text;
using EasyNetQ;
using Microsoft.Extensions.Configuration;
using FDBC_Shared.DTO;
using System.Threading.Tasks;
using FDBC_RabbitMQ.Config;
using EasyNetQ.Topology;
//using FDBC_RabbitMQ.MessageHandlers;
using FDBC_RabbitMQ.ErrorStrategies;
using EasyNetQ.Consumer;
using FDBC_Nethereum.Services;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Extensions.Logging;

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

    //private readonly ILogger<EasyNetQService> _logger;

    public void Dispose()
    {
      _client.Dispose();

      _web3geth_service.Dispose();
    }

    //public EasyNetQService(IConfiguration configuration, IWeb3GethService web3geth_service, ILogger<EasyNetQService> logger)
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

      //_logger = logger;

      //_logger.LogInformation("Initialized: EasyNetQService");

      // TEST Flight.Create
      foreach (var i in Enumerable.Range(0, 3))
      {
        var request = new I2B_Request
        {
          task_uuid = $"createNewBlockchainFlight_test{i}",
          task = new I2B_Request_Task()
          {
            name = "createNewBlockchainFlight",
            payload = "{\"pid\":28,\"ufid\":72,\"flight_code\":\"CX564\",\"fs_flight_code\":\"CX564\",\"departure_airport\":\"HKG\",\"arrival_airport\":\"TPE\",\"departure_utc_offset_hours\":8,\"arrival_utc_offset_hours\":8,\"scheduled_departure_date\":20170719,\"scheduled_departure_date_time\":\"2017-07-19T05:10:00Z\",\"scheduled_departure_date_time_local\":\"2017-07-19T13:10:00+08:00\",\"scheduled_arrival_date_time\":\"2017-07-19T07:10:00Z\",\"scheduled_arrival_date_time_local\":\"2017-07-19T15:10:00+08:00\",\"hash\":\"0xad037ad2f98401ea9b02b8fa4373e444858836e1acbddf2cea73c126dca40083\"}"
          }
        };

        SendI2B_Request(request).Wait();
      }

      //// TEST Flight.SetFlightAttribute
      //foreach (var i in Enumerable.Range(0, 1))
      //{
      //  var request = new I2B_Request
      //  {
      //    task_uuid = $"deleteBlockchainFlight_test",
      //    task = new I2B_Request_Task()
      //    {
      //      name = "deleteBlockchainFlight",
      //      payload = "{\"flight_id\":80,\"pid\":29,\"ufid\":79,\"flight_code\":\"CI172\",\"fs_flight_code\":\"CI172\",\"departure_utc_offset_hours\":8,\"arrival_utc_offset_hours\":9,\"departure_airport\":\"TPE\",\"arrival_airport\":\"KIX\",\"status\":null,\"scheduled_departure_date\":\"20170720\",\"scheduled_departure_date_time\":\"2017-07-20T06:20:00.000Z\",\"scheduled_departure_date_time_local\":\"2017-07-20T14:20:00+08:00\",\"actual_departure_date_time\":null,\"actual_departure_date_time_local\":null,\"scheduled_arrival_date_time\":\"2017-07-20T09:05:00.000Z\",\"scheduled_arrival_date_time_local\":\"2017-07-20T18:05:00+09:00\",\"actual_arrival_date_time\":null,\"actual_arrival_date_time_local\":null,\"cancel_date_time\":null,\"cancel_date_time_local\":null,\"hash\":\"0xde52a4b7fe8b7d69c68c37ce60639b989e928a457dace6dda065569b4138060b\",\"contract_address\":\"0x4f82d90edeecf2abc5880e8c41aa288312ce2981\",\"flight_status_source\":null,\"flight_status_fed\":false,\"flight_status_confirmed_txhash\":null,\"delay_notification_date_time\":null,\"deleted\":false,\"creation_txhash\":null,\"created_at\":\"2017-07-20T05:37:02.000Z\",\"version\":0}"
      //    }
      //  };

      //  DeleteFlight delete_flight = JsonConvert.DeserializeObject<DeleteFlight>(request.task.payload);

      //  Tuple<string, string> result = _web3geth_service.Flight.SetFlightAllAttributes(
      //    contract_address: delete_flight.contract_address,
      //    task_uuid: request.task_uuid,
      //    status: delete_flight.status,
      //    actual_departure_date_time: delete_flight.actual_departure_date_time,
      //    actual_departure_date_time_local: delete_flight.actual_departure_date_time_local,
      //    actual_arrival_date_time: delete_flight.actual_arrival_date_time,
      //    actual_arrival_date_time_local: delete_flight.actual_arrival_date_time_local,
      //    cancel_date_time: delete_flight.cancel_date_time,
      //    cancel_date_time_local: delete_flight.cancel_date_time_local,
      //    deleted: delete_flight.deleted.ToString(),
      //    flight_status_source: delete_flight.flight_status_source,
      //    flight_status_fed: delete_flight.flight_status_fed.ToString(),
      //    delay_notification_date_time: delete_flight.delay_notification_date_time
      //    ).Result;

      //// TEST Policy.Create
      //foreach (var i in Enumerable.Range(0, 10))
      //{
      //  var request = new I2B_Request
      //  {
      //    task_uuid = $"createNewBlockchainPolicy_test{i}",
      //    task = new I2B_Request_Task()
      //    {
      //      name = "createNewBlockchainPolicy",
      //      payload = "{\"status\":\"activated\",\"deleted\":false,\"version\":0,\"pid\":31,\"psn\":\"000033\",\"tenant_id\":1,\"start_date_time\":\"2017-07-20T08:00:00.000Z\",\"end_date_time\":\"2017-07-21T08:00:00.000Z\",\"start_date_time_local\":\"201707200800\",\"end_date_time_local\":\"201707210800\",\"created_at\":\"2017-07-20T05:37:51.601Z\"}"
      //    }
      //  };

      //  SendI2B_Request(request).Wait();
      //}

      ////TODO contract_address "0xdec46ce3bc57e48fa986173e223c176a26336922"
      //// TEST Policy.SetFlightAttribute
      //foreach (var i in Enumerable.Range(0, 1))
      //{
      //  var request = new I2B_Request
      //  {
      //    task_uuid = $"deleteBlockchainFlight_test",
      //    task = new I2B_Request_Task()
      //    {
      //      name = "deleteBlockchainFlight",
      //      payload = "{\"pid\":23,\"psn\":\"000024\",\"tenant_id\":1,\"start_date_time\":\"2017-07-19T08:00:00.000Z\",\"end_date_time\":\"2017-07-20T08:00:00.000Z\",\"start_date_time_local\":\"201707190800\",\"end_date_time_local\":\"201707200800\",\"status\":\"activated\",\"contract_address\":\"0xdec46ce3bc57e48fa986173e223c176a26336922\",\"deleted\":false,\"creation_txhash\":null,\"created_at\":\"2017-07-19T07:12:42.000Z\",\"version\":0}"
      //    }
      //  };

      //  //DeleteBlockchainPolicy(request).Wait();

      //  SendI2B_Request(request).Wait();
      //}

      // 需要 Node.js 那邊配合在 Message Propertoes 加入 type: FDBC_Shared.DTO.I2B_Request:FDBC_Shared
      // 目前測試過 PrefetchCount = 1 可以穩定處理所有message, N大於1都會在最後一批有 noacked N-1 卡住, 要關掉 blockchain module 才會 release
      ushort prefetch_count = 1;
      _client.Advanced.Consume<I2B_Request>(
        _queue_intermediate2blockchain,
        (msg, info) => OnReceiving_I2B_Request(msg, info),
        configure => configure.WithPrefetchCount(prefetch_count)
      );

      //_client.Receive<I2B_Request>(queue: QueueNameFormatting("intermediate2blockchain"), onMessage: message =>
      //  EasyNetQActions.OnReceiving_I2B_Request(message)
      //);
    }

    public async Task OnReceiving_I2B_Request(IMessage<I2B_Request> msg, MessageReceivedInfo info)
    {
      I2B_Request request = msg.Body;

      switch (request.task.name)
      {
        case "createNewBlockchainPolicy":
          await CreateNewBlockchainPolicy(request);
          break;

        case "createNewBlockchainFlight":
          await CreateNewBlockchainFlight(request);
          break;

        case "deleteBlockchainPolicy":
          await DeleteBlockchainPolicy(request);
          break;

        case "deleteBlockchainFlight":
          await DeleteBlockchainFlight(request);
          break;

        case "updateBlockchainPolicy":
          await UpdateBlockchainPolicy(request);
          break;

        case "updateBlockchainFlight":
          await UpdateBlockchainFlight(request);
          break;

        default:
          break;
      }
    }

    private async Task CreateNewBlockchainPolicy(I2B_Request request)
    {
      //_logger.LogInformation("Executing: EasyNetQService.CreateNewBlockchainPolicy()");

      CreatePolicy create_policy = JsonConvert.DeserializeObject<CreatePolicy>(request.task.payload);
      string tx_hash = await _web3geth_service.Policy.Create(
        task_uuid: request.task_uuid,
        pid: create_policy.pid.ToString(),
        psn: create_policy.psn,
        tenant_id: create_policy.tenant_id.ToString(),
        start_date_time: create_policy.start_date_time.ToString(),
        end_date_time: create_policy.end_date_time.ToString(),
        start_date_time_local: create_policy.start_date_time_local,
        end_date_time_local: create_policy.end_date_time_local
        );

      // Fire and forget
      await BackgroundWaitTransactionResult(tx_hash, request);
    }

    private async Task DeleteBlockchainPolicy(I2B_Request request)
    {
      //_logger.LogInformation("Executing: EasyNetQService.DeleteBlockchainPolicy()");

      DeletePolicy delete_policy = JsonConvert.DeserializeObject<DeletePolicy>(request.task.payload);

      string tx_hash = await _web3geth_service.Policy.SetPolicyAllAttributes(
        contract_address: delete_policy.contract_address,
        task_uuid: request.task_uuid,
        start_date_time: delete_policy.start_date_time.ToString(),
        end_date_time: delete_policy.end_date_time.ToString(),
        start_date_time_local: delete_policy.start_date_time_local,
        end_date_time_local: delete_policy.end_date_time_local,
        status: delete_policy.status,
        deleted: delete_policy.deleted.ToString()
        );

      // Fire and forget
      await BackgroundWaitTransactionResult(tx_hash, request);
    }

    private async Task UpdateBlockchainPolicy(I2B_Request request)
    {
      //_logger.LogInformation("Executing: EasyNetQService.UpdateBlockchainPolicy()");

      UpdatePolicy update_policy = JsonConvert.DeserializeObject<UpdatePolicy>(request.task.payload);

      string tx_hash = await _web3geth_service.Policy.SetPolicyAllAttributes(
        contract_address: update_policy.contract_address,
        task_uuid: request.task_uuid,
        start_date_time: update_policy.start_date_time.ToString(),
        end_date_time: update_policy.end_date_time.ToString(),
        start_date_time_local: update_policy.start_date_time_local,
        end_date_time_local: update_policy.end_date_time_local,
        status: update_policy.status,
        deleted: update_policy.deleted.ToString()
        );

      // Fire and forget
      await BackgroundWaitTransactionResult(tx_hash, request);
    }

    private async Task CreateNewBlockchainFlight(I2B_Request request)
    {
      //_logger.LogInformation("Executing: EasyNetQService.CreateNewBlockchainFlight()");

      CreateFlight create_flight = JsonConvert.DeserializeObject<CreateFlight>(request.task.payload);
      string tx_hash = await _web3geth_service.Flight.Create(
        task_uuid: request.task_uuid,
        flight_id: create_flight.ufid.ToString(),
        pid: create_flight.pid.ToString(),
        ufid: create_flight.ufid.ToString(),
        flight_code: create_flight.flight_code,
        fs_flight_code: create_flight.fs_flight_code,
        departure_utc_offset_hours: create_flight.departure_utc_offset_hours.ToString(),
        arrival_utc_offset_hours: create_flight.arrival_utc_offset_hours.ToString(),
        departure_airport: create_flight.departure_airport,
        arrival_airport: create_flight.arrival_airport,
        scheduled_departure_date: create_flight.scheduled_departure_date.ToString(),
        scheduled_departure_date_time: create_flight.scheduled_departure_date_time.ToString(),
        scheduled_departure_date_time_local: create_flight.scheduled_departure_date_time_local.ToString(),
        scheduled_arrival_date_time: create_flight.scheduled_arrival_date_time.ToString(),
        scheduled_arrival_date_time_local: create_flight.scheduled_departure_date_time_local.ToString()
        );

      // Fire and forget
      await BackgroundWaitTransactionResult(tx_hash, request);
    }

    private async Task UpdateBlockchainFlight(I2B_Request request)
    {
      //_logger.LogInformation("Executing: EasyNetQService.UpdateBlockchainFlight()");

      UpdateFlight update_flight = JsonConvert.DeserializeObject<UpdateFlight>(request.task.payload);

      string tx_hash = await _web3geth_service.Flight.SetFlightAllAttributes(
        contract_address: update_flight.contract_address,
        task_uuid: request.task_uuid,
        status: update_flight.status,
        actual_departure_date_time: update_flight.actual_departure_date_time,
        actual_departure_date_time_local: update_flight.actual_departure_date_time_local,
        actual_arrival_date_time: update_flight.actual_arrival_date_time,
        actual_arrival_date_time_local: update_flight.actual_arrival_date_time_local,
        cancel_date_time: update_flight.cancel_date_time,
        cancel_date_time_local: update_flight.cancel_date_time_local,
        deleted: update_flight.deleted.ToString(),
        flight_status_source: update_flight.flight_status_source,
        flight_status_fed: update_flight.flight_status_fed.ToString(),
        delay_notification_date_time: update_flight.delay_notification_date_time
        );

      // Fire and forget
      await BackgroundWaitTransactionResult(tx_hash, request);
    }

    private async Task DeleteBlockchainFlight(I2B_Request request)
    {
      //_logger.LogInformation("Executing: EasyNetQService.DeleteBlockchainFlight()");

      DeleteFlight delete_flight = JsonConvert.DeserializeObject<DeleteFlight>(request.task.payload);

      string tx_hash = await _web3geth_service.Flight.SetFlightAllAttributes(
        contract_address: delete_flight.contract_address,
        task_uuid: request.task_uuid,
        status: delete_flight.status,
        actual_departure_date_time: delete_flight.actual_departure_date_time,
        actual_departure_date_time_local: delete_flight.actual_departure_date_time_local,
        actual_arrival_date_time: delete_flight.actual_arrival_date_time,
        actual_arrival_date_time_local: delete_flight.actual_arrival_date_time_local,
        cancel_date_time: delete_flight.cancel_date_time,
        cancel_date_time_local: delete_flight.cancel_date_time_local,
        deleted: delete_flight.deleted.ToString(),
        flight_status_source: delete_flight.flight_status_source,
        flight_status_fed: delete_flight.flight_status_fed.ToString(),
        delay_notification_date_time: delete_flight.delay_notification_date_time
        );

      // Fire and forget
      await BackgroundWaitTransactionResult(tx_hash, request);
    }


    private async Task BackgroundWaitTransactionResult(string tx_hash, I2B_Request request)
    {
      Tuple<string, string> result = new Tuple<string, string>("", "");

      switch (request.task.name)
      {
        case "createNewBlockchainPolicy":
          result = await _web3geth_service.Policy.GetTransactionResult_Create(tx_hash);
          break;

        case "createNewBlockchainFlight":
          result = await _web3geth_service.Flight.GetTransactionResult_Create(tx_hash);
          break;

        case "deleteBlockchainPolicy":
          result = await _web3geth_service.Policy.GetTransactionResult_SetPolicyAllAttributes(tx_hash);
          break;

        case "deleteBlockchainFlight":
          result = await _web3geth_service.Flight.GetTransactionResult_SetFlightAllAttributes(tx_hash);
          break;

        case "updateBlockchainPolicy":
          result = await _web3geth_service.Policy.GetTransactionResult_SetPolicyAllAttributes(tx_hash);
          break;

        case "updateBlockchainFlight":
          result = await _web3geth_service.Flight.GetTransactionResult_SetFlightAllAttributes(tx_hash);
          break;

        default:
          break;
      }

      string transaction_receipt = result.Item1;
      string event_log = result.Item2;

      await SendB2I_Response(request.task_uuid, request.task.payload, transaction_receipt, event_log);
    }

    public async Task SendI2B_Request(I2B_Request request)
    {
      //_logger.LogInformation("Executing: EasyNetQService.SendI2B_Request()");

      var msg = new Message<I2B_Request>(request);
      await _client.Advanced.PublishAsync(Exchange.GetDefault(), QueueNameFormatting("intermediate2blockchain"), false, msg);
    }

    //public async Task SendI2B_Response(I2B_Response response)
    //{
    //  //await _client.SendAsync(queue: QueueNameFormatting("intermediate2blockchain"), message: response);
    //}

    //public async Task SendB2I_Request(B2I_Request request)
    //{
    //  //await _client.SendAsync(queue: QueueNameFormatting("blockchain2intermdiate"), message: request);

    //  var msg = new Message<B2I_Request>(request);
    //  await _client.Advanced.PublishAsync(Exchange.GetDefault(), QueueNameFormatting("blockchain2intermediate"), false, msg);

    //}

    public async Task SendB2I_Response(string task_uuid, string payload, string transaction_receipt, string event_log)
    {
      //_logger.LogInformation("Executing: EasyNetQService.SendB2I_Response()");

      //await _client.SendAsync(queue: QueueNameFormatting("blockchain2intermdiate"), message: response);
      var response = new B2I_Response()
      {
        task_uuid = task_uuid,
        task = new B2I_Response_Task
        {
          name = "getBlockhainEventResponse",
          payload = payload,
          transaction_receipt = transaction_receipt,
          event_log = event_log
        }
      };

      var msg = new Message<B2I_Response>(response);
      await _client.Advanced.PublishAsync(Exchange.GetDefault(), QueueNameFormatting("blockchain2intermediate"), false, msg);

    }

    private string QueueNameFormatting(string queue_name)
    {
      return $"{_settings.queue_prefix}{queue_name}{_settings.queue_postfix}";
    }
  }
}
