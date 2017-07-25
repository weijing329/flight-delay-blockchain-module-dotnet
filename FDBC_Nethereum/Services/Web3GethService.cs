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
using FDBC_Nethereum.Config;

namespace FDBC_Nethereum.Services
{
  public class Web3GethService : IWeb3GethService
  {
    private readonly ILogger<Web3GethService> _logger;

    private readonly BlockchainSettings _settings;

    private readonly Web3Geth _web3geth;

    private readonly Flight _flight;
    private readonly Policy _policy;

    public void Dispose()
    {
    }

    public Web3GethService(IConfiguration configuration, ILogger<Web3GethService> logger)
    //public Web3GethService(IConfiguration configuration)
    {
      _settings = configuration.GetSection("BlockchainSettings").Get<BlockchainSettings>();
      _logger = logger;

      string sender_address = _settings.default_sender_address;
      string password = _settings.default_sender_password;

      _web3geth = new Web3Geth(new ManagedAccount(sender_address, password), ClientFactory.GetClient(_settings.rpcapi_host));

      _flight = new Flight(_web3geth, _settings, _logger);
      _policy = new Policy(_web3geth, _settings, _logger);
      //_flight = new Flight(_web3geth, _settings);
      //_policy = new Policy(_web3geth, _settings);

      _logger.LogDebug("Initialized: Web3GethService");
    }

    public Flight Flight { get { return _flight; } }

    public Policy Policy { get { return _policy; } }
  }
}
