using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using FDBC_Nethereum.SmartContracts;
using Nethereum.RPC.Eth.DTOs;
using System.Threading.Tasks;
using FDBC_Nethereum.Blockchain;

namespace FDBC_Nethereum.Services
{
  public interface IWeb3GethService: IDisposable
  {
    BlockchainManager BlockchainManager { get; }
    Flight Flight { get; }
    Policy Policy { get; }
  }
}
