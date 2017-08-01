using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

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

using FDBC_Nethereum.DotNetWeb3Geth;
using FDBC_Nethereum.SmartContracts;
using FDBC_Nethereum.Config;
using System.Threading.Tasks;
using Nethereum.KeyStore;
using FDBC_Nethereum.Blockchain;

namespace FDBC_Nethereum.Services
{
  public class Web3GethService : IWeb3GethService
  {
    private readonly ILogger<Web3GethService> _logger;

    private readonly BlockchainSettings _settings;

    private readonly Web3Geth _web3geth;

    private readonly BlockchainManager _blockchain_manager;
    private readonly Flight _flight;
    private readonly Policy _policy;

    public void Dispose()
    {
    }

    public Web3GethService(IConfiguration configuration, ILogger<Web3GethService> logger)
    {
      _settings = configuration.GetSection("BlockchainSettings").Get<BlockchainSettings>();
      _logger = logger;

      string sender_address = _settings.default_sender_address;
      string password = _settings.default_sender_password;

      // you can instantiate Web3 with an Account(privateKey) which will send the transaction for you
      _web3geth = new Web3Geth(new ManagedAccount(sender_address, password), ClientFactory.GetClient(_settings.rpcapi_host));

      _blockchain_manager = new BlockchainManager(_web3geth, _settings, _logger);
      _flight = new Flight(_blockchain_manager);
      _policy = new Policy(_blockchain_manager);

      _logger.LogDebug("Initialized: Web3GethService");
    }

    public BlockchainManager BlockchainManager { get { return _blockchain_manager; } }

    public Flight Flight { get { return _flight; } }

    public Policy Policy { get { return _policy; } }
  }
}
