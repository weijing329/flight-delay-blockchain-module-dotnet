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
using FDBC_Nethereum.Blockchain;
using Nethereum.Signer;

namespace FDBC_Nethereum.SmartContracts
{
  public class Flight
  {
    private readonly ILogger _logger;

    private readonly BlockchainManager _blockchain_manager;

    private readonly BlockchainSettings _settings;

    private readonly Web3Geth _web3geth;

    private Web3Geth web3geth => _web3geth;

    public Flight(BlockchainManager blockchain_manager)
    //public Flight(Web3Geth web3geth, BlockchainSettings settings)
    {
      _blockchain_manager = blockchain_manager;
      _web3geth = blockchain_manager.Web3Geth;
      _settings = blockchain_manager.Settings;
      _logger = blockchain_manager.Logger;
    }

    public async Task<string> CreateAsync(
      string task_uuid,
      string flight_id, string pid, string ufid,
      string flight_code, string fs_flight_code,
      string departure_utc_offset_hours, string arrival_utc_offset_hours,
      string departure_airport, string arrival_airport,
      string scheduled_departure_date,
      string scheduled_departure_date_time, string scheduled_departure_date_time_local,
      string scheduled_arrival_date_time, string scheduled_arrival_date_time_local,
      BigInteger? nonce = null)
    {
      _logger.LogDebug("FDBC_Nethereum.SmartContracts.Flight.CreateAsync({task_uuid})", task_uuid);

      string sender_address = _settings.default_sender_address;
      string contract_abi = _settings.flight_contract_abi;
      string contract_bytecode = _settings.flight_contract_bytecode;

      //====================================
      // deploy contract
      var from = sender_address;
      var gasLimit = new HexBigInteger(_settings.flight_contract_deploy_gas);
      var wei = new HexBigInteger(0);
      object[] values = new object[] {
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
          };


      string tx_hash = "";

      if (nonce != null)
      {
        string data = web3geth.Eth.DeployContract.GetData(contract_bytecode, contract_abi, values);
        Nethereum.Signer.Transaction signable_transcation = new Nethereum.Signer.Transaction(
          to: null, amount: wei, nonce: (BigInteger)nonce,
          gasPrice: Nethereum.Signer.Transaction.DEFAULT_GAS_PRICE,
          gasLimit: gasLimit.Value,
          data: data
          );

        tx_hash = await _blockchain_manager.SignAndSendRawTransaction(signable_transcation);

        //_logger.LogDebug("SignAndSendRawTransaction(PrivateKey = {sender_private_key}, Signature = {@Signature}) => tx=hash = {tx_hash}", _blockchain_manager.sender_private_key, signable_transcation.Signature, tx_hash);
      }
      else
      {
        tx_hash = await web3geth.Eth.DeployContract.SendRequestAsync(
          abi: contract_abi,
          contractByteCode: contract_bytecode,
          from: from,
          gas: gasLimit,
          value: wei,
          values: values);
      }

      return tx_hash;
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
      string delay_notification_date_time,
      BigInteger? nonce = null
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

      var from = sender_address;
      var gasLimit = new HexBigInteger(_settings.flight_contract_set_all_gas);
      var wei = new HexBigInteger(0);
      object[] values = new object[] {
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
        };

      string tx_hash = "";

      if (nonce != null)
      {
        string data = set_function.GetData(values);

        Nethereum.Signer.Transaction signable_transcation = new Nethereum.Signer.Transaction(
          to: contract_address, amount: wei, nonce: (BigInteger)nonce,
          gasPrice: Nethereum.Signer.Transaction.DEFAULT_GAS_PRICE,
          gasLimit: gasLimit.Value,
          data: data
          );

        tx_hash = await _blockchain_manager.SignAndSendRawTransaction(signable_transcation);
      }
      else
      {
        tx_hash = await set_function.SendTransactionAsync(
            from: from, gas: gasLimit, value: wei,
            functionInput: values
          );
      }

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
