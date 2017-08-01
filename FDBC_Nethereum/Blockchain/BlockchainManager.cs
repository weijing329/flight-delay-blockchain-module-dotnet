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
using FDBC_Nethereum.Services;
using Nethereum.Signer;

namespace FDBC_Nethereum.Blockchain
{
  public class BlockchainManager: IDisposable
  {
    private readonly ILogger _logger;
    public ILogger Logger => _logger;

    private readonly BlockchainSettings _settings;
    public BlockchainSettings Settings => _settings;

    private readonly string _sender_private_key;
    public string sender_private_key => _sender_private_key;

    private readonly Web3Geth _web3geth;
    public Web3Geth Web3Geth => _web3geth;

    private BigInteger _inital_main_account_nonce;
    private int _new_transaction_count = 0;

    public readonly string INVALID_BLOCK_HASH = "0x0000000000000000000000000000000000000000000000000000000000000000";

    public void Dispose()
    {
    }

    public BlockchainManager(Web3Geth web3geth, BlockchainSettings settings, ILogger logger)
    {
      _web3geth = web3geth;
      _settings = settings;
      _logger = logger;

      string sender_address = _settings.default_sender_address;
      string password = _settings.default_sender_password;

      _sender_private_key = DecryptPrivateKeyFromScryptKeystore(_settings.default_sender_scrypt_keystore_json, password);

      _inital_main_account_nonce = _web3geth.Eth.Transactions.GetTransactionCount.SendRequestAsync(sender_address, BlockParameter.CreatePending()).Result.Value;

    }

    public BigInteger GetMainAccountNonceForRawTransaction
    {
      get
      {
        BigInteger nonce = _inital_main_account_nonce + _new_transaction_count;
        _new_transaction_count += 1;

        return nonce;
      }
    }

    public async Task<BlockWithTransactionHashes> GetBlockWithTransactionsHashesByHash(string block_hash)
    {
      return await _web3geth.Eth.Blocks.GetBlockWithTransactionsHashesByHash.SendRequestAsync(block_hash);
    }

    public async Task<BlockWithTransactionHashes> GetBlockWithTransactionsHashesByNumber(HexBigInteger block_number)
    {
      return await _web3geth.Eth.Blocks.GetBlockWithTransactionsHashesByNumber.SendRequestAsync(block_number);
    }

    public async Task<string> SignAndSendRawTransaction(Nethereum.Signer.Transaction transaction)
    {
      transaction.Sign(new EthECKey(_sender_private_key.HexToByteArray(), isPrivate: true));
      string signed_transaction_data = transaction.GetRLPEncoded().ToHex();
      string tx_hash = await _web3geth.Eth.Transactions.SendRawTransaction.SendRequestAsync(signed_transaction_data);

      return tx_hash;
    }

    private BigInteger GetMainAccountNonce
    {
      get
      {
        return _web3geth.Eth.Transactions.GetTransactionCount.SendRequestAsync(_settings.default_sender_address, BlockParameter.CreatePending()).Result.Value; ;
      }
    }

    public async Task<Nethereum.RPC.Eth.DTOs.Transaction> GetTransaction(string tx_hash)
    {
      return await _web3geth.Eth.Transactions.GetTransactionByHash.SendRequestAsync(tx_hash);
    }

    public async Task<TransactionReceipt> GetTransactionReceipt(string tx_hash)
    {
      return await _web3geth.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(tx_hash);
    }

    private string DecryptPrivateKeyFromScryptKeystore(string scrypt_keystore, string password)
    {
      var keyStoreScryptService = new KeyStoreScryptService();
      var keyStore = keyStoreScryptService.DeserializeKeyStoreFromJson(scrypt_keystore);
      var privateKeyDecrypted = keyStoreScryptService.DecryptKeyStore(password, keyStore);
      return privateKeyDecrypted.ToHex();
    }

  }
}
