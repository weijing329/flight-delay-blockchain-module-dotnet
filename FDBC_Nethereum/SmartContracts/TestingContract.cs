using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

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

using FDBC_Nethereum.DotNetWeb3Geth;

namespace FDBC_Nethereum.SmartContracts
{
  public class TestingContract
  {
    private readonly Web3Geth _web3geth;
    private readonly string _default_sender_account;

    public TestingContract(Web3Geth web3geth)
    {
      _web3geth = web3geth;
      _default_sender_account = "0x1b8fcde4948de04ab9d67600a145886a3544dfaa";
    }

    public async Task<EventLog<Event_Testing_sha3>> TestSha3()
    {
      var sender_address = "0x1b8fcde4948de04ab9d67600a145886a3544dfaa";
      //var password = "dev_password";

      //var web3geth = new Web3Geth(new ManagedAccount(sender_address, password), ClientFactory.GetClient());

      Web3Geth web3geth = _web3geth;

      string contract_address = "0x2b8fb416cbe7bec3aee82e2c041e6c30014068e5";
      string contract_abi = "[{'constant':false,'inputs':[{'name':'input','type':'string'}],'name':'test_sha3','outputs':[],'payable':false,'type':'function'},{'anonymous':false,'inputs':[{'indexed':true,'name':'sha3','type':'bytes32'},{'indexed':false,'name':'original','type':'string'}],'name':'event_test_sha3','type':'event'}]";
      //string contract_bytecode = "6060604052341561000c57fe5b5b6101ba8061001c6000396000f300606060405263ffffffff7c010000000000000000000000000000000000000000000000000000000060003504166351f7af76811461003a575bfe5b341561004257fe5b610090600480803590602001908201803590602001908080601f0160208091040260200160405190810160405280939291908181526020018383808284375094965061009295505050505050565b005b806040518082805190602001908083835b602083106100c25780518252601f1990920191602091820191016100a3565b51815160209384036101000a60001901801990921691161790526040805192909401829003822081835287518383015287519096507fc098fc58eeced83dc02871321df07c65ba6745c7e0066301e9214f13b70353129550879492935083928301918501908083838215610151575b80518252602083111561015157601f199092019160209182019101610131565b505050905090810190601f16801561017d5780820380516001836020036101000a031916815260200191505b509250505060405180910390a25b505600a165627a7a72305820c3615453b7e6f076f35f560d9902d9e36a448c3f8f29d4a02cbf3d6a34ad919d0029"

      ////====================================
      //// deploy contract

      // unlock for 120 secs
      //var unlockResult = await web3.Personal.UnlockAccount.SendRequestAsync(senderAddress, password, new HexBigInteger(120));
      //Assert.True(unlockResult);

      //var transactionHash = await web3.Eth.DeployContract.SendRequestAsync(abi, byteCode, senderAddress, new HexBigInteger(900000), multiplier);
      //var receipt = await GetTransactionReceiptAsync(web3, transactionHash);

      Contract contract = web3geth.Eth.GetContract(contract_abi, contract_address);

      //====================================
      // event filter

      string input = "test";
      string input_sha3 = $"0x{web3geth.Sha3(input)}";
      byte[] input_sha3_bytes32 = input_sha3.HexToByteArray();

      Event test_sha3_event = contract.GetEvent("event_test_sha3");
      // following both works
      //var test_sha3_event_filter = await test_sha3_event.CreateFilterAsync(new[] { input_sha3_bytes32 });
      var test_sha3_event_filter = await test_sha3_event.CreateFilterAsync<byte[]>(input_sha3_bytes32);

      ////====================================
      // transaction call 

      Function test_sha3_function = contract.GetFunction("test_sha3");

      var wei = new HexBigInteger(0);
      var tx_hash = await test_sha3_function.SendTransactionAsync(from: sender_address, gas: new HexBigInteger(4700000), value: wei, functionInput: new object[] { input });

      var bool_result = await web3geth.Miner.Start.SendRequestAsync(120);
      //Assert.True(result, "Mining should have started");
      ////the contract should be mining now

      //get the contract address 
      TransactionReceipt receipt = null;
      //wait for the contract to be mined to the address
      while (receipt == null)
      {
        Thread.Sleep(1000);
        receipt = await web3geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(tx_hash);
      }

      bool_result = await web3geth.Miner.Stop.SendRequestAsync();
      //Assert.True(bool_result, "Mining should have stopped");

      //====================================
      // event log

      //var logs = await web3geth.Eth.Filters.GetFilterChangesForEthNewFilter.SendRequestAsync(test_sha3_event_filter);
      //var logs_all = await web3geth.Eth.Filters.GetFilterChangesForEthNewFilter.SendRequestAsync(filter_all);
      //var logs_address = await web3geth.Eth.Filters.GetFilterChangesForEthNewFilter.SendRequestAsync(filter_address);


      var event_logs = await test_sha3_event.GetFilterChanges<Event_Testing_sha3>(test_sha3_event_filter);
      //var event_logs_all = await set_status_event.GetFilterChanges<EventSetStatus>(filter_all);
      //var event_logs_address = await set_status_event.GetFilterChanges<EventSetStatus>(filter_address);

      bool_result = await web3geth.Eth.Filters.UninstallFilter.SendRequestAsync(test_sha3_event_filter);

      return event_logs.FirstOrDefault();
    }
  }

  public class Event_Testing_sha3
  {
    [Parameter("bytes32", "sha3", 1, true)]
    public byte[] sha3 { get; set; }

    [Parameter("string", "input", 2, false)]
    public string input { get; set; }
  }
}
