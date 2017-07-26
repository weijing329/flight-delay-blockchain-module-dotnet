using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

using Nethereum.Geth;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Web3.Accounts.Managed;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Filters;
using Nethereum.ABI.FunctionEncoding.Attributes;

using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.Model;
using Nethereum.JsonRpc.Client;

using Nethereum.ABI.Encoders;

using Newtonsoft.Json;
using FDBC_Nethereum.Config;
using FDBC_Nethereum.Services;

namespace FDBC_Nethereum.SmartContracts
{
  public class Flight
  {
    private readonly ILogger<Web3GethService> _logger;

    private readonly BlockchainSettings _settings;

    private readonly Web3Geth _web3geth;

    private Web3Geth web3geth => _web3geth;

    public Flight(Web3Geth web3geth, BlockchainSettings settings, ILogger<Web3GethService> logger)
    //public Flight(Web3Geth web3geth, BlockchainSettings settings)
    {
      _web3geth = web3geth;
      _settings = settings;
      _logger = logger;
    }

    public async Task<string> CreateAsync(
      string task_uuid,
      string flight_id, string pid, string ufid,
      string flight_code, string fs_flight_code,
      string departure_utc_offset_hours, string arrival_utc_offset_hours,
      string departure_airport, string arrival_airport,
      string scheduled_departure_date,
      string scheduled_departure_date_time, string scheduled_departure_date_time_local,
      string scheduled_arrival_date_time, string scheduled_arrival_date_time_local)
    {
      _logger.LogDebug("FDBC_Nethereum.SmartContracts.Flight.CreateAsync({task_uuid})", task_uuid);

      string sender_address = _settings.default_sender_address;
      string contract_abi = _settings.flight_contract_abi;
      string contract_bytecode = _settings.flight_contract_bytecode;

      //====================================
      // deploy contract
      try
      {
        var gas = new HexBigInteger(_settings.flight_contract_deploy_gas);
        var wei = new HexBigInteger(0);
        string tx_hash = await web3geth.Eth.DeployContract.SendRequestAsync(
          abi: contract_abi,
          contractByteCode: contract_bytecode,
          from: sender_address,
          gas: gas,
          value: wei,
          values: new object[] {
          task_uuid,
          flight_id,
          pid,
          ufid,
          flight_code,
          fs_flight_code,
          departure_utc_offset_hours ,
          arrival_utc_offset_hours,
          departure_airport,
          arrival_airport,
          scheduled_departure_date,
          scheduled_departure_date_time,
          scheduled_departure_date_time_local,
          scheduled_arrival_date_time,
          scheduled_arrival_date_time_local
          });

        return tx_hash;
      }
      catch (Exception ex)
      {
        _logger.LogError("Exception {@ex}", ex);
        throw ex;
      }
    }

    public async Task<Tuple<string, string>> GetTransactionResultAsync_Create(string tx_hash)
    {
      TransactionReceipt receipt = await GetTransactionReceiptAsync(tx_hash);

      string stringified_receipt = JsonConvert.SerializeObject(receipt);
      string stringified_event_log = "";

      return new Tuple<string, string>(stringified_receipt, stringified_event_log);
    }

    public async Task<string> SetFlightAllAttributes(
      string contract_address,
      string task_uuid,
      string status,
      string actual_departure_date_time,
      string actual_departure_date_time_local,
      string actual_arrival_date_time,
      string actual_arrival_date_time_local,
      string cancel_date_time,
      string cancel_date_time_local,
      string deleted,
      string flight_status_source,
      string flight_status_fed,
      string delay_notification_date_time
      )
    {
      // SmartContract function doesn't take null as input for string
      status = status ?? "";
      actual_departure_date_time = actual_departure_date_time ?? "";
      actual_departure_date_time_local = actual_departure_date_time_local ?? "";
      actual_arrival_date_time = actual_arrival_date_time ?? "";
      actual_arrival_date_time_local = actual_arrival_date_time_local ?? "";
      cancel_date_time = cancel_date_time ?? "";
      cancel_date_time_local = cancel_date_time_local ?? "";
      deleted = deleted ?? "";
      flight_status_source = flight_status_source ?? "";
      flight_status_fed = flight_status_fed ?? "";
      delay_notification_date_time = delay_notification_date_time ?? "";

      // Web3
      string sender_address = _settings.default_sender_address;
      string contract_abi = _settings.flight_contract_abi;

      Contract contract = web3geth.Eth.GetContract(contract_abi, contract_address);

      string task_uuid_sha3 = $"0x{_web3geth.Sha3(task_uuid)}";
      byte[] task_uuid_sha3_bytes32 = task_uuid_sha3.HexToByteArray();

      Function set_function = contract.GetFunction("set_all");

      var gas = new HexBigInteger(_settings.flight_contract_set_all_gas);
      var wei = new HexBigInteger(0);
      var tx_hash = await set_function.SendTransactionAsync(
        from: sender_address, gas: gas, value: wei,
        functionInput: new object[] {
          task_uuid,
          status,
          actual_departure_date_time,
          actual_departure_date_time_local,
          actual_arrival_date_time,
          actual_arrival_date_time_local,
          cancel_date_time,
          cancel_date_time_local,
          deleted,
          flight_status_source,
          flight_status_fed,
          delay_notification_date_time
        });

      return tx_hash;
    }

    public async Task<Tuple<string, string>> GetTransactionResult_SetFlightAllAttributes(string tx_hash)
    {
      TransactionReceipt receipt = await GetTransactionReceiptAsync(tx_hash);

      string stringified_receipt = JsonConvert.SerializeObject(receipt);
      string stringified_event_log = "";

      return new Tuple<string, string>(stringified_receipt, stringified_event_log);
    }

    private async Task<TransactionReceipt> GetTransactionReceiptAsync(string tx_hash)
    {
      //_logger.LogDebug("Executing: Web3GethService.SmartContracts.Flight.GetTransactionReceiptAsync({tx_hash})", tx_hash);

      int web3_transaction_check_delay_in_ms = _settings.default_retry_in_ms;

      TransactionReceipt receipt = null;
      int request_count = 0;
      while ((receipt == null) && (request_count < _settings.max_retry_times))
      {
        await Task.Delay(web3_transaction_check_delay_in_ms);
        receipt = await web3geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(tx_hash);
        request_count += 1;

        _logger.LogDebug("GetTransactionReceipt({tx_hash}, request_count = {request_count}) = {@receipt}", tx_hash, request_count, receipt);
      }

      return receipt;
    }

    //public async Task<Tuple<string, string>> SetFlightAttribute(string contract_address, string task_uuid, string attribute_name, string attribute_value)
    //{
    //  Web3Geth web3geth = _web3geth;
    //  string sender_address = _default_sender_address;
    //  string contract_abi = _contract_abi;

    //  Contract contract = web3geth.Eth.GetContract(contract_abi, contract_address);

    //  string task_uuid_sha3 = $"0x{_web3geth.Sha3(task_uuid)}";
    //  byte[] task_uuid_sha3_bytes32 = task_uuid_sha3.HexToByteArray();

    //  var set_event = contract.GetEvent($"event_set_{attribute_name}");
    //  var set_event_filter_by_task_uuid = await set_event.CreateFilterAsync(new[] { task_uuid_sha3_bytes32 });

    //  Function set_function = contract.GetFunction($"set_{attribute_name}");

    //  var wei = new HexBigInteger(0);
    //  var tx_hash = await set_function.SendTransactionAsync(from: sender_address, gas: new HexBigInteger(4700000), value: wei, functionInput: new object[] { task_uuid, attribute_value });

    //  int web3_transaction_check_delay_in_ms = _default_retry_in_ms;

    //  TransactionReceipt receipt = null;
    //  while (receipt == null)
    //  {
    //    await Task.Delay(web3_transaction_check_delay_in_ms);
    //    receipt = await web3geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(tx_hash);
    //  }

    //  var set_event_logs = await set_event.GetFilterChanges<EventSetFlightAttribute>(set_event_filter_by_task_uuid);

    //  await web3geth.Eth.Filters.UninstallFilter.SendRequestAsync(set_event_filter_by_task_uuid);

    //  string stringified_receipt = JsonConvert.SerializeObject(receipt);
    //  string stringified_event_log = JsonConvert.SerializeObject(set_event_logs.FirstOrDefault());

    //  return new Tuple<string, string>(stringified_receipt, stringified_event_log);
    //}

    //public async Task SetStatus(string task_uuid, string input)
    //{
    //  Web3Geth web3geth = _web3geth;
    //  string sender_address = _default_sender_address;
    //  string contract_address = _test_existing_contract_address;
    //  string contract_abi = _contract_abi;
    //  string contract_bytecode = _contract_bytecode;

    //  // connection problem? Windows firewall TCP 8545
    //  // TODO 測試 deploy to local blockchain => test local set function and event

    //  Contract contract = web3geth.Eth.GetContract(contract_abi, contract_address);

    //  string task_uuid_sha3 = $"0x{_web3geth.Sha3(task_uuid)}";
    //  //Bytes32TypeEncoder encoder = new Bytes32TypeEncoder();
    //  //byte[] task_uuid_sha3_bytes32 = encoder.Encode(task_uuid_sha3);
    //  byte[] task_uuid_sha3_bytes32 = task_uuid_sha3.HexToByteArray();

    //  var set_status_event = contract.GetEvent("event_set_status");
    //  var set_status_event_filter = await set_status_event.CreateFilterAsync(task_uuid_sha3_bytes32);

    //  Function set_status_function = contract.GetFunction("set_status");

    //  var wei = new HexBigInteger(0);
    //  var tx_hash = await set_status_function.SendTransactionAsync(from: sender_address, gas: new HexBigInteger(4700000), value: wei, functionInput: new object[] { task_uuid, input });

    //  //bool bool_result = await web3geth.Miner.Start.SendRequestAsync(120);

    //  int web3_transaction_check_delay_in_ms = _default_retry_in_ms;

    //  TransactionReceipt receipt = null;
    //  while (receipt == null)
    //  {
    //    await Task.Delay(web3_transaction_check_delay_in_ms);
    //    receipt = await web3geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(tx_hash);
    //  }

    //  //bool_result = await web3geth.Miner.Stop.SendRequestAsync();

    //  //var set_status_event_logs = await set_status_event.GetFilterChanges<EventSetFlightAttribute>(set_status_event_filter);

    //  //bool uninstall_successful = await _web3geth.Eth.Filters.UninstallFilter.SendRequestAsync(set_status_event_filter);
    //}

    public class EventNewFlight
    {
      [Parameter("bytes32", "task_uuid", 1, true)]
      public byte[] task_uuid { get; set; }

      [Parameter("address", "flight_contract_address", 2, false)]
      public string flight_contract_address { get; set; }
    }

    public class EventSetFlightAttribute
    {
      [Parameter("bytes32", "task_uuid", 1, true)]
      public byte[] task_uuid { get; set; }

      [Parameter("string", "old_val", 2, false)]
      public string old_val { get; set; }

      [Parameter("string", "new_val", 3, false)]
      public string new_val { get; set; }
    }

  }
}
