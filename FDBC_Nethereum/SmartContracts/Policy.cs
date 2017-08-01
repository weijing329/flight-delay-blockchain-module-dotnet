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

namespace FDBC_Nethereum.SmartContracts
{
  public class Policy
  {
    private readonly ILogger _logger;

    private readonly BlockchainManager _blockchain_manager;

    private readonly BlockchainSettings _settings;

    private readonly Web3Geth _web3geth;

    private Web3Geth web3geth => _web3geth;

    public Policy(BlockchainManager blockchain_manager)
    //public Policy(Web3Geth web3geth, BlockchainSettings settings)
    {
      _blockchain_manager = blockchain_manager;
      _web3geth = blockchain_manager.Web3Geth;
      _settings = blockchain_manager.Settings;
      _logger = blockchain_manager.Logger;
    }

    public async Task<string> CreateAsync(
      string task_uuid,
      string pid, string psn,
      string tenant_id,
      string start_date_time, string end_date_time,
      string start_date_time_local, string end_date_time_local,
      BigInteger? nonce = null)
    {
      _logger.LogDebug("FDBC_Nethereum.SmartContracts.Policy.CreateAsync({task_uuid})", task_uuid);

      string sender_address = _settings.default_sender_address;
      string contract_abi = _settings.policy_contract_abi;
      string contract_bytecode = _settings.policy_contract_bytecode;

      ////====================================
      //// deploy contract
      var from = sender_address;
      var gasLimit = new HexBigInteger(4700000);
      var wei = new HexBigInteger(0);
      object[] values = new object[] {
          task_uuid,
          pid, psn,
          tenant_id,
          start_date_time, end_date_time,
          start_date_time_local, end_date_time_local
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

    public async Task<string> SetPolicyAllAttributes(
      string contract_address,
      string task_uuid,
      string start_date_time,
      string end_date_time,
      string start_date_time_local,
      string end_date_time_local,
      string status,
      string deleted,
      BigInteger? nonce = null)
    {
      // SmartContract function doesn't take null as input for string
      start_date_time = start_date_time ?? "";
      end_date_time = end_date_time ?? "";
      start_date_time_local = start_date_time_local ?? "";
      end_date_time_local = end_date_time_local ?? "";
      status = status ?? "";
      deleted = deleted ?? "";

      // Web3
      string sender_address = _settings.default_sender_address;
      string contract_abi = _settings.policy_contract_abi;

      Contract contract = web3geth.Eth.GetContract(contract_abi, contract_address);

      string task_uuid_sha3 = $"0x{_web3geth.Sha3(task_uuid)}";
      byte[] task_uuid_sha3_bytes32 = task_uuid_sha3.HexToByteArray();

      Function set_function = contract.GetFunction("set_all");

      var from = sender_address;
      var gasLimit = new HexBigInteger(4700000);
      var wei = new HexBigInteger(0);
      object[] values = new object[] {
          task_uuid,
          start_date_time,
          end_date_time,
          start_date_time_local,
          end_date_time_local,
          status,
          deleted
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
          functionInput: values);
      }

      return tx_hash;
    }

    public async Task<Tuple<string, string>> GetTransactionResult_SetPolicyAllAttributes(string tx_hash)
    {
      TransactionReceipt receipt = await GetTransactionReceiptAsync(tx_hash);

      string stringified_receipt = JsonConvert.SerializeObject(receipt);
      string stringified_event_log = "";

      return new Tuple<string, string>(stringified_receipt, stringified_event_log);
    }

    private async Task<TransactionReceipt> GetTransactionReceiptAsync(string tx_hash)
    {
      int web3_transaction_check_delay_in_ms = _settings.default_retry_in_ms;

      TransactionReceipt receipt = null;
      while (receipt == null)
      {
        await Task.Delay(web3_transaction_check_delay_in_ms);
        receipt = await web3geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(tx_hash);
      }

      return receipt;
    }

    //public async Task<Tuple<string, string>> SetPolicyAttribute(string contract_address, string task_uuid, string attribute_name, string attribute_value)
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

    //  var set_event_logs = await set_event.GetFilterChanges<EventSetPolicyAttribute>(set_event_filter_by_task_uuid);

    //  await web3geth.Eth.Filters.UninstallFilter.SendRequestAsync(set_event_filter_by_task_uuid);

    //  string stringified_receipt = JsonConvert.SerializeObject(receipt);
    //  string stringified_event_log = JsonConvert.SerializeObject(set_event_logs.FirstOrDefault());

    //  return new Tuple<string, string>(stringified_receipt, stringified_event_log);
    //}
  }

  public class EventNewPolicy
  {
    [Parameter("bytes32", "task_uuid", 1, true)]
    public byte[] task_uuid { get; set; }

    [Parameter("address", "policy_contract_address", 2, false)]
    public string policy_contract_address { get; set; }
  }

  public class EventSetPolicyAttribute
  {
    [Parameter("bytes32", "task_uuid", 1, true)]
    public byte[] task_uuid { get; set; }

    [Parameter("string", "old_val", 2, false)]
    public string old_val { get; set; }

    [Parameter("string", "new_val", 3, false)]
    public string new_val { get; set; }
  }
}
