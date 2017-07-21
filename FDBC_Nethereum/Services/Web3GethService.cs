using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using Nethereum.Geth;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Web3.Accounts.Managed;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.RPC.Eth.Filters;
using Nethereum.ABI.FunctionEncoding.Attributes;

using Nethereum.ABI.FunctionEncoding;
using Nethereum.ABI.Model;
using Nethereum.JsonRpc.Client;

using FDBC_Nethereum.DotNetWeb3Geth;
using FDBC_Nethereum.SmartContracts;

namespace FDBC_Nethereum.Services
{
  public class Web3GethService : IWeb3GethService
  {
    private readonly ILogger<Web3GethService> _logger;

    private readonly Web3Geth _web3geth;

    private readonly Flight _flight;
    private readonly Policy _policy;

    private readonly TestingContract _testing_contract;

    private readonly string _default_sender_address;
    private readonly string _default_sender_password;

    public void Dispose()
    {
    }

    public Web3GethService(IConfiguration configuration)
    {
      _default_sender_address = "0x6376612e86c6f0be774cfa62e0eb732f87352115";
      _default_sender_password = "123";
      //_default_sender_address = "0x1b8fcde4948de04ab9d67600a145886a3544dfaa";
      //_default_sender_password = "dev_password";

      string sender_address = _default_sender_address;
      string password = _default_sender_password;

      _web3geth = new Web3Geth(new ManagedAccount(sender_address, password), ClientFactory.GetClient());

      _flight = new Flight(_web3geth);
      _policy = new Policy(_web3geth);

      _testing_contract = new TestingContract(_web3geth);
    }

    public Flight Flight { get { return _flight; } }

    public Policy Policy { get { return _policy; } }

    public TestingContract TestingContract { get { return _testing_contract; } }
  }
}
