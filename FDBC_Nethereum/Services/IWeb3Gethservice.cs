using System;
using System.Collections.Generic;
using System.Text;
using FDBC_Nethereum.SmartContracts;

namespace FDBC_Nethereum.Services
{
  public interface IWeb3GethService: IDisposable
  {
    Flight Flight { get; }
    Policy Policy { get; }
  }
}
