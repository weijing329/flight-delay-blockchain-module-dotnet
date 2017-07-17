using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using Nethereum.Web3;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Threading;

namespace FDBC_Nethereum.Helpers
{
  public class Web3Helper
  {
    public static Web3 GetWeb3()
    {
      return new Web3(url: "http://172.20.10.2:28545");
    }

    public static async Task<BigInteger> GetDefaultAccountBalance()
    {
      try
      {
        var web3 = GetWeb3();
        //This can look at a local store for account addresses
        var accounts = await web3.Eth.Accounts.SendRequestAsync();

        var hex_account_blance = await web3.Eth.GetBalance.SendRequestAsync(accounts[0]);
        //return Convert.ToInt32(hex_account_blance.Value);
        return hex_account_blance.Value;
      }
      catch (Exception ex)
      {
        throw ex;
      }
    }

    public static async Task GetValue()
    {
      //contract SimpleStorage {
      //  uint public storedData;
      //
      //  function SimpleStorage(uint initialValue)
      //  {
      //    storedData = initialValue;
      //  }
      //
      //  function set(uint x)
      //  {
      //    storedData = x;
      //  }
      //
      //  function get() constant returns(uint retVal)
      //  {
      //    return storedData;
      //  }
      //
      //}

      var senderAddress = "0x64e18aa010ca6ce31b771742538d580e3f1cfb4c";
      var password = "dev_password";

      var abi = @"";
      var byteCode = "";

      var multiplier = 7;

      var web3 = GetWeb3();

      //====================================
      // deploy contract

      // unlock for 120 secs
      var unlockResult = await web3.Personal.UnlockAccount.SendRequestAsync(senderAddress, password, new HexBigInteger(120));
      //Assert.True(unlockResult);

      var transactionHash = await web3.Eth.DeployContract.SendRequestAsync(abi, byteCode, senderAddress, new HexBigInteger(900000), multiplier);
      var receipt = await GetTransactionReceiptAsync(web3, transactionHash);

      //====================================
      // transaction call 

      var contractAddress = receipt.ContractAddress;

      var contract = web3.Eth.GetContract(abi, contractAddress);

      var multiplyFunction = contract.GetFunction("multiply");

      transactionHash = await multiplyFunction.SendTransactionAsync(senderAddress, 7);
      transactionHash = await multiplyFunction.SendTransactionAsync(senderAddress, 8);

      receipt = await GetTransactionReceiptAsync(web3, transactionHash);

      //====================================
      // event

      //var multiplyEvent = contract.GetEvent("Multiplied");

      //var filterAll = await multiplyEvent.CreateFilterAsync();

      //var filter7 = await multiplyEvent.CreateFilterAsync(7);

      //var filterSender = await multiplyEvent.CreateFilterAsync(null, senderAddress);
    }

    public async Task ShouldBeAbleCallAndReadEventLogs()
    {
      //contract test {
      //  int _multiplier;
      //
      //  event Multiplied(int indexed a, address indexed sender, int result );
      //
      //  function test(int multiplier)
      //  {
      //    _multiplier = multiplier;
      //  }
      //
      //  function multiply(int a) returns(int r)
      //  {
      //    r = a * _multiplier;
      //    Multiplied(a, msg.sender, r);
      //    return r;
      //  }
      //}

      var senderAddress = "0x12890d2cce102216644c59daE5baed380d84830c";
      var password = "password";

      var abi = @"[{'constant':false,'inputs':[{'name':'a','type':'int256'}],'name':'multiply','outputs':[{'name':'r','type':'int256'}],'type':'function'},{'inputs':[{'name':'multiplier','type':'int256'}],'type':'constructor'},{'anonymous':false,'inputs':[{'indexed':true,'name':'a','type':'int256'},{'indexed':true,'name':'sender','type':'address'},{'indexed':false,'name':'result','type':'int256'}],'name':'Multiplied','type':'event'}]";

      var byteCode = "0x6060604052604051602080610104833981016040528080519060200190919050505b806000600050819055505b5060ca8061003a6000396000f360606040526000357c0100000000000000000000000000000000000000000000000000000000900480631df4f144146037576035565b005b604b60048080359060200190919050506061565b6040518082815260200191505060405180910390f35b60006000600050548202905080503373ffffffffffffffffffffffffffffffffffffffff16827f841774c8b4d8511a3974d7040b5bc3c603d304c926ad25d168dacd04e25c4bed836040518082815260200191505060405180910390a380905060c5565b91905056";

      var multiplier = 7;

      var web3 = new Web3();

      var unlockResult = await web3.Personal.UnlockAccount.SendRequestAsync(senderAddress, password, new HexBigInteger(120));
      //Assert.True(unlockResult);

      var transactionHash = await web3.Eth.DeployContract.SendRequestAsync(abi, byteCode, senderAddress, new HexBigInteger(900000), multiplier);
      var receipt = await GetTransactionReceiptAsync(web3, transactionHash);

      var contractAddress = receipt.ContractAddress;

      var contract = web3.Eth.GetContract(abi, contractAddress);

      var multiplyFunction = contract.GetFunction("multiply");

      var multiplyEvent = contract.GetEvent("Multiplied");

      var filterAll = await multiplyEvent.CreateFilterAsync();
      var filter7 = await multiplyEvent.CreateFilterAsync(7);
      var filterSender = await multiplyEvent.CreateFilterAsync(
        new MultipliedEvent()
        {
          Sender = senderAddress
        }
      );

      transactionHash = await multiplyFunction.SendTransactionAsync(senderAddress, 7);
      transactionHash = await multiplyFunction.SendTransactionAsync(senderAddress, 8);

      receipt = await GetTransactionReceiptAsync(web3, transactionHash);

      var log = await multiplyEvent.GetFilterChanges<MultipliedEvent>(filterAll);
      var log7 = await multiplyEvent.GetFilterChanges<MultipliedEvent>(filter7);

      //Assert.Equal(2, log.Count);
      //Assert.Equal(1, log7.Count);
      //Assert.Equal(7, log7[0].Event.MultiplicationInput);
      //Assert.Equal(49, log7[0].Event.Result);
    }

    //event Multiplied(int indexed a, address indexed sender, int result );

    public class MultipliedEvent
    {
      [Parameter("int", "a", 1, true)]
      public int MultiplicationInput { get; set; }

      [Parameter("address", "sender", 2, true)]
      public string Sender { get; set; }

      [Parameter("int", "result", 3, false)]
      public int Result { get; set; }

    }

    public static async Task<TransactionReceipt> GetTransactionReceiptAsync(Web3 web3, string transactionHash)
    {
      var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);

      while (receipt == null)
      {
        Thread.Sleep(1000);
        receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
      }

      return receipt;
    }

    //public async Task<TransactionReceipt> MineAndGetReceiptAsync(Web3 web3, string transactionHash)
    //{

    //  var miningResult = await web3.Miner.Start.SendRequestAsync(6);
    //  //Assert.True(miningResult);

    //  var receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);

    //  while (receipt == null)
    //  {
    //    Thread.Sleep(1000);
    //    receipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
    //  }

    //  miningResult = await web3.Miner.Stop.SendRequestAsync();
    //  //Assert.True(miningResult);
    //  return receipt;
    //}
  }
}
