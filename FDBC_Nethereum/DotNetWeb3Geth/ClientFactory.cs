using System;
using Nethereum.JsonRpc.Client;

namespace FDBC_Nethereum.DotNetWeb3Geth
{
  public class ClientFactory
  {
    public static IClient GetClient(string rpcapi_host)
    {
      return new RpcClient(new Uri(rpcapi_host));
    }
  }
}
